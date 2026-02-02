Shader "Custom/PaintOverlay_Lit_Base"
{
    Properties
    {
        _PaintMask("_PaintMask", 2D) = "black" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.01
        _BaseColor("Base Color", Color) = (0,0,0,0)
    }

        SubShader
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
                "Queue" = "Geometry"
            }

            Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile _ _ADDITIONAL_LIGHTS
                #pragma multi_compile _ _SHADOWS_SOFT

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float3 normalOS   : NORMAL;
                    float2 uv         : TEXCOORD0;

                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float3 normalWS   : TEXCOORD0;
                    float3 positionWS : TEXCOORD1;
                    float2 uv         : TEXCOORD2;
                };

                TEXTURE2D(_PaintMask);
                SAMPLER(sampler_PaintMask);

                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseColor;
                float _Cutoff;
                CBUFFER_END



                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                    OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                    OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                    OUT.uv = IN.uv;
                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
                {
                    half4 paint = SAMPLE_TEXTURE2D(_PaintMask, sampler_PaintMask, IN.uv);

                    half3 normal = normalize(IN.normalWS);

                    Light mainLight = GetMainLight();
                    half NdotL = saturate(dot(normal, mainLight.direction));

                    half3 baseCol = _BaseColor.rgb;

                    // Blend paint over base
                    half3 surfaceColor = lerp(baseCol, paint.rgb, paint.a);

                    half3 direct = surfaceColor * mainLight.color * NdotL;
                    half3 ambient = SampleSH(normal) * surfaceColor;

                    return half4(direct + ambient, 1.0);
                }

                ENDHLSL
            }
        }
}

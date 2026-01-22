Shader "Custom/URP/PaintOverlay_Lit_Proof"
{
    Properties
    {
        _PaintMask("_PaintMask", 2D) = "black" {}
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.01
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
                float _Cutoff;

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

                    // Invisible where unpainted
                    clip(paint.a - _Cutoff);

                    // Main light
                    Light mainLight = GetMainLight();
                    half3 normal = normalize(IN.normalWS);
                    half NdotL = saturate(dot(normal, mainLight.direction));

                    // Direct light
                    half3 direct = paint.rgb * mainLight.color * NdotL;

                    // Ambient light
                    half3 ambient = SampleSH(normal) * paint.rgb;

                    // Combine
                    half3 litColor = direct + ambient;

                    return half4(litColor, 1.0);
                }

                ENDHLSL
            }
        }
}

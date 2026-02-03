Shader "Paint/PaintRT_Triplanar_Local"
{
    Properties
    {
        _PaintRT("Paint RenderTexture", 2D) = "black" {}
        _PaintScale("Paint Scale (Local)", Float) = 1
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }

        SubShader
        {
            Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

            Pass
            {
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float3 normalOS   : NORMAL;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float3 objectPos   : TEXCOORD0;
                    float3 objectNormal: TEXCOORD1;
                };

                TEXTURE2D(_PaintRT);
                SAMPLER(sampler_PaintRT);

                float _PaintScale;
                float4 _BaseColor;

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                    // LOCAL (object) space
                    OUT.objectPos = IN.positionOS.xyz;
                    OUT.objectNormal = IN.normalOS;

                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
                {
                    //  Proper normal (REQUIRED)
                    float3 n = abs(normalize(IN.objectNormal));

                    //  Normalised object position (matches C stamping)
                    float3 p = (IN.objectPos - _BoundsMin) / _BoundsSize;

                    float2 uvX = p.zy * _PaintScale;
                    float2 uvY = p.xz * _PaintScale;
                    float2 uvZ = p.xy * _PaintScale;

                    //  Sample paint textures
                    float px = SAMPLE_TEXTURE2D(_PaintRT, sampler_PaintRT, uvX).r;
                    float py = SAMPLE_TEXTURE2D(_PaintRT, sampler_PaintRT, uvY).r;
                    float pz = SAMPLE_TEXTURE2D(_PaintRT, sampler_PaintRT, uvZ).r;

                    //  Proper triplanar blend (NO PATCHES)
                    float w = n.x + n.y + n.z + 1e-5;
                    float paint = (px * n.x + py * n.y + pz * n.z) / w;

                    // 5 Optional confidence boost (recommended)
                    paint = saturate(paint * 1.25);

                    float3 color = lerp(_BaseColor.rgb, float3(0,0,0), paint);
                    return half4(color, 1);
                }
                ENDHLSL
            }
        }
}

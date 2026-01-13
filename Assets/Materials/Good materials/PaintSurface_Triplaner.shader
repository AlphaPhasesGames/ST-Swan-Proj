Shader "Custom/PaintSurface_Triplanar"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)

        _PaintPosX("Paint +X", 2D) = "black" {}
        _PaintNegX("Paint -X", 2D) = "black" {}
        _PaintPosY("Paint +Y", 2D) = "black" {}
        _PaintNegY("Paint -Y", 2D) = "black" {}
        _PaintPosZ("Paint +Z", 2D) = "black" {}
        _PaintNegZ("Paint -Z", 2D) = "black" {}

        _BoundsMin("Bounds Min", Vector) = (0,0,0,0)
        _BoundsSize("Bounds Size", Vector) = (1,1,1,0)
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            Pass
            {
                CGPROGRAM
                float4 _PaintPosX_TexelSize;
            float4 _PaintNegX_TexelSize;
            float4 _PaintPosY_TexelSize;
            float4 _PaintNegY_TexelSize;
            float4 _PaintPosZ_TexelSize;
            float4 _PaintNegZ_TexelSize;
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                // -------------------------------------------------
                // Helper: safe half-texel inset
                // -------------------------------------------------
                float2 SafeUV(float2 uv, float4 texelSize)
                {
                    float2 inset = texelSize.xy * 0.25;
                    //float2 inset = texelSize.xy * 0.5;
                    return clamp(uv, inset, 1.0 - inset);
                }

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 localPos : TEXCOORD0;
                    float3 localNormal : TEXCOORD1;
                };

                fixed4 _BaseColor;

                sampler2D _PaintPosX, _PaintNegX;
                sampler2D _PaintPosY, _PaintNegY;
                sampler2D _PaintPosZ, _PaintNegZ;

                float3 _BoundsMin;
                float3 _BoundsSize;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.localPos = v.vertex.xyz;
                    o.localNormal = normalize(v.normal);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // ---------------------------------------------
                    // Triplanar blend weights from normal
                    // ---------------------------------------------
                    float3 n = normalize(i.localNormal);
                    float3 w = abs(n);
                    w /= (w.x + w.y + w.z + 1e-5);

                    // ---------------------------------------------
                    // Normalize local position into 0..1
                    // ---------------------------------------------
                    float3 p = (i.localPos - _BoundsMin) / _BoundsSize;

                    // Base planar UVs
                    float2 uvX = p.zy;
                    float2 uvY = p.xz;
                    float2 uvZ = p.xy;

                    // ---------------------------------------------
                    // Sample each axis (SafeUV + mirroring)
                    // ---------------------------------------------
                    fixed4 px = (n.x >= 0)
                        ? tex2D(_PaintPosX, SafeUV(uvX, _PaintPosX_TexelSize))
                        : tex2D(_PaintNegX, SafeUV(float2(1.0 - uvX.x, uvX.y), _PaintNegX_TexelSize));

                    fixed4 py = (n.y >= 0)
                        ? tex2D(_PaintPosY, SafeUV(uvY, _PaintPosY_TexelSize))
                        : tex2D(_PaintNegY, SafeUV(float2(uvY.x, 1.0 - uvY.y), _PaintNegY_TexelSize));

                    fixed4 pz = (n.z >= 0)
                        ? tex2D(_PaintPosZ, SafeUV(uvZ, _PaintPosZ_TexelSize))
                        : tex2D(_PaintNegZ, SafeUV(float2(1.0 - uvZ.x, uvZ.y), _PaintNegZ_TexelSize));

                    // ---------------------------------------------
                    // Blended triplanar paint result
                    // ---------------------------------------------
                    fixed4 paint =
                        px * w.x +
                        py * w.y +
                        pz * w.z;

                    float a = saturate(paint.a * 1.05);
                    return lerp(_BaseColor, paint, a);
                }
                ENDCG
            }
        }
}

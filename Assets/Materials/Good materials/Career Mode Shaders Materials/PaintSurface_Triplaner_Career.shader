Shader "Custom/PaintSurface_Triplaner_Career"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0,0,0,0)

        // UV paint (single RT)
        _PaintMask("Paint Mask (UV)", 2D) = "black" {}
        _UseUVPaint("Use UV Paint (0=Tri, 1=UV)", Range(0,1)) = 0

            // Triplanar paint RTs
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
                Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
                ZWrite Off
                LOD 200

                Pass
                {
                     Blend SrcAlpha OneMinusSrcAlpha
                     ZWrite Off

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #include "UnityCG.cginc"

                    float4 _PaintPosX_TexelSize;
                    float4 _PaintNegX_TexelSize;
                    float4 _PaintPosY_TexelSize;
                    float4 _PaintNegY_TexelSize;
                    float4 _PaintPosZ_TexelSize;
                    float4 _PaintNegZ_TexelSize;

                    float4 _PaintMask_TexelSize;

                    // Helper: safe half-texel inset
                    float2 SafeUV(float2 uv, float4 texelSize)
                    {
                        float2 inset = texelSize.xy * 0.25;
                        return clamp(uv, inset, 1.0 - inset);
                    }

                    struct appdata
                    {
                        float4 vertex : POSITION;
                        float3 normal : NORMAL;
                        float2 uv     : TEXCOORD0;  // <--- IMPORTANT: mesh UVs
                    };

                    struct v2f
                    {
                        float4 pos        : SV_POSITION;
                        float3 localPos   : TEXCOORD0;
                        float3 localNormal: TEXCOORD1;
                        float2 uv         : TEXCOORD2; // <--- pass UV through
                    };

                    fixed4 _BaseColor;

                    sampler2D _PaintMask;
                    float _UseUVPaint;

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
                        o.uv = v.uv;
                        return o;
                    }

                    fixed4 frag(v2f i) : SV_Target
                    {
                        // ---- UV paint path ----
                        if (_UseUVPaint > 0.5)
                        {
                            float2 uv = SafeUV(i.uv, _PaintMask_TexelSize);
                            fixed4 paint = tex2D(_PaintMask, uv);

                            float a = saturate(paint.a * 1.05);
                            return lerp(_BaseColor, paint, a);
                        }

                    // ---- Existing triplanar path (unchanged) ----
                    float3 n = normalize(i.localNormal);
                    float3 w = abs(n);
                    w /= (w.x + w.y + w.z + 1e-5);

                    float3 p = (i.localPos - _BoundsMin) / _BoundsSize;

                    float2 uvX = p.zy;
                    float2 uvY = p.xz;
                    float2 uvZ = p.xy;

                    fixed4 px = (n.x >= 0)
                        ? tex2D(_PaintPosX, SafeUV(uvX, _PaintPosX_TexelSize))
                        : tex2D(_PaintNegX, SafeUV(float2(1.0 - uvX.x, uvX.y), _PaintNegX_TexelSize));

                    fixed4 py = (n.y >= 0)
                        ? tex2D(_PaintPosY, SafeUV(uvY, _PaintPosY_TexelSize))
                        : tex2D(_PaintNegY, SafeUV(float2(uvY.x, 1.0 - uvY.y), _PaintNegY_TexelSize));

                    fixed4 pz = (n.z >= 0)
                        ? tex2D(_PaintPosZ, SafeUV(uvZ, _PaintPosZ_TexelSize))
                        : tex2D(_PaintNegZ, SafeUV(float2(1.0 - uvZ.x, uvZ.y), _PaintNegZ_TexelSize));

                    fixed4 paint = px * w.x + py * w.y + pz * w.z;

                    float a = saturate(paint.a * 1.05);
                    return lerp(_BaseColor, paint, a);
                }
                ENDCG
            }
            }
}

Shader "Custom/PaintRT_Mask"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _PaintColor("Paint Color", Color) = (0,0,0,1)

        _PaintMask("Paint Mask (RT)", 2D) = "black" {}
        _PaintStength("Paint Strength", Float) = 0
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv     : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv  : TEXCOORD0;
                };

                float4 _BaseColor;
                float4 _PaintColor;
                sampler2D _PaintMask;
                float4 _PaintMask_ST;

                float _PaintStength; // keep your typo if you like; name must match C#

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Mask is 0..1 (black..white)
                    float m = tex2D(_PaintMask, i.uv).r;

                // Normal paint from mask
                fixed4 painted = lerp(_BaseColor, _PaintColor, saturate(m));

                // Completion override: strength=1 forces fully painted color
                return lerp(painted, _PaintColor, saturate(_PaintStength));
            }
            ENDCG
        }
        }
}

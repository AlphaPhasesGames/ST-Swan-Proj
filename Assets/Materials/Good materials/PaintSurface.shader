Shader "Custom/PaintSurface"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _PaintMask("Paint RT", 2D) = "black" {}
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _PaintMask;
            float4 _BaseColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 paint = tex2D(_PaintMask, i.uv);

            // paint.rgb = stamped colour
            // paint.a   = coverage
            return lerp(_BaseColor, paint, paint.a);
        }
        ENDCG
    }
    }
}

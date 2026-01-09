Shader "Custom/PaintRT_SurfaceColor"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _PaintTex("Paint RT", 2D) = "black" {}
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _BaseColor;
            sampler2D _PaintTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 paint = tex2D(_PaintTex, i.uv);

            // Paint overlays base using its alpha
            return fixed4(_BaseColor.rgb * (1 - paint.a) + paint.rgb, 1);
        }
        ENDCG
    }
    }
}

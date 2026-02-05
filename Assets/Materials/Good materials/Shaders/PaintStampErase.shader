Shader "Custom/PaintStampErase"
{
    Properties
    {
        _MainTex("Brush", 2D) = "white" {}
    }

        SubShader
    {
        Tags { "Queue" = "Transparent" }
        ZWrite Off
        Cull Off

        // Alpha-only, multiplicative erase
        ColorMask A
        Blend Zero OneMinusSrcAlpha

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

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float a = tex2D(_MainTex, i.uv).a;
                return fixed4(0, 0, 0, a);
            }
            ENDCG
        }
    }
}

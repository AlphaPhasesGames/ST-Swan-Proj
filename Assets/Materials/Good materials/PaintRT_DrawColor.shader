Shader "Custom/PaintRT_DrawColor"
{
    Properties
    {
        _MainTex("Brush", 2D) = "white" {}
        _Color("Paint Color", Color) = (1,1,1,1)
    }

        SubShader
        {
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            Blend One OneMinusSrcAlpha
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _Color;

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

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed a = tex2D(_MainTex, i.uv).a;

                // PREMULTIPLIED COLOR
                fixed3 rgb = _Color.rgb * a;

                return fixed4(rgb, a);
                }
                ENDCG
            }
        }
}

Shader "Custom/PaintStampWater"
{
    Properties
    {
        _MainTex("Brush", 2D) = "white" {}
        _Color("Paint Color", Color) = (1,1,1,1)
        _HardStamp("Hard Stamp", Float) = 0
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" }
            ZWrite Off
            Cull Off

            // STANDARD ALPHA BLENDING (NOT premultiplied)
            Blend SrcAlpha OneMinusSrcAlpha

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
                float4 _Color;
                float _HardStamp;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float brushAlpha = tex2D(_MainTex, i.uv).a;

                    if (_HardStamp > 0.5)
                        brushAlpha = smoothstep(0.45, 0.55, brushAlpha);

                    float finalAlpha = brushAlpha * _Color.a;

                    return fixed4(_Color.rgb, finalAlpha);
                }
                ENDCG
            }
        }
}

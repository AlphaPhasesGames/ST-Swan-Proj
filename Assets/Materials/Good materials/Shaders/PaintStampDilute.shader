Shader "Custom/PaintStampDilute"
{
    Properties
    {
        _MainTex("Brush", 2D) = "white" {}
        _Strength("Dilution Strength", Range(0,1)) = 0.3
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" }
            ZWrite Off
            Cull Off

            //Blend Zero OneMinusSrcAlpha
            Blend DstColor Zero
            BlendOp Add
            ColorMask RGB

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
                float _Strength;

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

                    float darken = 1 - pow(a, 2.2) * _Strength;

                    return fixed4(darken, darken, darken, 1);
                }

                ENDCG
            }
        }
}
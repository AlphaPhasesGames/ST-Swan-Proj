Shader "Custom/PaintStampColor"
{
    Properties
    {
        _MainTex("Brush", 2D) = "white" {}
        _Color("Paint Color", Color) = (0,0,0,1)
        _HardStamp("Hard Stamp", Float) = 0
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" }
            ZWrite Off
            Cull Off
            Blend One OneMinusSrcAlpha   // premultiplied alpha

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


                    float2 texel = fwidth(i.uv); // screen-space texel size

                    float a =
                        tex2D(_MainTex, i.uv + texel * float2(-0.5, -0.5)).a +
                        tex2D(_MainTex, i.uv + texel * float2(0.5, -0.5)).a +
                        tex2D(_MainTex, i.uv + texel * float2(-0.5,  0.5)).a +
                        tex2D(_MainTex, i.uv + texel * float2(0.5,  0.5)).a;

                            a *= 0.25; // average (downsample)

                        if (_HardStamp > 0.5)
                    {
                            // sharpen but keep sub-pixel fidelity
                             a = smoothstep(0.4, 0.6, a);
                         }

                          return fixed4(_Color.rgb * a, a);



                          /*
                         float a = tex2D(_MainTex, i.uv).a;

                      if (_HardStamp > 0.5)
                          {
                             // sharpen transition instead of binary step
                             a = smoothstep(0.45, 0.55, a);
                          }

                      // premultiplied output
                      return fixed4(_Color.rgb * a, a);*/
                  }
                  ENDCG
              }
        }
}





























/*Shader "Custom/PaintStampColor"
{
    Properties
    {
        _MainTex("Brush", 2D) = "white" {}
        _Color("Paint Color", Color) = (0,0,0,1)
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" }
            ZWrite Off
            Cull Off

            //  CORRECT FOR PREMULTIPLIED ALPHA
            Blend One OneMinusSrcAlpha

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

                // premultiplied output
                return fixed4(_Color.rgb * a, a);
            }
            ENDCG
        }
        }
}*/

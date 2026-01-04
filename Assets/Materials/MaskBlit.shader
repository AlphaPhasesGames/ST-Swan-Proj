Shader "Hidden/MaskBlit"
{
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;   // existing RT
            sampler2D _BrushTex;  // brush

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
                float baseMask = tex2D(_MainTex, i.uv).r;
                float brush = tex2D(_BrushTex, i.uv).r;
                float result = max(baseMask, brush);
                return fixed4(result, result, result, 1);
            }
            ENDCG
        }
    }
}

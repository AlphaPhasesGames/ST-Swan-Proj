Shader "Custom/PaintBlob_World"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _PaintColor("Paint Color", Color) = (0,0,0,1)
        _PaintPos("Paint Position", Vector) = (0,0,0,0)
        _PaintRadius("Paint Radius", Float) = 0.25
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _PaintColor;
            float3 _PaintPos;
            float _PaintRadius;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.worldPos, _PaintPos);
                float mask = step(dist, _PaintRadius);

                return lerp(_BaseColor, _PaintColor, mask);
            }
            ENDCG
        }
    }
}
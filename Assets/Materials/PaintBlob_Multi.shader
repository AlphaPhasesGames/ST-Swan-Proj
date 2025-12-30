Shader "Custom/PaintBlob_Multi"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _PaintColor("Paint Color", Color) = (0,0,0,1)
        _PaintRadius("Paint Radius", Float) = 0.25

        //  add this so PaintCoverageMesh can SetFloat/GetFloat it
        _PaintStength("Paint Strength", Float) = 0
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

            //  raise this to 150 (or whatever you want)
            #define MAX_PAINT_POINTS 150

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
            float _PaintRadius;

            float _PaintStength;                  //   matches your typo
            float4 _PaintPoints[MAX_PAINT_POINTS];
            int _PaintCount;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = 0;

                for (int p = 0; p < _PaintCount; p++)
                {
                    float dist = distance(i.worldPos, _PaintPoints[p].xyz);
                    mask = max(mask, step(dist, _PaintRadius));
                }

                // blob paint result
                fixed4 painted = lerp(_BaseColor, _PaintColor, mask);

                //  completion override: strength=1 forces black (or paint color)
                return lerp(painted, _PaintColor, saturate(_PaintStength));
            }
            ENDCG
        }
    }
}

Shader "Custom/PaintRT_12MaskComposite_LastOnTop"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)

        // 12 paint colours (match your wheel indices 0–11)
        _C0("C0", Color) = (1,0,0,1)
        _C1("C1", Color) = (1,0.5,0,1)
        _C2("C2", Color) = (1,1,0,1)
        _C3("C3", Color) = (0.5,1,0,1)
        _C4("C4", Color) = (0,1,0,1)
        _C5("C5", Color) = (0,1,0.5,1)
        _C6("C6", Color) = (0,1,1,1)
        _C7("C7", Color) = (0,0.5,1,1)
        _C8("C8", Color) = (0,0,1,1)
        _C9("C9", Color) = (0.5,0,1,1)
        _C10("C10", Color) = (1,0,1,1)
        _C11("C11", Color) = (1,0,0.5,1)

        // Paint masks (one per colour)
        _PaintMask0("PaintMask0", 2D) = "black" {}
        _PaintMask1("PaintMask1", 2D) = "black" {}
        _PaintMask2("PaintMask2", 2D) = "black" {}
        _PaintMask3("PaintMask3", 2D) = "black" {}
        _PaintMask4("PaintMask4", 2D) = "black" {}
        _PaintMask5("PaintMask5", 2D) = "black" {}
        _PaintMask6("PaintMask6", 2D) = "black" {}
        _PaintMask7("PaintMask7", 2D) = "black" {}
        _PaintMask8("PaintMask8", 2D) = "black" {}
        _PaintMask9("PaintMask9", 2D) = "black" {}
        _PaintMask10("PaintMask10", 2D) = "black" {}
        _PaintMask11("PaintMask11", 2D) = "black" {}

        // Index of the most recently painted colour
        _LastPaintedIndex("Last Painted Index", Int) = -1
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
                    float2 uv     : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv  : TEXCOORD0;
                };

                fixed4 _BaseColor;

                fixed4 _C0,_C1,_C2,_C3,_C4,_C5,_C6,_C7,_C8,_C9,_C10,_C11;

                sampler2D _PaintMask0,_PaintMask1,_PaintMask2,_PaintMask3,_PaintMask4,_PaintMask5;
                sampler2D _PaintMask6,_PaintMask7,_PaintMask8,_PaintMask9,_PaintMask10,_PaintMask11;

                int _LastPaintedIndex;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Base colour
                    fixed3 col = _BaseColor.rgb;

                // Sample all mask alphas
                fixed m0 = tex2D(_PaintMask0,  i.uv).a;
                fixed m1 = tex2D(_PaintMask1,  i.uv).a;
                fixed m2 = tex2D(_PaintMask2,  i.uv).a;
                fixed m3 = tex2D(_PaintMask3,  i.uv).a;
                fixed m4 = tex2D(_PaintMask4,  i.uv).a;
                fixed m5 = tex2D(_PaintMask5,  i.uv).a;
                fixed m6 = tex2D(_PaintMask6,  i.uv).a;
                fixed m7 = tex2D(_PaintMask7,  i.uv).a;
                fixed m8 = tex2D(_PaintMask8,  i.uv).a;
                fixed m9 = tex2D(_PaintMask9,  i.uv).a;
                fixed m10 = tex2D(_PaintMask10, i.uv).a;
                fixed m11 = tex2D(_PaintMask11, i.uv).a;

                // --------------------------------------------------
                // PASS 1: composite everything EXCEPT last painted
                // --------------------------------------------------
                if (_LastPaintedIndex != 0)  col = lerp(col, _C0.rgb,  m0);
                if (_LastPaintedIndex != 1)  col = lerp(col, _C1.rgb,  m1);
                if (_LastPaintedIndex != 2)  col = lerp(col, _C2.rgb,  m2);
                if (_LastPaintedIndex != 3)  col = lerp(col, _C3.rgb,  m3);
                if (_LastPaintedIndex != 4)  col = lerp(col, _C4.rgb,  m4);
                if (_LastPaintedIndex != 5)  col = lerp(col, _C5.rgb,  m5);
                if (_LastPaintedIndex != 6)  col = lerp(col, _C6.rgb,  m6);
                if (_LastPaintedIndex != 7)  col = lerp(col, _C7.rgb,  m7);
                if (_LastPaintedIndex != 8)  col = lerp(col, _C8.rgb,  m8);
                if (_LastPaintedIndex != 9)  col = lerp(col, _C9.rgb,  m9);
                if (_LastPaintedIndex != 10) col = lerp(col, _C10.rgb, m10);
                if (_LastPaintedIndex != 11) col = lerp(col, _C11.rgb, m11);

                // --------------------------------------------------
                // PASS 2: force last-painted colour on top
                // --------------------------------------------------
                if (_LastPaintedIndex == 0)  col = lerp(col, _C0.rgb,  m0);
                if (_LastPaintedIndex == 1)  col = lerp(col, _C1.rgb,  m1);
                if (_LastPaintedIndex == 2)  col = lerp(col, _C2.rgb,  m2);
                if (_LastPaintedIndex == 3)  col = lerp(col, _C3.rgb,  m3);
                if (_LastPaintedIndex == 4)  col = lerp(col, _C4.rgb,  m4);
                if (_LastPaintedIndex == 5)  col = lerp(col, _C5.rgb,  m5);
                if (_LastPaintedIndex == 6)  col = lerp(col, _C6.rgb,  m6);
                if (_LastPaintedIndex == 7)  col = lerp(col, _C7.rgb,  m7);
                if (_LastPaintedIndex == 8)  col = lerp(col, _C8.rgb,  m8);
                if (_LastPaintedIndex == 9)  col = lerp(col, _C9.rgb,  m9);
                if (_LastPaintedIndex == 10) col = lerp(col, _C10.rgb, m10);
                if (_LastPaintedIndex == 11) col = lerp(col, _C11.rgb, m11);

                return fixed4(col, 1);
            }
            ENDCG
        }
        }
}

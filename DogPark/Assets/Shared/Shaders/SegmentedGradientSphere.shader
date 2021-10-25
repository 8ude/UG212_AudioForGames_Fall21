Shader "Custom/SegmentedGradientSphere" {
    // -- editor --
    Properties {
        _StartColor ("Start Color", Color) = (1, 1, 1, 1)
        _EndColor ("End Color", Color) = (0, 0, 0, 0)
        _StartOffset ("Start Offset", Float) = 0
        _EndOffset ("End Offset", Float) = 1

        _Segments ("Segments", Int) = 8
        _Color1 ("Color1", Color) = (1, 1, 1, 1)
        _Color2 ("Color2", Color) = (0, 0, 0, 1)
        _Color3 ("Color3", Color) = (1, 1, 1, 1)
        _Color4 ("Color4", Color) = (0, 0, 0, 1)
        _Color5 ("Color5", Color) = (1, 1, 1, 1)
        _Color6 ("Color6", Color) = (0, 0, 0, 1)
        _Color7 ("Color7", Color) = (1, 1, 1, 1)
        _Color8 ("Color8", Color) = (0, 0, 0, 1)

        _BlendFactor ("Blend Factor", Float) = 0.5
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        LOD 100

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex Transform
            #pragma fragment Render

            // -- includes --
            #include "UnityCG.cginc"
            #include "./Math.cginc"

            // -- types --
            struct Vert {
                float4 vert : POSITION;
            };

            struct Frag {
                float4 proj : SV_POSITION;
                float4 vert : TEXCOORD0;
            };

            // -- props --
            int _Segments;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _Color4;
            fixed4 _Color5;
            fixed4 _Color6;
            fixed4 _Color7;
            fixed4 _Color8;

            fixed4 _StartColor;
            fixed4 _EndColor;
            float _StartOffset;
            float _EndOffset;

            float _BlendFactor;

            // -- impls --
            Frag Transform(Vert v) {
                Frag f;
                f.proj = UnityObjectToClipPos(v.vert);
                f.vert = v.vert;

                return f;
            }

            fixed4 Render(Frag f) : SV_Target {
                // calculate the gradient color
                float sample = normalize(f.vert.y, _StartOffset, _EndOffset);
                fixed4 c1 = lerp(_StartColor, _EndColor, sample);

                float longitude = atan2(f.vert.y, f.vert.z)/6.2832 + 0.5;
                int i = longitude*_Segments;
                fixed4 colors[8] = {_Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7, _Color8};
                fixed4 c2 = colors[i];

                return lerp(c1, c2, _BlendFactor);
            }

            ENDCG
        }

        // uncomment to cast shadows
        UsePass "VertexLit/SHADOWCASTER"
    }
}

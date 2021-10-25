Shader "Custom/GradientWorld" {
    // -- editor --
    Properties {
        _StartColor ("Start Color", Color) = (1, 1, 1, 1)
        _EndColor ("End Color", Color) = (0, 0, 0, 0)
        _StartOffset ("Start Offset", Float) = 0
        _EndOffset ("End Offset", Float) = 1
        [Toggle] _UseVertexColors ("Use Vertex Colors", Float) = 1
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
                float4 color : COLOR;
            };

            struct Frag {
                float4 proj : SV_POSITION;
                fixed4 color : COLOR0;
            };

            // -- props --
            fixed4 _StartColor;
            fixed4 _EndColor;
            float _StartOffset;
            float _EndOffset;
            float _UseVertexColors; // bool

            // -- impls --
            Frag Transform(Vert v) {
                Frag f;
                f.proj = UnityObjectToClipPos(v.vert);

                // calculate the gradient color
                float y = mul(unity_ObjectToWorld, float4(v.vert.xyz, 1)).y;
                float s = normalize(y, _StartOffset, _EndOffset);
                f.color = lerp(_StartColor, _EndColor, s);

                if (_UseVertexColors) {
                    // multiply in the vertex color
                    f.color *= v.color;
                }

                return f;
            }

            fixed4 Render(Frag f) : SV_Target {
                return f.color;
            }

            ENDCG
        }

        // uncomment to cast shadows
        UsePass "VertexLit/SHADOWCASTER"
    }
}

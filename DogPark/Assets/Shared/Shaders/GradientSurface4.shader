Shader "Custom/GradientSurface4" {
    // -- editor --
    Properties {
        _Color1 ("Color 1", Color) = (0, 0, 0, 1)
        _Offset1 ("Offset 1", Float) = 0
        _Color2 ("Color 2", Color) = (0.5, 0.5, 0.5, 1)
        _Offset2 ("Offset 2", Float) = 0.5
        _Color3 ("Color 3", Color) = (1, 1, 1, 1)
        _Offset3 ("Offset 3", Float) = 1
        _Color4 ("Color 4", Color) = (1, 1, 1, 1)
        _Offset4 ("Offset 4", Float) = 1
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        LOD 200

        CGPROGRAM

        // -- config --
        #pragma surface Surface Standard fullforwardshadows vertex:Transform
        #pragma target 3.0

        // -- includes --
        #include "./Math.cginc"

        // -- types --
        struct Input {
            float4 color;
        };

        // -- props --
        fixed4 _Color1;
        float _Offset1;
        fixed4 _Color2;
        float _Offset2;
        fixed4 _Color3;
        float _Offset3;
        fixed4 _Color4;
        float _Offset4;

        // -- impls --
        void Transform(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // calculate the gradient color
            float y = v.vertex.y;
            if (y < _Offset2) {
                o.color = lerp(_Color1, _Color2, normalize(y, _Offset1, _Offset2));
            } else if (y < _Offset3) {
                o.color = lerp(_Color2, _Color3, normalize(y, _Offset2, _Offset3));
            } else {
                o.color = lerp(_Color3, _Color4, normalize(y, _Offset3, _Offset4));
            }

            // multiply in the vertex color
            o.color *= v.color;
        }

        void Surface(Input i, inout SurfaceOutputStandard o) {
            fixed4 c = i.color;
            o.Albedo = c;
            o.Alpha = c.a;
        }

        ENDCG
    }

    FallBack "Standard"
}

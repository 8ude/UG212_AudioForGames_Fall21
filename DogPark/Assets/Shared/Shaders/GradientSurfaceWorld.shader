Shader "Custom/GradientSurfaceWorld" {
    // -- editor --
    Properties {
        _StartColor ("Start Color", Color) = (1, 1, 1, 1)
        _EndColor ("End Color", Color) = (0, 0, 0, 0)
        _StartOffset ("Start Offset", Float) = 0
        _EndOffset ("End Offset", Float) = 1
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
        fixed4 _StartColor;
        fixed4 _EndColor;
        float _StartOffset;
        float _EndOffset;

        // -- impls --
        void Transform(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            // calculate the gradient color
            float y = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).y;
            float s = normalize(y, _StartOffset, _EndOffset);
            o.color = lerp(_StartColor, _EndColor, s);

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

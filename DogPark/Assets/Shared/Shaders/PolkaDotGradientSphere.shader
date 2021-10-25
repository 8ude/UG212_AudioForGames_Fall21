// see https://bduvenhage.me/geometry/2019/07/31/generating-equidistant-vectors.html
Shader "Custom/PolkaDotSphere" {
    // -- editor --
    Properties {
        _NPoints ("Number of dots", Int) = 20
        _YScale ("Y Scale", Float) = 1
        _GradientScale ("GradientScale", Float) = 1
        _DotSize ("Dot size", Float) = 0.01
        _Color0 ("Background color", Color) = (1,1,1,1)

        _NColors ("Number of dot colors", Int) = 8
        _Color1 ("Color1", Color) = (0, 0, 0, 1)
        _Color2 ("Color2", Color) = (0, 0, 0, 1)
        _Color3 ("Color3", Color) = (0, 0, 0, 1)
        _Color4 ("Color4", Color) = (0, 0, 0, 1)
        _Color5 ("Color5", Color) = (0, 0, 0, 1)
        _Color6 ("Color6", Color) = (0, 0, 0, 1)
        _Color7 ("Color7", Color) = (0, 0, 0, 1)
        _Color8 ("Color8", Color) = (0, 0, 0, 1)

        _StartColor ("Start Color", Color) = (1, 1, 1, 1)
        _EndColor ("End Color", Color) = (0, 0, 0, 0)

        _BlendFactor ("Blend Factor", Float) = 0.5
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        LOD 100

        Cull Off // no backface culling so we can use this as skybox

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex Transform
            #pragma fragment Render

            // -- includes --
            #include "UnityCG.cginc"
            #include "./Math.cginc"

            #define mod(x, y) ((x) - (y) * floor((x) / float(y)))

            // -- types --
            struct Vert {
                float4 vert : POSITION;
            };

            struct Frag {
                float4 proj : SV_POSITION;
                float4 vert : TEXCOORD0;
                float4 color : COLOR0;
            };

            // -- const --
            static const float GOLDEN_ANGLE = 2.3999632297;
            static const float PI = 3.1415926536;

            // -- props --
            int _NPoints;
            float _YScale;
            float _GradientScale;
            float _DotSize;
            fixed4 _Color0;

            int _NColors;
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

            float _BlendFactor;

            // -- impls --
            Frag Transform(Vert v) {
                Frag f;
                f.proj = UnityObjectToClipPos(v.vert);
                f.vert = v.vert;

                return f;
            }


            float angle_distance(float alpha, float beta) {
                float phi = fmod(abs(alpha-beta), 2.*PI);
                float d = (phi > PI) ? ((2.*PI) - phi) : phi;
                return d;
            }

            float great_circle_distance2(float lon1, float lat1, float lon2, float lat2) {
                float long_d = angle_distance(lon1, lon2);
                return atan(sqrt(pow(cos(lat2)*sin(long_d), 2.)+pow(cos(lat1)*sin(lat2)-sin(lat1)*cos(lat2)*cos(long_d),2.))/(sin(lat1)*sin(lat2)+cos(lat1)*cos(lat2)*cos(long_d)));
            }

            float great_circle_distance(float longitude, float latitude, float longitude2, float latitude2) {
                return acos(
                    sin(latitude)*sin(latitude2)
                  + cos(latitude)*cos(latitude2)*cos(angle_distance(longitude, longitude2))
                );
            }

            fixed4 polka_color(float4 vert) {
                float phi = atan2(vert.x, vert.z); // longitude

                float r = length(vert);
                float theta = asin(vert.y); // latitude
                // ^ shouldn't that be asin(f.vert.y/r) ?

                if (angle_distance(-PI/2, theta) < _DotSize || angle_distance(PI/2, theta) < _DotSize) {
                    return _Color0; // Don't draw dots at either pole (things seem to be messed up there)
                }

                float a = 4*PI/_NPoints;      // ~area per point
                float d = sqrt(a);            // ~distance between points
                float M_theta = round(PI/d);  // num lines of latitude
                float d_theta = PI/M_theta;   // latitudinal separation
                float d_phi = a/d_theta;      // longitudinal separation (at equator)

                int i_theta = round(theta/d_theta - 0.0); // latitudinal index

                float theta_dot = PI*(i_theta+0.0)/M_theta; // latitude of nearest dot

                float M_phi = round(2*PI*cos(theta_dot)/d_phi); // number of dots at this latitude

                int i_phi = mod(round(M_phi*phi/(2*PI)), M_phi); // longitudinal index

                float phi_dot = 2*PI*i_phi/M_phi; // longitude of nearest dot

                float dist = great_circle_distance2(phi, theta, phi_dot, theta_dot); // distance to nearest dot
                if (dist<_DotSize) {
                    int i = mod(i_phi, _NColors);
                    if (i == _NColors) i = 0; // shouldn't need this..

                    fixed4 colors[8] = {_Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7, _Color8};
                    return colors[i];
                } else {
                    return _Color0;
                }
            }

            fixed4 gradient_color(float4 vert) {
                // assume y is in [-1,1]
                return lerp(_StartColor, _EndColor, (_GradientScale*vert.y)*0.5 + 0.5);
            }
            fixed4 Render(Frag f) : SV_Target {
                f.vert.y *= _YScale;
                return lerp(polka_color(f.vert), gradient_color(f.vert), _BlendFactor);
            }

            ENDCG
        }

        // uncomment to cast shadows
        UsePass "VertexLit/SHADOWCASTER"
    }
}

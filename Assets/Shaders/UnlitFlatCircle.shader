Shader "Custom/Unlit Flat Circle"
    {
    Properties
        {
            _Color("Color", Color) = (0,0,0,0)
            _Center("Center", Vector) = (0,0,0)
            _Radius("Radius", Float) = 1
            _RadiusWidth("Radius Width", Float) = 2
        }
        SubShader
                {
                    Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Unlit" }
                    Cull Off
                    Lighting Off
                    ZWrite Off
                    Material { Emission[_Color] }
                    Color[_Color]
                    Blend SrcAlpha OneMinusSrcAlpha
                    AlphaTest Greater .001
                    LOD 200

                    CGPROGRAM
                    #pragma surface surf Standard alpha:blend

                    fixed4 _Color;
                    float3 _Center;
                    float _Radius;
                    float _RadiusWidth;

                    struct Input
                    {
                        float3 worldPos;	// The in-world position
                    };

                    void surf(Input IN, inout SurfaceOutputStandard o)
                    {
                        float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                        float d = distance(_Center, localPos);
                        if (d > _Radius && d < _Radius + _RadiusWidth)
                            d = distance(_Center, localPos);

                        float3 xBasis = float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20);
                        float3 zBasis = float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22);
                        float scale = sqrt(length(cross(xBasis, zBasis)));
                        if (d > _Radius * scale - _RadiusWidth && d < _Radius * scale)
                            {
                            o.Albedo = _Color;
                            o.Emission = _Color.rgb;
                            o.Alpha = _Color.a;
                            }
                        else
                            {
                            o.Albedo = float4(0, 0, 0, 0);
                            o.Emission = float3(0, 0, 0);
                            o.Alpha = 0;
                            //discard;
                            }

                    }
                    ENDCG
                }
                FallBack "Transparent"
    }
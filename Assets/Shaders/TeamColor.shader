Shader "Custom/TeamColor"
    {
    Properties
        {
            _MainTex("Albedo (RGB)", 2D) = "white" {}
            _Color("Color", Color) = (1,1,1,1)
            _Glossiness("Smoothness", Range(0,1)) = 0.0
            _Metallic("Metallic", Range(0,1)) = 0.0
            _TeamColorMap("TeamColorMap (B/W)", 2D) = "white" {}
            _TeamColor("TeamColor", Color) = (1,1,1,1)
        }
        SubShader
                {
                    Tags { "RenderType" = "Opaque" }
                    LOD 200

                    CGPROGRAM
                // Physically based Standard lighting model, and enable shadows on all light types
                #pragma surface surf Standard fullforwardshadows

                // Use shader model 3.0 target, to get nicer looking lighting
                #pragma target 3.0

                sampler2D _MainTex;
                sampler2D _TeamColorMap;

                struct Input
                {
                    float2 uv_MainTex;
                    float2 uv_TeamColorMap;
                };

                half _Glossiness;
                half _Metallic;
                fixed4 _Color;
                fixed4 _TeamColor;

                // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
                // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
                // #pragma instancing_options assumeuniformscaling
                UNITY_INSTANCING_BUFFER_START(Props)
                    // put more per-instance properties here
                UNITY_INSTANCING_BUFFER_END(Props)

                void surf(Input IN, inout SurfaceOutputStandard o)
                {
                    // Albedo comes from a texture tinted by color
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    fixed4 m = tex2D(_TeamColorMap, IN.uv_TeamColorMap);

                    if (m.r > 0.5)
                        {
                        c.rgb = c.rgb * _TeamColor;
                        }

                    o.Albedo = c.rgb;
                    // Metallic and smoothness come from slider variables
                    o.Metallic = _Metallic;
                    o.Smoothness = _Glossiness;
                    o.Alpha = c.a;
                }
                ENDCG
                }
                FallBack "Diffuse"
    }

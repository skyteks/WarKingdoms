Shader "Custom/FogOfWar Negative Shader" {
    Properties{
        _MainTex("RenderTexture", 2D) = "black" {}
        _Alpha("Smoothness", Range(0,1)) = 1
        }

        SubShader{
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane"}
            Lighting Off
            ZTest Off

            //Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
            //Blend One OneMinusSrcAlpha // Premultiplied transparency
            Blend One One // Additive
            //Blend OneMinusDstColor One // Soft Additive
            //Blend DstColor Zero // Multiplicative
            //Blend DstColor SrcColor // 2x Multiplicative

            CGPROGRAM
            #pragma surface surf Lambert alpha

            sampler2D _MainTex;
            half _Alpha;

            struct Input {
                float2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutput o) {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = fixed3(0, 0, 0);
                o.Alpha = (1 - c.r) * _Alpha;
            }
            ENDCG
            }

            Fallback "Transparent/VertexLit"
    }
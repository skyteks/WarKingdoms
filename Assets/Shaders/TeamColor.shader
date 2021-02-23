Shader "Custom/TeamColor"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        _Glossiness("Smoothness", Range(0,1)) = 0.0
        _Metallic("Metallic", Range(0,1)) = 0.0
        _TeamColorMap("TeamColorMap (B/W)", 2D) = "black" {}
        _TeamColor("TeamColor", Color) = (1,1,1,1)
        _TeamColorCutoff("TeamColor cutoff", Range(0,1)) = 0.5
        _Mode("Shader Mode", Range(0,2.99)) = 1

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        CGPROGRAM

        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _TeamColorMap;
        float _Cutoff;
        float _Glossiness;
        float _Metallic;
        float _TeamColorCutoff;
        uint _Mode;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_TeamColorMap;
        };


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _TeamColor)
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 BlendOverlay( float3 a, float3 b )
        {
            return a < 0.5 ? 2 *a * b : 1-2 *(1-a) * (1-b);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed4 m = tex2D(_TeamColorMap, IN.uv_TeamColorMap);

            float3 teamColor = UNITY_ACCESS_INSTANCED_PROP(Props, _TeamColor).rgb;
            if (_Mode == 0)// multiply
            {
                if (m.r > _TeamColorCutoff)
                {
                    c.rgb = c.rgb * teamColor;
                }
            }
            else if (_Mode == 1) // 2x multiply
            {
                if (m.r > _TeamColorCutoff)
                {
                    c.rgb = 2 * c.rgb * lerp( 1, teamColor, m.r );
                }
            }
            else if (_Mode == 2) // 0.75 blend
            {
                c.rgb = lerp( c.rgb, BlendOverlay(c.rgb * 0.75, teamColor), m.r );
            }

            if (c.a > _Cutoff)
            {
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = clamp(c.a,0,1);
            }
            else
            {
                discard;
            }
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}
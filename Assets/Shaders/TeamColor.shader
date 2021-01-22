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
        fixed4 _Color;
        fixed4 _TeamColor;

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
                //o.Albedo = fixed3(0,0,0);
                //o.Metallic = 0;
                //o.Smoothness = 0;
                //o.Alpha = 0;
            }
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}
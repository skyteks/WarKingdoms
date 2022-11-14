Shader "Custom/Tint"
{
    Properties
    {
        [MainTexture] _MainTex("Albedo (RGB)", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5

        _TintMaskMap("Tint Mask (B/W)", 2D) = "black" {}
        _TintRColor("Tint (R Channel)", Color) = (1,1,1,1)
        _TintGColor("Tint (G Channel)", Color) = (1,1,1,1)
        _TintBColor("Tint (B Channel)", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        CGPROGRAM

        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma target 3.0
        #pragma instancing_options assumeuniformscaling

        sampler2D _MainTex;
        sampler2D _TintMaskMap;
        float _Cutoff;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_TintMaskMap;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintRColor)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintGColor)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintBColor)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            fixed4 m = tex2D(_TintMaskMap, IN.uv_TintMaskMap);

            float3 tintR = UNITY_ACCESS_INSTANCED_PROP(Props, _TintRColor).rgb;
            float3 tintG = UNITY_ACCESS_INSTANCED_PROP(Props, _TintGColor).rgb;
            float3 tintB = UNITY_ACCESS_INSTANCED_PROP(Props, _TintBColor).rgb;

            c.rgb = lerp(c.rgb, c.rgb * tintR, m.r);
            c.rgb = lerp(c.rgb, c.rgb * tintG, m.g);
            c.rgb = lerp(c.rgb, c.rgb * tintB, m.b);

            if (c.a > _Cutoff)
            {
                o.Albedo = c.rgb;
                o.Alpha = saturate(c.a);
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
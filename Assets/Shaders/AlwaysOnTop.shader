Shader "Custom/AlwaysOnTop"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Base", 2D) = "white" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;

    half4 _MainTex_ST;

    UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
    UNITY_INSTANCING_BUFFER_END(Props)

    struct v2f {
        half4 pos : SV_POSITION;
        half2 uv : TEXCOORD0;
        fixed4 vertexColor : COLOR;
    };

    v2f vert(appdata_full v)
    {
        v2f o;

        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.vertexColor = v.color * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

        return o;
    }

    fixed4 frag(v2f i) : COLOR
    {
        return tex2D(_MainTex, i.uv.xy) * i.vertexColor;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100" "PreviewType" = "Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma instancing_options assumeuniformscaling

        ENDCG
        }

    }
    FallBack Off
}
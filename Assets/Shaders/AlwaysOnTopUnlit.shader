
Shader "Custom/AlwaysOnTopUnlit" {

    Properties{
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Particle Texture", 2D) = "white" {}
    }

        SubShader{
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            Tags { "LightMode" = "Vertex" }
            Cull Off
            Lighting Off
            Material { Emission[_Color] }
            ZWrite Off
            Color [_Color]
            Blend SrcAlpha OneMinusSrcAlpha
            AlphaTest Greater .001
            Pass {
                SetTexture[_MainTex] { combine primary * texture }
            }
    }
}
#ifndef TINT_FORWARD_PASS
#define TINT_FORWARD_PASS

#include "Assets/ShaderLibrary/RenderUtils.hlsl"

uniform sampler2D _MainTex;
uniform float4  _MainTex_ST;
uniform float _Cutoff;
uniform sampler2D _TintMaskMap;

// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
// put more per-instance properties here
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TintRColor)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TintGColor)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TintBColor)
UNITY_INSTANCING_BUFFER_END(Props)

RasterizerData vert (VertexData input)
{
    RasterizerData output = (RasterizerData) 0;
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangetOS);
    
    InitializeRasterizerData (input, vertexInput, normalInput, output);
    output.uv1.xy = input.uv1;
    output.uv1.zw = input.uv2;
    output.uv2.xy = input.uv3;
    output.uv2.zw = input.uv4;
    
    return output;
}

float4 frag (RasterizerData input) : SV_Target
{
    float4 c = tex2D(_MainTex, input.uv1.xy) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
    clip(c.w - _Cutoff);
    float4 m = tex2D(_TintMaskMap, input.uv1.xy);

    float3 tintR = UNITY_ACCESS_INSTANCED_PROP(Props, _TintRColor).rgb;
    float3 tintG = UNITY_ACCESS_INSTANCED_PROP(Props, _TintGColor).rgb;
    float3 tintB = UNITY_ACCESS_INSTANCED_PROP(Props, _TintBColor).rgb;

    c.rgb = lerp(c.rgb, c.rgb * tintR, m.r);
    c.rgb = lerp(c.rgb, c.rgb * tintG, m.g);
    c.rgb = lerp(c.rgb, c.rgb * tintB, m.b);

    float4 render = float4(c.xyz, 1);

    return render;
}
#endif
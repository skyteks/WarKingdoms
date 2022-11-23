#ifndef FIELDOFVIEW_FORWARD_PASS
#define FIELDOFVIEW_FORWARD_PASS

#include "Assets/ShaderLibrary/RenderUtils.hlsl"

uniform float _RadiusWidth;
uniform float _Smoothness;

// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
// put more per-instance properties here
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(Props)

RasterizerData vert (VertexData input)
{
    RasterizerData output = (RasterizerData) 0;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangetOS);
    
    InitializeRasterizerData (input, vertexInput, normalInput, output);
    output.uv1.xy = input.positionOS.xz;
    output.uv1.zw = input.uv2;
    output.uv2.xy = input.uv3;
    output.uv2.zw = input.uv4;
    
    return output;
}

float4 frag (RasterizerData input) : SV_Target
{
    float circle = length(input.uv1.xy);
    //clip(1 - circle);
    circle = 1 - abs(1 + (circle - 1) / (_RadiusWidth * 0.5));
    circle /= _Smoothness;
    circle = saturate(circle);
    return float4(0,0,0,0);
}
#endif
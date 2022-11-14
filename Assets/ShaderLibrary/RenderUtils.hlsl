#ifndef RENDER_UTILS
#define RENDER_UTILS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct VertexData
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangetOS     : TANGENT;
    float2 uv1          : TEXCOORD0;
    float2 uv2          : TEXCOORD1;
    float2 uv3          : TEXCOORD2;
    float2 uv4          : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct RasterizerData
{
    float4 positionCS               : SV_POSITION;
    float4 positionWS               : TEXCOORD0;
    float4 uv1                      : TEXCOORD1;
    float4 uv2                      : TEXCOORD2;
    float4 screenCoords             : TEXCOORD3;
    float4 normalWS                 : NORMAL;
    float4 tangentWS                : TANGENT;
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD4;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float w             : TEXCOORD5;
    float4 positionNDC : TEXCOORD6;
};

void InitializeRasterizerData(VertexData vertexData, VertexPositionInputs vertexInput, const VertexNormalInputs normalInput, out RasterizerData output)
{
    UNITY_SETUP_INSTANCE_ID(vertexData);
    UNITY_TRANSFER_INSTANCE_ID(vertexData, output);
    
    output.screenCoords = ComputeScreenPos(vertexInput.positionCS);
    output.w = -vertexInput.positionVS.z;
    output.positionCS    = vertexInput.positionCS;
    output.positionNDC = vertexInput.positionNDC;
    output.positionWS    = float4 (vertexInput.positionWS ,                                                    normalInput.tangentWS.z);
    output.normalWS      = float4 (normalInput.tangentWS.x, normalInput.bitangentWS.x, normalInput.normalWS.x, normalInput.bitangentWS.z);
    output.tangentWS     = float4 (normalInput.tangentWS.y, normalInput.bitangentWS.y, normalInput.normalWS.y, normalInput.normalWS.z);

    output.uv1 = float4 (0,0,0,0);
    output.uv2 = float4 (0,0,0,0);
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
}

#endif
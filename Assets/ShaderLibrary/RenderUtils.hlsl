#ifndef RENDER_UTILS
#define RENDER_UTILS

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

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

struct GeometryData
{
    float3 positonWS;
    float3 normalWS;
    float3 tangentWS;
    float3 bitangentWS;
    float2 positionVS;
    float3 normalPerturbedWS;
    float3 view;
    float sceneDepth;
    float depth;
    float depthDelta;
    float screenDither;
    float3 sceneWorld;
    float sceneCameraDistance;
};

struct GlobalIlluminationData
{
    half occlusion;
    half3 diffuseGI;
};

static float4x4 ditherMatrix = float4x4(
    1.0,  9.0,  3.0, 11.0,
    13.0,  5.0, 15.0,  7.0,
    4.0, 12.0,  2.0, 10.0,
    16.0,  8.0, 14.0,  6.0) / 17.0;

float ScreenSpaceDither (float2 positionVS)
{
    float2 screenPos = positionVS.xy * _ScreenParams.xy;
    screenPos %= 4;
    return ditherMatrix[screenPos.x][screenPos.y];
}

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

float SampleLinearDepth(float2 uv)
{
    float depth = SampleSceneDepth(uv + 0.25 /_ScreenParams.xy); 
    return LinearEyeDepth(depth, _ZBufferParams); 
}

void InitializeGeometryData (RasterizerData i, float3 normalTS, out GeometryData outGeometryData)
{
    outGeometryData.positonWS = i.positionWS.xyz;
    outGeometryData.normalWS = normalize (float3 (i.normalWS.z, i.tangentWS.z, i.tangentWS.w));
    #ifdef _NORMALMAP
    float3x3 tangentToWorld = float3x3 (i.normalWS.xyz, i.tangentWS.xyz, float3 (i.positionWS.w, i.normalWS.w, i.tangentWS.w));
    outGeometryData.normalPerturbedWS = normalize (mul(tangentToWorld, normalTS));
    outGeometryData.tangentWS = float3(0,0,0);
    outGeometryData.bitangentWS = float3(0,0,0);
    #else
    outGeometryData.normalPerturbedWS = outGeometryData.normalWS;
    outGeometryData.tangentWS = float3(0,0,0);
    outGeometryData.bitangentWS = float3(0,0,0);
    #endif

    outGeometryData.view = normalize(_WorldSpaceCameraPos - outGeometryData.positonWS);
    outGeometryData.positionVS = i.screenCoords.xy/i.screenCoords.w;
    outGeometryData.sceneDepth = SampleLinearDepth(outGeometryData.positionVS);
    outGeometryData.depth = i.w;
    outGeometryData.depthDelta = outGeometryData.sceneDepth - outGeometryData.depth;

    outGeometryData.screenDither = ScreenSpaceDither(outGeometryData.positionVS.xy);

    float2 UV = GetNormalizedScreenSpaceUV(i.positionCS)*2;
    
    #if UNITY_REVERSED_Z
    float deep = SampleSceneDepth(UV);
    #else
    // Adjust z to match NDC for OpenGL
    float deep = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
    #endif
    
    outGeometryData.sceneWorld = ComputeWorldSpacePosition(UV, deep, UNITY_MATRIX_I_VP);
    outGeometryData.sceneCameraDistance = length(outGeometryData.sceneWorld-_WorldSpaceCameraPos);
}

void InitializeGlobalIlluminationData (GeometryData geometryData, half occlusion, out GlobalIlluminationData outGI)
{
    outGI.occlusion = occlusion;
    outGI.diffuseGI = SampleSH (geometryData.normalPerturbedWS);
}

#endif
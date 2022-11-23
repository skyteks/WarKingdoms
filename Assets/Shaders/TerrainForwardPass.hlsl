#ifndef TERRAIN_FORWARD_PASS
#define TERRAIN_FORWARD_PASS

#include "Assets/ShaderLibrary/RenderUtils.hlsl"
#include "Assets/ShaderLibrary/FogOfWar.hlsl"

uniform sampler2D _MainTex;
uniform float4  _MainTex_ST;
uniform float _Cutoff;

// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
// put more per-instance properties here
UNITY_INSTANCING_BUFFER_END(Props)

RasterizerData vert (VertexData input)
{
    RasterizerData output = (RasterizerData) 0;
    
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangetOS);
    
    InitializeRasterizerData (input, vertexInput, normalInput, output);
    output.uv1.xy = input.uv1 * _MainTex_ST.xy + _MainTex_ST.zw;
    output.uv1.zw = GetFowCoords(vertexInput.positionWS);
    output.uv2.xy = input.uv3;
    output.uv2.zw = input.uv4;
    
    return output;
}

float4 frag (RasterizerData input) : SV_Target
{
    //sample texture and tint-mask
    float4 albedo = tex2D(_MainTex, input.uv1.xy);

    float metallic = 0;
    float3 specular = float3(0,0,0);
    float smoothness = 0;
    float alpha = 1;
    float3 normalTS = float3(0,0,1);

    //setup render structure
    //surface data
    BRDFData brdfData;
    InitializeBRDFData(albedo.rgb, metallic, specular, smoothness, alpha, brdfData);

    //geometry data
    GeometryData geometryData;
    InitializeGeometryData(input, normalTS, geometryData);

    //global illumination data
    GlobalIlluminationData globalIlluData;
    InitializeGlobalIlluminationData(geometryData, 1, globalIlluData);

    //get light source
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    Light mainLight = GetMainLight(input.shadowCoord);
    #else
    Light mainLight = GetMainLight();
    #endif

    //render stuff starts here
    float3 render = float3(0,0,0);

    //add global illumination
    render += GlobalIllumination(brdfData, globalIlluData.diffuseGI, globalIlluData.occlusion, geometryData.normalWS, geometryData.view);
    
    //add main light
    render += LightingPhysicallyBased(brdfData, mainLight, geometryData.normalWS, geometryData.view);

    //add additional lights
    #ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, geometryData.positonWS);
        render += LightingPhysicallyBased(brdfData, light, geometryData.normalPerturbedWS, geometryData.view);
    }
    #endif

    //apply Fog of War
    render = ApplyFow(render, input.uv1.zw);

    return float4(render, 1);
}
#endif
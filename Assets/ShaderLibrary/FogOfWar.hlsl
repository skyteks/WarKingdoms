#ifndef FOGOFWAR
#define FOGOFWAR

uniform float4x4 _WorldToFow;
uniform sampler2D _FowTexture;

float2 GetFowCoords (float3 worldPos)
{
	return mul(_WorldToFow, float4(worldPos, 1)).xz;
}

float3 ApplyFow (float3 render, float2 fowCoords)
{
	float4 tex_fow = tex2D(_FowTexture, fowCoords);
	render = lerp(render, float3(0,0,0), tex_fow.r * 0.5);
	render = lerp(render, float3(0,0,0), tex_fow.g);
	return render;
}

#endif
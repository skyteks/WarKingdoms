Shader "Hovl/Particles/Add_Fresnel"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		_Emission("Emission", Float) = 2
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_Flow("Flow", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Distortionpower("Distortion power", Float) = 0.2
		_Fresnelscale("Fresnel scale", Float) = 3
		_Fresnelpower("Fresnel power", Float) = 3
		_Depthpower("Depth power", Float) = 0.2
		[Toggle]_Useonlycolor("Use only color", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				//#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float3 ase_normal : NORMAL;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
					//float4 ase_texcoord5 : TEXCOORD5;
				};
				
				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform sampler2D_float _CameraDepthTexture;
				uniform float _Useonlycolor;
				uniform float4 _SpeedMainTexUVNoiseZW;
				uniform sampler2D _Mask;
				uniform float4 _Mask_ST;
				uniform sampler2D _Flow;
				uniform float4 _Flow_ST;
				uniform float _Distortionpower;
				uniform sampler2D _Noise;
				uniform float4 _Noise_ST;
				uniform float4 _Color;
				uniform float _Emission;
				uniform float _Fresnelscale;
				uniform float _Fresnelpower;
				uniform float _Depthpower;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord4.xyz = ase_worldNormal;
					float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
					float4 screenPos = ComputeScreenPos(ase_clipPos);
					//o.ase_texcoord5 = screenPos;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i , half ase_vface : VFACE ) : SV_Target
				{
					float fade = 1;
					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						fade = saturate (_Depthpower * (sceneZ-partZ));
					#endif
					
					float2 appendResult186 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
					float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float2 uv_Mask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
					float2 appendResult177 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
					float3 uv_Flow = i.texcoord.xyz;
					uv_Flow.xy = i.texcoord.xyz.xy * _Flow_ST.xy + _Flow_ST.zw;
					float4 tex2DNode203 = tex2D( _MainTex, ( ( ( appendResult186 * _Time.y ) + uv_MainTex ) - ( (( tex2D( _Mask, uv_Mask ) * tex2D( _Flow, ( ( _Time.y * appendResult177 ) + (uv_Flow).xy ) ) )).rg * _Distortionpower ) ) );
					float2 uv_Noise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
					float4 tex2DNode211 = tex2D( _Noise, uv_Noise );
					float w199 = (1.0 + (uv_Flow.z - 0.0) * (128.0 - 1.0) / (1.0 - 0.0));
					float4 temp_cast_3 = (tex2DNode203.a).xxxx;
					float div207=256.0/float((int)w199);
					float4 posterize207 = ( floor( temp_cast_3 * div207 ) / div207 );
					float opac215 = (posterize207).a;
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 ase_worldNormal = i.ase_texcoord4.xyz;
					float fresnelNdotV187 = dot( ase_worldNormal, ase_worldViewDir );
					float fresnelNode187 = ( 0.0 + _Fresnelscale * pow( 1.0 - fresnelNdotV187, _Fresnelpower ) );
					float clampResult193 = clamp( fresnelNode187 , 0.0 , 1.0 );
					float switchResult206 = (((ase_vface>0)?(clampResult193):(0.0)));
					float clampResult202 = clamp( fade , 0.0 , 1.0 );			
					float clampResult214 = clamp( ( (1.0 + (clampResult202 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) - switchResult206 ) , 0.0 , 1.0 );
					float4 appendResult224 = (float4(( lerp(float4( (( tex2DNode203 * tex2DNode211 * _Color * i.color )).rgb , 0.0 ),_Color,_Useonlycolor) * _Emission ).rgb , ( opac215 * tex2DNode211.a * _Color.a * i.color.a * ( switchResult206 + clampResult214 ) )));
					fixed4 col = appendResult224;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
}

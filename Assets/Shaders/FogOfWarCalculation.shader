Shader "Custom/FogOfWarCalculation"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "black" {}
	}

	SubShader
	{
		Tags{ "Queue"="Transparent+100" }

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Equal

		CGPROGRAM

		//#pragma vertex vert
		//#pragma fragment frag
		#pragma surface surf Standard alpha:blend

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos; // The in-world position
		};

		sampler2D _MainTex;

		int2 _TerrainSize;

		int _UnitPositionsCount;
		float4 _UnitPositionsAndRange[1024*2];

		/*
		v2f vert(appdata v)
		{
			v2f o;

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;

			//o.uv1 = v.uv.xy * unity_LightmapST.xy + unity_LightmapST.zw;// lightmap uvs

			return o;
		}

		float4 frag(v2f o) : COLOR
		{
			float4 main_color = tex2D(_MainTex, o.uv); // main texture
			return main_color;
		}
		*/

		float VectorMagnitude(float3 vector3)
		{
			return (vector3.x * vector3.x + vector3.y * vector3.y + vector3.z * vector3.z);
		}

		float VectorMagnitude(float2 vector2)
		{
			return (vector2.x * vector2.x + vector2.y * vector2.y);
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float4 color = float4(0,0,0,1);
			float height = tex2D(_MainTex, IN.uv_MainTex).a;
		    //float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

			for (int i = 0; i < _UnitPositionsCount; i++)
			{
				float2 position = _UnitPositionsAndRange[i].xz;
				float unitHeight = _UnitPositionsAndRange[i].y;
				float visionRange =_UnitPositionsAndRange[i].w;

				if (VectorMagnitude(position - IN.worldPos.xz) < visionRange * 10)
				{
					color = float4(1,1,1,0);
				}
			}

			o.Albedo = color.rgb;
		    o.Alpha = saturate(color.a);
		}

		ENDCG
	}
    FallBack "Transparent"
}
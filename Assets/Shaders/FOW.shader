Shader "Custom/FOW"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent+100" } // to cover other transparent non-z-write things

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Equal

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			float4x4 unity_Projector;
			sampler2D _MainTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = mul(unity_Projector, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 color = tex2Dproj(_MainTex, i.uv);
                color.a = max(0, color.a);
				return color;
			}
			ENDCG
		}
	}
}
// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hovl/Particles/VolumeLaser"
{
	Properties
	{
		[HideInInspector]_StartPoint("StartPoint", Vector) = (0,1,0,0)
		_StartDistance("Start Distance", Float) = 2
		_StartRound("Start Round", Float) = 6
		[Toggle]_UseEndRound("Use End Round", Float) = 1
		[HideInInspector]_EndPoint("EndPoint", Vector) = (-10,1,0,0)
		_EndDistance("End Distance", Float) = 2
		_EndRound("End Round", Float) = 6
		_Distance("Distance", Float) = 10
		_MainTex("MainTex", 2D) = "white" {}
		_DissolveNoise("Dissolve Noise", 2D) = "white" {}
		_MainTexTilingXYNoiseTilingZW("MainTex Tiling XY Noise Tiling ZW", Vector) = (1,1,1,1)
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_Emission("Emission", Float) = 2
		_Color("Color", Color) = (1,1,1,1)
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Dissolve("Dissolve", Range( 0 , 1)) = 1
		_VertexPower("Vertex Power", Float) = 0.3
		_TextureVertexPower("Texture Vertex Power", Float) = 0.2
		[HideInInspector]_Scale("Scale", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
		};

		uniform float4 _StartPoint;
		uniform float _StartDistance;
		uniform float _StartRound;
		uniform float _UseEndRound;
		uniform float4 _EndPoint;
		uniform float _EndDistance;
		uniform float _EndRound;
		uniform sampler2D _MainTex;
		uniform float4 _MainTexTilingXYNoiseTilingZW;
		uniform float4 _SpeedMainTexUVNoiseZW;
		uniform float _TextureVertexPower;
		uniform float _VertexPower;
		uniform float _Scale;
		uniform float4 _Color;
		uniform float _Emission;
		uniform float _Distance;
		uniform float _Dissolve;
		uniform sampler2D _DissolveNoise;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float temp_output_3_0 = distance( _StartPoint , float4( ase_worldPos , 0.0 ) );
			float StartPoint83 = temp_output_3_0;
			float clampResult10 = clamp( ( (0.0 + (StartPoint83 - 0.0) * (-1.0 - 0.0) / (1.0 - 0.0)) + _StartDistance ) , 0.0 , _StartDistance );
			float myVarName106 = distance( float4( ase_worldPos , 0.0 ) , _EndPoint );
			float clampResult109 = clamp( ( (0.0 + (myVarName106 - 0.0) * (-1.0 - 0.0) / (1.0 - 0.0)) + _EndDistance ) , 0.0 , _EndDistance );
			float temp_output_15_0 = max( pow( (0.0 + (clampResult10 - 0.0) * (1.0 - 0.0) / (_StartDistance - 0.0)) , _StartRound ) , lerp(0.0,pow( (0.0 + (clampResult109 - 0.0) * (1.0 - 0.0) / (_EndDistance - 0.0)) , _EndRound ),_UseEndRound) );
			float2 appendResult46 = (float2(_MainTexTilingXYNoiseTilingZW.x , _MainTexTilingXYNoiseTilingZW.y));
			float2 appendResult57 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			float2 appendResult40 = (float2(v.texcoord.xy.x , temp_output_3_0));
			float2 panner48 = ( 1.0 * _Time.y * appendResult57 + appendResult40);
			float4 tex2DNode32 = tex2Dlod( _MainTex, float4( ( appendResult46 * panner48 ), 0, 0.0) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			v.vertex.xyz += ( ( ( 1.0 - ( temp_output_15_0 + ( tex2DNode32.r * ( 1.0 - temp_output_15_0 ) * _TextureVertexPower ) ) ) * 2.0 ) * ase_worldNormal * _VertexPower * _Scale );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 appendResult46 = (float2(_MainTexTilingXYNoiseTilingZW.x , _MainTexTilingXYNoiseTilingZW.y));
			float2 appendResult57 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			float3 ase_worldPos = i.worldPos;
			float temp_output_3_0 = distance( _StartPoint , float4( ase_worldPos , 0.0 ) );
			float2 appendResult40 = (float2(i.uv_texcoord.x , temp_output_3_0));
			float2 panner48 = ( 1.0 * _Time.y * appendResult57 + appendResult40);
			float4 tex2DNode32 = tex2D( _MainTex, ( appendResult46 * panner48 ) );
			o.Emission = ( _Color * i.vertexColor * _Emission * tex2DNode32 ).rgb;
			o.Alpha = 1;
			float StartPoint83 = temp_output_3_0;
			float2 _Vector0 = float2(0,1);
			float ifLocalVar82 = 0;
			if( StartPoint83 >= _Distance )
				ifLocalVar82 = _Vector0.x;
			else
				ifLocalVar82 = _Vector0.y;
			float2 appendResult94 = (float2(_MainTexTilingXYNoiseTilingZW.z , _MainTexTilingXYNoiseTilingZW.w));
			float2 appendResult122 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
			float2 panner123 = ( 1.0 * _Time.y * appendResult122 + appendResult40);
			float clampResult101 = clamp( ( tex2D( _DissolveNoise, ( appendResult94 * panner123 ) ).r + 0.05 ) , 0.0 , 1.0 );
			clip( ( ifLocalVar82 * (1.0 + (_Dissolve - 0.0) * (0.49 - 1.0) / (clampResult101 - 0.0)) ) - _Cutoff );
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=17000
488;212;1019;673;2559.528;852.1436;4.063673;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;4;-3723.706,745.2616;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector4Node;18;-3731.185,1015.665;Float;False;Property;_EndPoint;EndPoint;4;1;[HideInInspector];Create;True;0;0;False;0;-10,1,0,0;-10,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;105;-3467.877,969.6665;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;5;-3702.355,516.6335;Float;False;Property;_StartPoint;StartPoint;0;1;[HideInInspector];Create;True;0;0;False;0;0,1,0,0;0,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-3251.944,1057.509;Float;False;myVarName;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;3;-3427.32,518.8906;Float;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-2998.878,604.8352;Float;False;StartPoint;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;107;-3032.07,1067.13;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-3027.931,1238.876;Float;False;Property;_EndDistance;End Distance;5;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;37;-2779.004,614.4567;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;108;-2769.436,1061.376;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2791.071,769.5554;Float;False;Property;_StartDistance;Start Distance;1;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-2516.37,608.7026;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;109;-2591.839,1061.49;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2360.64,1239.721;Float;False;Property;_EndRound;End Round;6;0;Create;True;0;0;False;0;6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;10;-2338.773,608.8168;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;58;-3238.767,-151.042;Float;False;Property;_SpeedMainTexUVNoiseZW;Speed MainTex U/V + Noise Z/W;11;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;41;-3212.719,57.66781;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;110;-2376.387,1062.221;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;54;-2861.638,-114.0388;Float;False;Property;_MainTexTilingXYNoiseTilingZW;MainTex Tiling XY Noise Tiling ZW;10;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;40;-2806.381,73.31741;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;111;-2114.052,1062.383;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2108.61,765.806;Float;False;Property;_StartRound;Start Round;2;0;Create;True;0;0;False;0;6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;11;-2123.321,609.548;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;57;-2793.746,165.2972;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-2509.522,-38.71316;Float;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;39;-1884.014,845.8511;Float;False;Property;_UseEndRound;Use End Round;3;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;12;-1860.987,609.7101;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;48;-2531.817,75.06427;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;122;-2801.653,276.887;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;15;-1652.057,612.8437;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;94;-2530.618,199.1989;Float;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;123;-2539.129,304.2021;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2285.195,-33.52421;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;49;-1472.692,756.4017;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-1525.692,835.1599;Float;False;Property;_TextureVertexPower;Texture Vertex Power;17;0;Create;True;0;0;False;0;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2277.272,193.7767;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;32;-1861.296,-64.02641;Float;True;Property;_MainTex;MainTex;8;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-1263.44,704.174;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;92;-1869.931,241.6539;Float;True;Property;_DissolveNoise;Dissolve Noise;9;0;Create;True;0;0;False;0;3584f2bf4afb5284d91edb6a29126e62;3584f2bf4afb5284d91edb6a29126e62;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-1101.031,601.4785;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;97;-1542.913,283.1172;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;86;-1264.012,112.5988;Float;False;Constant;_Vector0;Vector 0;16;0;Create;True;0;0;False;0;0,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;119;-1302.889,38.61176;Float;False;Property;_Distance;Distance;7;0;Create;True;0;0;False;0;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-1292.234,-28.24228;Float;False;83;StartPoint;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;29;-746.6608,382.4877;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-1847.162,137.5184;Float;False;Property;_Dissolve;Dissolve;15;0;Create;True;0;0;False;0;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;101;-1387.751,282.4348;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;55;-1765.904,-328.7682;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;56;-1796.355,-490.5531;Float;False;Property;_Color;Color;13;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;103;-1214.98,237.6845;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0.49;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-559.8307,380.9315;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;121;-621.1272,726.7653;Float;False;Property;_Scale;Scale;18;1;[HideInInspector];Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;2;-657.4492,493.3651;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;13;-1741.793,-166.5391;Float;False;Property;_Emission;Emission;12;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;82;-1068.378,10.39251;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;10;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-657.0306,649.8007;Float;False;Property;_VertexPower;Vertex Power;16;0;Create;True;0;0;False;0;0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-865.6904,233.1446;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1;-362.2819,397.4759;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1491.388,-245.6022;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;;0;0;Unlit;Hovl/Particles/VolumeLaser;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;14;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;105;0;4;0
WireConnection;105;1;18;0
WireConnection;106;0;105;0
WireConnection;3;0;5;0
WireConnection;3;1;4;0
WireConnection;83;0;3;0
WireConnection;107;0;106;0
WireConnection;37;0;83;0
WireConnection;108;0;107;0
WireConnection;108;1;22;0
WireConnection;7;0;37;0
WireConnection;7;1;9;0
WireConnection;109;0;108;0
WireConnection;109;2;22;0
WireConnection;10;0;7;0
WireConnection;10;2;9;0
WireConnection;110;0;109;0
WireConnection;110;2;22;0
WireConnection;40;0;41;1
WireConnection;40;1;3;0
WireConnection;111;0;110;0
WireConnection;111;1;26;0
WireConnection;11;0;10;0
WireConnection;11;2;9;0
WireConnection;57;0;58;1
WireConnection;57;1;58;2
WireConnection;46;0;54;1
WireConnection;46;1;54;2
WireConnection;39;1;111;0
WireConnection;12;0;11;0
WireConnection;12;1;14;0
WireConnection;48;0;40;0
WireConnection;48;2;57;0
WireConnection;122;0;58;3
WireConnection;122;1;58;4
WireConnection;15;0;12;0
WireConnection;15;1;39;0
WireConnection;94;0;54;3
WireConnection;94;1;54;4
WireConnection;123;0;40;0
WireConnection;123;2;122;0
WireConnection;47;0;46;0
WireConnection;47;1;48;0
WireConnection;49;0;15;0
WireConnection;95;0;94;0
WireConnection;95;1;123;0
WireConnection;32;1;47;0
WireConnection;50;0;32;1
WireConnection;50;1;49;0
WireConnection;50;2;53;0
WireConnection;92;1;95;0
WireConnection;51;0;15;0
WireConnection;51;1;50;0
WireConnection;97;0;92;1
WireConnection;29;0;51;0
WireConnection;101;0;97;0
WireConnection;103;0;59;0
WireConnection;103;2;101;0
WireConnection;30;0;29;0
WireConnection;82;0;85;0
WireConnection;82;1;119;0
WireConnection;82;2;86;1
WireConnection;82;3;86;1
WireConnection;82;4;86;2
WireConnection;93;0;82;0
WireConnection;93;1;103;0
WireConnection;1;0;30;0
WireConnection;1;1;2;0
WireConnection;1;2;16;0
WireConnection;1;3;121;0
WireConnection;31;0;56;0
WireConnection;31;1;55;0
WireConnection;31;2;13;0
WireConnection;31;3;32;0
WireConnection;0;2;31;0
WireConnection;0;10;93;0
WireConnection;0;11;1;0
ASEEND*/
//CHKSM=FD52E6890287F953E60A7E662EDF8CACA4963DEA
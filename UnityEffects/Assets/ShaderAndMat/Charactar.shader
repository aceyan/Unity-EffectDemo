Shader "QinYou/Charactar" {
    //shader for character in the scene
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		[KeywordEnum(OFF, ON)] _Fresnel ("fresnel switch?", Float) = 0
		_RimColor ("rim color", Color) = (1, 1, 1, 1) 
		_RimPower ("intensity of rim", Range(0, 8.0)) = 3.0 

		[KeywordEnum(Off, Add, Multi, Overlay)] _Flowoverlay ("flow mode", Float) = 0
		_FlowAlphaMask("FlowAlphaMask", 2D) = "black"{} 
		_FlowTex("texture of flow", 2D) = "black" {} 
		_FlowColor("color of flow",Color) = (1,1,1,1)
		_FlowExposure("intensity of flow color",float) = 1 
		_FlowxSpeed("flow speed in x",float) = 0
		_FlowySpeed("flow speed in y",float) = 0

		[HideInInspector] _AlphaCutoff ("Cutoff", float) = 0.5

		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
		[HideInInspector] _Mode ("Mode", Float) = 0.0
		[HideInInspector] _ZWrite ("ZWrite", Float) = 1
		[HideInInspector] _SrcBlend ("SrcBlend", Float) = 1.0
		[HideInInspector] _DstBlend ("DstBlend", Float) = 0.0

	}


	SubShader {
		Tags {  "RenderType"="Opaque" }
		Pass {
			Cull [_Cull]
			ZWrite [_ZWrite]
			Blend [_SrcBlend][_DstBlend]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest	
		
			#pragma shader_feature _ALPHATEST_ON

			#pragma multi_compile _FRESNEL_OFF _FRESNEL_ON

			#pragma shader_feature _FLOWOVERLAY_OFF _FLOWOVERLAY_ADD _FLOWOVERLAY_MULTI _FLOWOVERLAY_OVERLAY

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;
			//
			fixed4 _RimColor;
			half _RimPower;
			//
			sampler2D _FlowAlphaMask;
			sampler2D _FlowTex;
			float4 _FlowTex_ST;
			fixed4 _FlowColor;
			fixed _FlowExposure;
			fixed _FlowxSpeed;
			fixed _FlowySpeed;
			float _AlphaCutoff;

			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};
			
			struct v2f {
				float4 pos : POSITION;
				float4 viewDir  : TEXCOORD1;//direction of view
				float3 normal : TEXCOORD2;
				float2 texcoord : TEXCOORD0;
				float2 flowTexcoord : TEXCOORD3;
			
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				//o.flowTexcoord = TRANSFORM_TEX(v.texcoord, _FlowTex);
				#ifndef _FLOWOVERLAY_OFF
				o.flowTexcoord = TRANSFORM_TEX(v.texcoord, _FlowTex) + float2(_Time.y * _FlowxSpeed, _Time.y * _FlowySpeed);
				#else
				o.flowTexcoord = TRANSFORM_TEX(v.texcoord, _FlowTex);
				#endif
				o.normal   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);

				#if _FRESNEL_ON
					float4 fviewDir;
					fviewDir.xyz = ObjSpaceViewDir(v.vertex);
					fviewDir.w = 0;
					o.viewDir = mul(UNITY_MATRIX_MV, fviewDir);
				#else
					o.viewDir = 1;
				#endif
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord); 
				col.rgb *= _TintColor.rgb * 2;
				col.a *= _TintColor.a;
				#ifndef _FLOWOVERLAY_OFF
					float2 uv = i.flowTexcoord;
					fixed4 maskCol = tex2D(_FlowAlphaMask, i.texcoord);
					half3 flow = tex2D (_FlowTex,  uv) * _FlowColor.rgb * _FlowExposure;
					#if _FLOWOVERLAY_ADD
					col.rgb = col.rgb + flow * maskCol.a;
					#elif _FLOWOVERLAY_MULTI 
					col.rgb = col.rgb * (1-maskCol.a)  + col.rgb * flow * maskCol.a;
					#elif _FLOWOVERLAY_OVERLAY
					col.rgb = col.rgb * (1-maskCol.a) + flow  * maskCol.a;
					#endif
				#endif

				#if _FRESNEL_ON
					float rim = 1 - saturate(dot (normalize(i.viewDir), normalize(float4(i.normal,1))));
 					float fresnelTerm =  pow (rim , _RimPower);//Fresnel factor
 					col.rgb = col * (1- fresnelTerm ) + fresnelTerm * _RimColor;
				#endif

				#if _ALPHATEST_ON
				if(col.a < _AlphaCutoff)
				{
					discard;
				}
				#endif
				return col;
			}
			ENDCG			
		}
	} 
	CustomEditor "CharactarShaderGUI"
}

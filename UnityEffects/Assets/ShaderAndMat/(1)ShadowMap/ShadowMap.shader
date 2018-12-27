// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "UnityEffects/ShadowMap"
{
	//Renders objects that receive shadows
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_bias ("bias", float) = 0.002
	}
	SubShader
	{
		Tags { "RenderType"="ShadowMap" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 projectionPos :  TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 projectionPos :  TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform matrix _ViewProjectionMat;//view matrix * projection matrix
			uniform sampler2D _DepthMap;
			uniform float _NearClip;
			uniform float _FarClip;
			float4 _DepthMap_ST;
			float _bias;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				matrix mvp = mul(_ViewProjectionMat ,unity_ObjectToWorld);
				o.projectionPos = mul( mvp,float4(v.vertex.xyz,1));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			
				//
				float4 uvPos = i.projectionPos;
				uvPos.x = uvPos.x * 0.5f + uvPos.w * 0.5f;//transfer into[0,w]
				uvPos.y = uvPos.y * 0.5f    + uvPos.w * 0.5f;//transfer into[0,w]
				//To map the projected points to the texture, we have to consider the y direction of the uv space on different platforms
				#if UNITY_UV_STARTS_AT_TOP
				//Dx like
				uvPos.y = uvPos.w - uvPos.y;
				#endif


				float depth =  DecodeFloatRGBA(tex2D(_DepthMap, uvPos.xy/ uvPos.w));//get depth

				float depthPixel = uvPos.z / uvPos.w;


				#if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)) && defined(SHADER_API_MOBILE)
				//GL like
				depthPixel = depthPixel * 0.5f + 0.5; 

				
				#else
				//DX like
				depthPixel = depthPixel;


				#endif

				float4 textureCol = tex2D(_MainTex, i.uv);

				//adjust the depth error using an offset value
				float4 shadowCol = (depthPixel - depth > _bias)  ? 0.3 : 1;

				return textureCol * shadowCol;
				
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

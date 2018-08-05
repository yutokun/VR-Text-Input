// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// *** OvrAvatar Mobile Single Component Loading shader  ***
//
// Cut-down single component version of the avatar shader to be used during loading
//

Shader "OvrAvatar/Avatar_Mobile_Loader"
{
	Properties
	{
		[NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
		
		_BaseColor("Color Tint", Color) = (0.95,0.82,0.73,1.0)
		_Dimmer("Dimmer", Range(0.0,1.0)) = 1.0
		_LoadingDimmer("Loading Dimmer", Range(0.0,1.0)) = 1.0
		_Alpha("Alpha", Range(0.0,1.0)) = 1.0

		_DiffuseIntensity("Diffuse Intensity", Range(0.0,1.0)) = 0.3
		_RimIntensity("Rim Intensity", Range(0.0,10.0)) = 5.0
		
		_BacklightOffset("Backlight Offset", Vector) = (0.0,3.22,-1.0)
		_BacklightIntensity("Backlight Intensity", Range(0.0,1.0)) = 1.0
		
		[HideInInspector] _MouthPosition("Mouth position", Vector) = (0,0,0,1)
		[HideInInspector] _MouthDirection("Mouth direction", Vector) = (0,0,0,1)
		[HideInInspector] _MouthEffectDistance("Mouth Effect Distance", Float) = 0.03
	}
	SubShader
	{
		Pass
		{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			LOD 100
			ZWrite On
			ZTest LEqual
			Cull Back
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest

			uniform sampler2D	_NormalMap;
			uniform float4		_NormalMap_ST;
			
			uniform float4		_BaseColor;
			uniform float		_Dimmer;
			uniform float		_LoadingDimmer;
			uniform float		_Alpha;
			uniform float		_BaseHeight;

			uniform float		_RimIntensity;
			uniform float		_DiffuseIntensity;
			
			uniform float		_BacklightIntensity;
			uniform float3		_BacklightOffset;
			
			uniform float		_Voice;
			uniform float4		_MouthPosition;
			uniform float4		_MouthDirection;
			uniform float		_MouthEffectDistance;
			static const fixed	MOUTH_ZSCALE = 0.5f;
			static const fixed	MOUTH_DROPOFF = 0.01f;
						
			struct appdata
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
				float4 tangent: TANGENT;
				float4 uv: TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 posWorld: TEXCOORD1;
				float3 normalDir: TEXCOORD2;
				float3 tangentDir: TEXCOORD3;
				float3 bitangentDir: TEXCOORD4;
				float fade : TEXCOORD5;
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o)

				// Mouth vertex animation with voip
				float4 worldVert = mul(unity_ObjectToWorld, v.vertex);;
				float3 delta = _MouthPosition - worldVert;
				delta.z *= MOUTH_ZSCALE;
				float dist = length(delta);
				float displacement = _Voice * smoothstep(_MouthEffectDistance + MOUTH_DROPOFF, _MouthEffectDistance, dist);
				worldVert.xyz -= _MouthDirection * displacement;
				v.vertex = mul(unity_WorldToObject, worldVert);
				
				// Calculate tangents for normal mapping
				o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
				o.tangentDir = normalize(mul(unity_ObjectToWorld, half4(v.tangent.xyz, 0.0)).xyz);
				o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);

				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				// Light directions
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 reverseLightDirection = lightDirection * _BacklightOffset;

				// Calculate normal
				float3 normalMap = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
				float3 normalDirection = normalize(mul(normalMap.rgb, tangentTransform));
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				// Apply view, normal, and lighting dependent terms
				float VdotN = saturate(dot(viewDirection, normalDirection));
				float NdotL = saturate(dot(normalDirection, lightDirection));
				float NdotInvL = saturate(dot(normalDirection, normalize(lightDirection * reverseLightDirection)));

				// Calculate color
				fixed4 col;
				// Multiply in color tint
				col.rgb = _BaseColor;
				// Main light
				col.rgb += (_DiffuseIntensity * NdotL);
				// Illuminate main light from behind
				col.rgb += (_DiffuseIntensity * _BacklightIntensity) * NdotInvL;
				// Rim term
				col.rgb += pow(1.0 - VdotN, _RimIntensity) * NdotL * _LightColor0;
							
				// Global dimmer
				col.rgb *= lerp(_Dimmer, _LoadingDimmer, step(_LoadingDimmer, _Dimmer));
				
				// Global alpha
				col.a = _Alpha;

				// Return clamped final color
				return saturate(col);
			}
			ENDCG
		}
	}
}
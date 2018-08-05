// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// *** OvrAvatar Mobile Combined Mesh shader ***
// *** Texture array approach for rendering a combined mesh avatar ***
//
// This is a Unity vertex-fragnment shader implementation for our 1.5 skin shaded avatar look.
// The benefit of using this version is performance as it bypasses the PBR lighting model and 
// so is generally recommended for use on mobile.
//
// Critically, this is the texture array version of the shader, which will draw all pre-combined
// components in one draw call. This is coupled with OvrAvatarMaterialManager to populate the
// shader properties.

Shader "OvrAvatar/Avatar_Mobile_CombinedMesh"
{
	Properties
	{
		_MainTex("Main Texture Array", 2DArray) = "white" {}
	_NormalMap("Normal Map Array", 2DArray) = "bump" {}
	_RoughnessMap("Roughness Map Array", 2DArray) = "white" {}

	_Cubemap("Reflection Map", Cube) = "" {}
	_Dimmer("Dimmer", Range(0.0,1.0)) = 1.0
		_Alpha("Alpha", Range(0.0,1.0)) = 1.0

		_Samples("Samples", Int) = 4.97

		[HideInInspector] _BacklightOffset("Backlight Offset", Vector) = (0.0,3.22,-1.0)
		[HideInInspector] _MouthPosition("Mouth position", Vector) = (0,0,0,1)
		[HideInInspector] _MouthDirection("Mouth direction", Vector) = (0,0,0,1)
		[HideInInspector] _MouthEffectDistance("Mouth Effect Distance", Float) = 0.03
		[HideInInspector] _MouthDistanceScale("Mouth Effect Scale", Float) = 1.0
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
#pragma target 3.5
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#pragma fragmentoption ARB_precision_hint_fastest

		UNITY_DECLARE_TEX2DARRAY(_MainTex);
	UNITY_DECLARE_TEX2DARRAY(_NormalMap);
	float4 _NormalMap_ST;
	UNITY_DECLARE_TEX2DARRAY(_RoughnessMap);
	int _Samples;

	uniform float4		_BaseColor[5];
	uniform float		_DiffuseIntensity[5];
	uniform float		_RimIntensity[5];
	uniform float		_BacklightIntensity[5];
	uniform float		_ReflectionIntensity[5];

	uniform samplerCUBE _Cubemap;
	uniform float		_Dimmer;
	uniform float		_Alpha;

	uniform float3		_BacklightOffset;

	uniform float		_Voice;
	uniform float4		_MouthPosition;
	uniform float4		_MouthDirection;
	uniform float		_MouthEffectDistance;
	uniform float		_MouthDistanceScale;

	static const fixed	MOUTH_ZSCALE = 0.5f;
	static const fixed	MOUTH_DROPOFF = 0.01f;

	struct appdata
	{
		float4 vertex: POSITION;
		float3 normal: NORMAL;
		float4 tangent: TANGENT;
		float2 texcoord: TEXCOORD0;
		float4 vertexColor : COLOR0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float3 uv : TEXCOORD0;
		float4 posWorld: TEXCOORD1;
		float3 normalDir: TEXCOORD2;
		float3 tangentDir: TEXCOORD3;
		float3 bitangentDir: TEXCOORD4;
	};

	v2f vert(appdata v)
	{
		v2f o;

		// Mouth vertex animation with voip
		float4 worldVert = mul(unity_ObjectToWorld, v.vertex);;
		float3 delta = _MouthPosition - worldVert;
		delta.z *= MOUTH_ZSCALE;
		float dist = length(delta);
		float scaledMouthDropoff = _MouthDistanceScale * MOUTH_DROPOFF;
		float scaledMouthEffect = _MouthDistanceScale * _MouthEffectDistance;

		float displacement = _Voice * smoothstep(scaledMouthEffect + scaledMouthDropoff, scaledMouthEffect, dist);
		worldVert.xyz -= _MouthDirection * displacement;
		v.vertex = mul(unity_WorldToObject, worldVert);

		// Calculate tangents for normal mapping
		o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
		o.tangentDir = normalize(mul(unity_ObjectToWorld, half4(v.tangent.xyz, 0.0)).xyz);
		o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);

		o.posWorld = worldVert;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.texcoord;
		o.uv.z = v.vertexColor.x * _Samples;
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		// Light directions
		float3 lightDirection = _WorldSpaceLightPos0.xyz;
		float3 reverseLightDirection = lightDirection * _BacklightOffset;

		// Unpack normal map									
		float3 transformedNormalUV = i.uv;
		transformedNormalUV.xy = float2(TRANSFORM_TEX(i.uv.xy, _NormalMap));
		float3 normalMap = UNITY_SAMPLE_TEX2DARRAY(_NormalMap, transformedNormalUV).rgb * 2 - 1;

		// Calculate normal
		float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
		float3 normalDirection = normalize(mul(normalMap.rgb, tangentTransform));
		float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

		// Apply view, normal, and lighting dependent terms
		float VdotN = saturate(dot(viewDirection, normalDirection));
		float NdotL = saturate(dot(normalDirection, lightDirection));
		float NdotInvL = saturate(dot(normalDirection, normalize(lightDirection * reverseLightDirection)));

		// Reflection proble sample
		float3 worldReflection = reflect(-viewDirection, normalDirection);
		half3 reflectionColor = texCUBE(_Cubemap, worldReflection).rgb;

		// Calculate color for each component
		float4 col;
		// Get index into texture array
		int componentIndex = floor(i.uv.z + 0.5);
		// Diffuse texture sample
		col = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv);
		// Multiply in color tint
		col.rgb *= _BaseColor[componentIndex];
		// Main light
		col.rgb += _DiffuseIntensity[componentIndex] * NdotL;
		// Illuminate main light from behind
		col.rgb += (_DiffuseIntensity[componentIndex] * _BacklightIntensity[componentIndex]) * NdotInvL;
		// Rim term
		col.rgb += pow(1.0 - VdotN, _RimIntensity[componentIndex]) * NdotL * _LightColor0;
		// Reflection
		col.rgb += reflectionColor * UNITY_SAMPLE_TEX2DARRAY(_RoughnessMap, i.uv).a * _ReflectionIntensity[componentIndex];

		// Global dimmer
		col.rgb *= _Dimmer;
		// Global alpha
		col.a *= _Alpha;

		// Return clamped final color
		return saturate(col);
	}
		ENDCG
	}
	}
}
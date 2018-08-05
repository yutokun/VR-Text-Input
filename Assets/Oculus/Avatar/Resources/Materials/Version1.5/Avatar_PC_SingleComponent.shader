//
// OvrAvatar PC single component shader
//
// This is a Unity Surface shader implementation for our 1.5 skin shaded avatar look.
// The benefit of using this version is that it uses the Unity PBR lighting under the hood,
// whereas the Mobile version supports one light. The vert-frag path is generally recommended
// on mobile.
//
// Note:	Mouth vertex movement does not work due to Unity surface shader hidden settings
//			Use Mobile implementation instead if you need this.
//
// Mouth vertex shader settings:
// - EffectDistance: 0.03
// - See MouthDriver.cs for script driver

Shader "OvrAvatar/Avatar_PC_SingleComponent"
{
    Properties
	{
		[NoScaleOffset] _MainTex("Color (RGB)", 2D) = "white" {}
		[NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
		[NoScaleOffset] _RoughnessMap("RoughnessMap", 2D) = "black" {}

		_GlossinessScale("Glossiness Scale", Range(0.0,1.0)) = 0.14

		_BaseColor("Color Tint", Color) = (0.95,0.82,0.73,1.0)

		_Dimmer("Dimmer", Range(0.0,1.2)) = 1.0
		_Alpha("Alpha", Range(0.0,1.0)) = 1.0
        
		_DiffuseIntensity("Diffuse Intensity", Range(0.0,1.0)) = 0.3
		_RimIntensity("Rim Intensity", Range(0.0,10.0)) = 5.0

		_BacklightOffset("Backlight Offset", Vector) = (0.0,3.22,-1.0)
		_BacklightIntensity("Backlight Intensity", Range(0.0,1.0)) = 1.0

        _Voice("Voice", Range(0.0,1.0)) = 0
        _EffectDistance("Voice Effect Distance", Float) = 0.03
        _MouthPosition("Mouth position", Vector) = (0,0,0,1)
        _MouthDirection("Mouth direction", Vector) = (0,0,0,1)
    }

    SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		// Render the back facing parts of the object then set on backface culling.
		// This fixes broken faces with convex meshes when using the alpha path.
		Pass
		{
			Color(0,0,0,0)
		}

		CGPROGRAM
		#pragma surface surf Standard alpha:fade
		#pragma target 3.0
		#pragma fragmentoption ARB_precision_hint_fastest		
		#pragma multi_compile PBR_LIGHTING_OFF PBR_LIGHTING_ON
		#pragma multi_compile FIX_NORMAL_OFF FIX_NORMAL_ON

		uniform sampler2D	_MainTex;
		uniform sampler2D	_NormalMap;
		uniform float4		_NormalMap_ST;

		uniform float		_GlossinessScale;
		uniform sampler2D	_RoughnessMap;
		
		uniform float4		_BaseColor;
		uniform float		_Dimmer;
		uniform float		_Alpha;

		uniform float		_DiffuseIntensity;
		uniform float		_RimIntensity;

		uniform float3		_BacklightOffset;
		uniform float		_BacklightIntensity;
			
		uniform float		_Voice;
		uniform float		_EffectDistance;
		uniform float4		_MouthPosition;
		uniform float4		_MouthDirection;
		static const fixed	MOUTH_ZSCALE = 0.1f;
		static const fixed	MOUTH_DROPOFF = 0.01f;

		struct Input
		{
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldNormal; INTERNAL_DATA
		};

		void vert(inout appdata_full v)
		{
			// Animate mouth verts.
			// Broken in surface version, use vert-frag shader if this is needed.
			const float ZScale = 0.5;
			const float DropOffLength = 0.01;

			float4 worldVert = v.vertex;
			float3 delta = _MouthPosition - worldVert;
			delta.z *= MOUTH_ZSCALE;
			float dist = length(delta);
			float displacement = _Voice * smoothstep(_EffectDistance + MOUTH_DROPOFF, _EffectDistance, dist);
			worldVert.xyz -= _MouthDirection * displacement;
			v.vertex = worldVert;
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float4 c = tex2D(_MainTex, IN.uv_MainTex) * _BaseColor;
			o.Normal = tex2D(_NormalMap, TRANSFORM_TEX(IN.uv_MainTex, _NormalMap)).rgb * 2 - 1;
			o.Alpha = c.a * _Alpha;

			// PBR path packs color into Albedo
			#ifdef PBR_LIGHTING_ON
				float4 roughnessSample = tex2D(_RoughnessMap, IN.uv_MainTex);
				o.Emission = 0;
				o.Smoothness = roughnessSample.a;
				o.Metallic = roughnessSample.r;
				o.Albedo = c.rgb;

			// Single light path packs color into Emission
			#else
				o.Albedo = 0;
				o.Emission = c.rgb;
				o.Smoothness = 0;
				o.Metallic = 0;				
				
				float3 lightDirection = _WorldSpaceLightPos0.xyz;
				float3 reverseLightDirection = lightDirection * _BacklightOffset;
				
				float VdotN = saturate(dot(normalize(IN.viewDir), o.Normal));
				float NdotL = saturate(dot(WorldNormalVector(IN, o.Normal), _WorldSpaceLightPos0.xyz));
				float NdotInvL = saturate(dot(WorldNormalVector(IN, o.Normal), normalize(lightDirection * reverseLightDirection)));
				
				o.Emission += _DiffuseIntensity * NdotL;
				o.Emission += (_DiffuseIntensity * _BacklightIntensity) * NdotInvL;
				o.Emission += pow(1.0 - VdotN, _RimIntensity) * NdotL * _LightColor0;
				o.Emission *= _Dimmer;
			#endif
        }
        ENDCG
    }
}

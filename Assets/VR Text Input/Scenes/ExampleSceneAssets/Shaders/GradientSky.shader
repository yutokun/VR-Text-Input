Shader "Unlit/GradientSky"
{
	Properties
	{
		_TopColor ("Top Color", Color) = (1, 1, 1, 1)
		_BottomColor ("Bottom Color", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Background" "Queue"="Background" "PreviewType"="SkyBox"}
		LOD 100

		Pass
		{
		    ZWrite Off
		    Cull Off
		    
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			struct appdata
			{
				float4 vertex : POSITION;
				float3 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 _TopColor;
			fixed4 _BottomColor;
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(lerp(_BottomColor, _TopColor, i.uv.y));
			}
			ENDCG
		}
	}
}

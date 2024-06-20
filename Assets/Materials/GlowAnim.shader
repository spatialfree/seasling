// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/GlowAnim"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_EmissionMap ("Emission Map", 2D) = "black" {}
		_ScrollsSpeeds ("Scroll Speeds", vector) = (-5, -20, 0, 0)

		_Speed ("Speed", float) = 1
		_RotOffset ("RotOffset", float) = 1
		_Ripple ("Ripple", float) = 1
		_TraAmplitude ("Translate Amplitude", float) = 1
		_RotDegrees ("Rotation Max", float) = 15
		_Mask ("Mask Falloff", 2D) = "white" {}
		_MaskScale ("_Scale", float) = 1

		// Instance Specific
		[PerRendererData]_OffsetPlay ("Offset Play", float) = 1
		[PerRendererData]_PlaySpeed ("Play Speed", float) = 1
		[PerRendererData]_EmissionColor ("Emission Color", Color) = (0, 1, 1)
	}
	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _EmissionMap, _Mask;
		float2 _ScrollsSpeeds;
		fixed4 _Color;
		half _Glossiness, _Metallic, _Speed, _TraAmplitude, _RotDegrees, _MaskScale, _RotOffset, _Ripple;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float, _OffsetPlay)
			UNITY_DEFINE_INSTANCED_PROP(float, _PlaySpeed)
			UNITY_DEFINE_INSTANCED_PROP(fixed3, _EmissionColor)
		UNITY_INSTANCING_BUFFER_END(Props)

		float4 RotateAroundYInDegrees (float4 vertex, float degrees)
		{
			float alpha = degrees * UNITY_PI / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float4(mul(m, vertex.xz), vertex.yw).xzyw;
		}

		void vert (inout appdata_full v)
		{
			// translate + stretch ---> v.vertex.z += sin(_Time.x * 100 * UNITY_ACCESS_INSTANCED_PROP(Props, _PlaySpeed) + (v.vertex.z * 0.1)) * 0.25f;
			float masklod = tex2Dlod(_Mask, v.vertex.z * _MaskScale);
			//v.vertex.x += sin(_Time.x * _Speed * UNITY_ACCESS_INSTANCED_PROP(Props, _PlaySpeed)) * _TraAmplitude * masklod;
			//v.vertex.x += sin(_Time.x * _Ripple) * _TraAmplitude * masklod;
			v.vertex.x += RotateAroundYInDegrees(v.vertex.x, sin(_Time.x * (_Speed * _RotOffset) * UNITY_ACCESS_INSTANCED_PROP(Props, _PlaySpeed)) * _RotDegrees).x * masklod;
			v.vertex.z += RotateAroundYInDegrees(v.vertex.z, sin(_Time.x * (_Speed * _RotOffset) * UNITY_ACCESS_INSTANCED_PROP(Props, _PlaySpeed)) * _RotDegrees).z * masklod;
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			IN.uv_MainTex += _ScrollsSpeeds * _Time.x + UNITY_ACCESS_INSTANCED_PROP(Props, _OffsetPlay);
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			//o.Alpha = c.a;
			o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * UNITY_ACCESS_INSTANCED_PROP(Props, _EmissionColor).rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

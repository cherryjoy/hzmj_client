Shader "CM/ModelAddtiveDisturb" 
{
	Properties 
	{
		_TintColor ("Main Color", Color) = (1, 1, 1, 1)
		_MainDirX ("MainTex dirX", Float) = 0.0
		_MainDirY ("MainTex dirY", Float) = 0.2
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_DisturbTex ("Disturb Texture", 2D) = "white" {}
		_DistortDirX ("Distort dirX", Float) = 0.1
		_DistortDirY ("Distort dirY", Float) = 0.1
		_DistortStrength ("Distort Strength", Float) = 0.1

		_ChannelTex ("Channel Texture", 2D) = "white" {}
		_ThetaChannel ("Rotate Channel Tex", range(0, 1.5707963)) = 0	//0.5*PI

		_ColorBright ("Color Bright", Float) = 1.0
		_AlphaBright ("Alpha Bright", Float) = 1.0
	}
	
	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha One
				
		Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members uv_MainTex,uv_DisturbTex)
			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _ChannelTex;
			sampler2D _DisturbTex;
			half4 _TintColor;
			half _MainDirX;
			half _MainDirY;
			half _DistortDirX;
			half _DistortDirY;
			half _DistortStrength;
			half _ThetaChannel;
			half _ColorBright;
			half _AlphaBright;
					
			struct v2f
			{
				float4 pos : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_DisturbTex : TEXCOORD1;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
												
				return o;
			}
				
			float4 frag (v2f i) : COLOR
			{
				half2 distortDir = half2(_DistortDirX, _DistortDirY);
				half2 mainDir = half2(_MainDirX, _MainDirY);
				half distort = tex2D(_DisturbTex, i.uv_DisturbTex + _Time.y * distortDir).r;
				half offsetUV = _DistortStrength * (distort - 0.5);
				half4 c = tex2D (_MainTex, i.uv_MainTex + offsetUV + _Time.y * mainDir);

				half2 channelUV = i.uv_MainTex;
				channelUV -= 0.5;
				half cosTheta = cos(_ThetaChannel);
				half sinTheta = sin(_ThetaChannel);
				half rotUVx = channelUV.x * cosTheta - channelUV.y * sinTheta;
				half rotUVy = channelUV.x * sinTheta + channelUV.y * cosTheta;
				channelUV = half2(rotUVx + 0.5, rotUVy + 0.5);
				half4 channel = tex2D (_ChannelTex, channelUV);
						
				fixed4 finalColor;
				finalColor.rgb = c.rgb * _ColorBright * _TintColor.rgb;
				finalColor.a = c.a * channel.r * _AlphaBright * _TintColor.a;
					
				return finalColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

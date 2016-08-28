Shader "CM/ParticleBlendCutoff" 
{
	Properties 
	{
		_Cutoff ("cut off", Range(0.0, 1)) = 0.01
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_MainTex ("Particle Texture", 2D) = "white" {}
		_MainDirX ("MainTex dirX", Float) = 0.0
		_MainDirY ("MainTex dirY", Float) = 0.0

		_DisturbTex ("Disturb Texture", 2D) = "white" {}
		_DistortDirX ("Distort dirX", Float) = 0
		_DistortDirY ("Distort dirY", Float) = 0
		_DistortStrength ("Distort Strength", Float) = 0
		_ChannelTex ("Channel Texture", 2D) = "white" {}
		
		_RotateCenterX ("Rotate Center X", float) = 0.5
		_RotateCenterY ("Rotate Center Y", float) = 0.5
		_Rotation ("Rotation theta", float) = 0.0
		_ScaleCenterX ("Scale Center X", float) = 0.5
		_ScaleCenterY ("Scale Center Y", float) = 0.5
		_ScaleX ("Scale X", float) = 1
		_ScaleY ("Scale Y", float) = 1
				
		_RGBBright ("RGB Bright", float) = 1
		_AlphaBright ("Alpha Bright", float) = 1
	}

	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Off 
		Lighting Off 
		ZWrite Off

		SubShader 
		{
			Pass 
			{		
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				
				#include "UnityCG.cginc"

				float _Cutoff;
				sampler2D _MainTex;
				sampler2D _DisturbTex;
				sampler2D _ChannelTex;
				fixed4 _TintColor;
				
				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD1;
					#endif
					float2 texcoord1 : TEXCOORD2;
					float2 texcoord2 : TEXCOORD3;
				};
				
				float4 _MainTex_ST;
				float4 _DisturbTex_ST;
				float4 _ChannelTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					o.texcoord1 = TRANSFORM_TEX(v.texcoord,_DisturbTex);
					o.texcoord2 = TRANSFORM_TEX(v.texcoord,_ChannelTex);
					return o;
				}

				sampler2D_float _CameraDepthTexture;
				float _InvFade;
				half _MainDirX;
				half _MainDirY;
				half _DistortDirX;
				half _DistortDirY;
				half _DistortStrength;

				float _Rotation;
				float _RotateCenterX;
				float _RotateCenterY;
				float _ScaleCenterX;
				float _ScaleCenterY;
				float _ScaleX;
				float _ScaleY;
				float _RGBBright;
				float _AlphaBright;
				
				fixed4 frag (v2f i) : COLOR
				{
				/*
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif 
				*/
					half channel = tex2D(_ChannelTex, i.texcoord2).r;
					i.color.rgb *= _RGBBright;
					i.color.a *= _AlphaBright * channel;

					//rotate uv
					half theta = _Rotation / 180 * 3.1415926;
					half cosA = cos(theta);
					half sinA = sin(theta);
					half ux = i.texcoord.x - _RotateCenterX;
					half uy = i.texcoord.y - _RotateCenterY;
					half vx = ux * cosA - uy * sinA;
					half vy = ux * sinA + uy * cosA;
					i.texcoord = (half2(vx + _RotateCenterX, vy + _RotateCenterY));

					//scale uv
					ux = (_ScaleCenterX - _RotateCenterX);
					uy = (_ScaleCenterY - _RotateCenterY);
					_ScaleCenterX = ux * cosA - uy * sinA + _RotateCenterX;
					_ScaleCenterY = ux * sinA + uy * cosA + _RotateCenterY;
					i.texcoord.x = (i.texcoord.x - _ScaleCenterX) * _ScaleX + _ScaleCenterX;
					i.texcoord.y = (i.texcoord.y - _ScaleCenterY) * _ScaleY + _ScaleCenterY;

					//disturb uv
					half2 distortDir = half2(_DistortDirX, _DistortDirY);
					half2 disturb = tex2D(_MainTex, i.texcoord1 + _Time.y * distortDir).rg;
					half2 offsetUV = _DistortStrength * (disturb - 0.5);
					i.texcoord = saturate(i.texcoord + offsetUV);

					half2 mainDir = half2(_MainDirX, _MainDirY);
					fixed4 finalColor = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord + _Time.y * mainDir);
					
					clip(finalColor.a-_Cutoff);
									
					return finalColor;
				}
				ENDCG 
			}
		}	
	}
}

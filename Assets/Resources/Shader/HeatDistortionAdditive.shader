Shader "CM/HeatDistortionAdditive" 
{
	Properties 
	{
		_NoiseTex ("Noise Tex", 2D) = "white" {}
		_ChannelTex ("ChannelTex", 2D) = "white" {}
		_OverlayTex ("OverlayTex", 2D) = "white" {}
		_HeatTime ("Heat Time", Range(0, 1.5)) = 1
		_HeatForce ("Heat Force", Range(0.1, 250)) = 0.5
		_OverlayPercent ("OverlayPercent",Range(0,1)) = 0.5
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
	}
	
	Category 
	{
		Tags {"Queue"="Transparent+1" "IgnoreProjector"="true" "RenderType"="Transparent"}
		Cull Off 
		Lighting Off 
		ZWrite Off
		
		SubShader 
		{
			Blend SrcAlpha One
			LOD 200
			
			GrabPass 
			{
				Name "BASE"
				Tags { "LightMode" = "Always"}
			}
			
			Pass 
			{
				Name "BASE"
				Tags { "LightMode" = "Always"}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				struct v2f 
				{
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uv_noise : TEXCOORD1;
					float2 uv_channel : TEXCOORD2;
					float2 uv_overlay : TEXCOORD3;
				};
				
				float _HeatForce;
				float _HeatTime;
				sampler2D _NoiseTex;
				float4 _NoiseTex_ST;
				sampler2D _ChannelTex;
				float4 _ChannelTex_ST;
				sampler2D _OverlayTex;
				float4 _OverlayTex_ST;
				sampler2D _GrabTexture;
				float _OverlayPercent;
				float4 _Color;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uv_noise = TRANSFORM_TEX(v.texcoord, _NoiseTex);
					o.uv_channel = TRANSFORM_TEX(v.texcoord, _ChannelTex);
					o.uv_overlay = TRANSFORM_TEX(v.texcoord, _OverlayTex);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					half4 offsetColor1 = tex2D(_NoiseTex, i.uv_noise + _Time.xz * _HeatTime);
					half4 offsetColor2 = tex2D(_NoiseTex, i.uv_noise - _Time.yx * _HeatTime);
					half4 uv_old = i.uvgrab;
					half4 channelColor = tex2D(_ChannelTex, i.uv_channel);
					i.uvgrab.x += channelColor.a * (offsetColor1.r + offsetColor2.r - 1) * _HeatForce;
					i.uvgrab.y += channelColor.a * (offsetColor1.g + offsetColor2.g - 1) * _HeatForce;

					half4 col_distort = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
					col_distort.a = 1;
					half4 col_orig = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv_old));
					
					half4 distortedColor = lerp(col_distort, col_orig, 1 - channelColor.a);
					half4 overlayColor = tex2D(_OverlayTex,i.uv_overlay);
					
					half4 finalColor; 
					finalColor.rgb = (distortedColor.rgb * _Color.rgb * _Color.a + overlayColor.rgb * overlayColor.a * _OverlayPercent);
					finalColor.a = channelColor.a;
					
					return finalColor;
				}
				ENDCG
			}
		}

		SubShader 
		{
			Blend SrcAlpha One
			LOD 100
						
			Pass 
			{
				Name "BASE"
				Tags { "LightMode" = "Always"}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
				
				struct v2f 
				{
					float4 vertex : POSITION;
					float2 uv_channel : TEXCOORD0;
					float2 uv_overlay : TEXCOORD1;
				};
				
				sampler2D _ChannelTex;
				float4 _ChannelTex_ST;
				sampler2D _OverlayTex;
				float4 _OverlayTex_ST;
				float _OverlayPercent;
				float4 _Color;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv_channel = TRANSFORM_TEX(v.texcoord, _ChannelTex);
					o.uv_overlay = TRANSFORM_TEX(v.texcoord, _OverlayTex);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					half4 channelColor = tex2D(_ChannelTex, i.uv_channel);
					half4 overlayColor = tex2D(_OverlayTex,i.uv_overlay);
					
					half4 finalColor;
					finalColor.rgb = channelColor.rgb * overlayColor.rgb * _Color.rgb;
					finalColor.a = _Color.a;
					
					return finalColor;
				}
				ENDCG
			}
		}
	}
}

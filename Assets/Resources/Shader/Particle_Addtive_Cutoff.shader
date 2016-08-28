Shader "CM/ParticleAddtiveCutoff" 
{
	Properties 
	{
		_Cutoff ("cut off", Range(0.01, 1)) = 0.01
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_MainDirX ("MainTex dirX", Float) = 0.0
		_MainDirY ("MainTex dirY", Float) = 0.0
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
		Blend SrcAlpha One
		AlphaTest Greater [_Cutoff]
		ColorMask RGBA
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Fog { Color (0,0,0,0) }

		SubShader 
		{
			Pass 
			{		
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _TintColor;
				half _MainDirX;
				half _MainDirY;
				float _Rotation;
				float _RotateCenterX;
				float _RotateCenterY;
				float _ScaleCenterX;
				float _ScaleCenterY;
				float _ScaleX;
				float _ScaleY;
				float _RGBBright;
				float _AlphaBright;
				
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
				};

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					
					return o;
				}
				
				fixed4 frag (v2f i) : COLOR
				{
					i.color.rgb *= _RGBBright;
					i.color.a *= _AlphaBright;

					half theta = _Rotation / 180 * 3.1415926;
					half cosA = cos(theta);
					half sinA = sin(theta);
					half ux = i.texcoord.x - _RotateCenterX;
					half uy = i.texcoord.y - _RotateCenterY;
					half vx = ux * cosA - uy * sinA;
					half vy = ux * sinA + uy * cosA;
					i.texcoord = (half2(vx + _RotateCenterX, vy + _RotateCenterY));

					ux = (_ScaleCenterX - _RotateCenterX);
					uy = (_ScaleCenterY - _RotateCenterY);
					_ScaleCenterX = ux * cosA - uy * sinA + _RotateCenterX;
					_ScaleCenterY = ux * sinA + uy * cosA + _RotateCenterY;
					i.texcoord.x = (i.texcoord.x - _ScaleCenterX) * _ScaleX + _ScaleCenterX;
					i.texcoord.y = (i.texcoord.y - _ScaleCenterY) * _ScaleY + _ScaleCenterY;
					half2 mainDir = half2(_MainDirX, _MainDirY);
					
					return 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord + _Time.y * mainDir);
				}
				ENDCG 
			}
		}	
	}
}

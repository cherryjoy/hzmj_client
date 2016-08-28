Shader "GUI/FlashText" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "white" {}
		_Speed ("Speed" , float) = 0.1
		_TextLength("TextLength", float) = 1
		_TextScale ("TextScale" , float) = 0.1
		_TextOffsetX ("TextOffsetX" , float) = 0
		_TextOffsetY ("TextOffsetY" , float) = 0
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Offset -1, -1
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					half4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					float4 vertex : POSITION;
					half4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				sampler2D _MainTex;
				sampler2D _MaskTex;
				float4 _MainTex_ST;
				float4 _MaskTex_ST;
				float _Speed;
				float _TextScale;
				float _TextLength;
				float _TextOffsetX;
				float _TextOffsetY;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					v.vertex.x -= _TextOffsetX;
					v.vertex.y -= _TextOffsetY;
					float currentPos = (v.vertex.x / _TextScale)-fmod(_Time.x*_Speed,_TextLength);
					float2 temp = float2(currentPos,v.vertex.y / _TextScale);

					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord1 = TRANSFORM_TEX(temp, _MaskTex);

					o.color = v.color;
					return o;
				}

				half4 frag (v2f i) : COLOR
				{
					half4 col = i.color;
					col.a = tex2D(_MainTex, i.texcoord).a;
					half4 colMask = tex2D(_MaskTex,i.texcoord1);

					half tempA = col.a;
					//col = col + col*(colMask-1)*floor(colMask.a/col.a);
					//col.a *= ceil(clamp(colMask.a,0.0,1.0));
					col =colMask.a*colMask+(1-colMask.a)*col;
					col.a = tempA;

					return col;
				}
			ENDCG
		}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		Lighting Off
		Cull Off
		ZTest Always
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		Pass
		{
			SetTexture [_MainTex]
			{
				combine primary, texture
			}
		}
	}
}

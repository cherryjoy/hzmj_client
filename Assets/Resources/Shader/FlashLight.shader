Shader "Custom/FlashLight" 
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "black" {}
		_MainTextureColor ("MainTextureColor",Color) = (1,1,1,0)
		_MaskColor ("MaskColor (RGB)", Color) = (1,1,1,0)
		_SpeedX ("SpeedX" , float) = 0.1
		_SpeedY ("SpeedY", float) = 0.1
		_TextLength("TextLength", float) = 1
		_TransScaleX ("TransScaleX" , float) = 0.1
		_TransScaleY ("TransScaleY" , float) = 0.1
		_TextOffsetX ("TextOffsetX" , float) = 0
		_TextOffsetY ("TextOffsetY" , float) = 0
	}
	
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			LOD 100
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			AlphaTest Off
			Blend SrcAlpha OneMinusSrcAlpha
			
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

			float4 _Color;
			sampler2D _MainTex;
			sampler2D _MaskTex;
			float4 _MainTex_ST;
			float4 _MaskTex_ST;
			float _TransScaleX;
			float _TransScaleY;
			float _TextLength;
			float _TextOffsetX;
			float _TextOffsetY;
			fixed4 _MaskColor;
			fixed4 _MainTextureColor;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord, _MaskTex); 
				o.color = v.color * _Color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 mainColor = tex2D(_MainTex, i.texcoord);
				
				half4 finalColor;
				finalColor = mainColor * i.color;
				
				return finalColor;
			}
			ENDCG
		}
		
		Pass
		{	
			LOD 100
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			AlphaTest Off
			Blend SrcAlpha One

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

			float4 _Color;
			sampler2D _MainTex;
			sampler2D _MaskTex;
			float4 _MainTex_ST;
			float4 _MaskTex_ST;
			float _SpeedX;
			float _SpeedY;
			float _TransScaleX;
			float _TransScaleY;
			float _TextLength;
			float _TextOffsetX;
			float _TextOffsetY;
			fixed4 _MaskColor;
			fixed4 _MainTextureColor;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				v.vertex.x -= _TextOffsetX;
				v.vertex.y -= _TextOffsetY;
				float currentPosX = (v.vertex.x / _TransScaleX)-fmod(_Time.x * _SpeedX,_TextLength);
				float currentPosY = (v.vertex.y / _TransScaleY)-fmod(_Time.x * _SpeedY,_TextLength);
				float2 temp = float2(currentPosX,currentPosY);
				
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord1 = TRANSFORM_TEX(temp, _MaskTex); 
				
				o.color = v.color * _Color;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 col;
				col = tex2D(_MainTex, i.texcoord);
				col *= _MainTextureColor;
				half4 colMask = tex2D(_MaskTex,i.texcoord1);
				colMask.rgb *= _MaskColor;
				col.rgb = col.rgb + colMask.rgb;
				
				half4 finalColor;
				finalColor.rgb = colMask.rgb * i.color.rgb;
				finalColor.a = col.a * colMask.a * i.color.a;
				
				return finalColor;
			}
			ENDCG
		}
	}
}
Shader "CM/Transparent/Alpha" 
{
	Properties 
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
		_AlphaValue ("AlphaValue", Range(0, 1)) = 1
	}
	
	CGINCLUDE
	sampler2D _MainTex;
	float _AlphaValue;
	fixed4 _Color;
	ENDCG

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
		Lighting Off 
		LOD 100
		
		Pass
		{
		   ZWrite On
		   ColorMask 0
		   
		   CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex,i.uv);
				clip(mainColor.a - 0.8);
				
				return fixed4(0, 0, 0, 0);
			}
			
			ENDCG
		}

		Pass 
		{
			Tags{"LightMode"="ForwardBase"}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex,i.uv);
				fixed4 finalColor = mainColor * _Color;
				finalColor.a *= _AlphaValue;
			
				return finalColor;
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

Shader "CM/Discoloration" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_Angle ("Angle", Range(-3.14, 3.14)) = 0
		_Color ("Color",Color) = (1,1,1,1)
	}
	
	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _MatCap;
			float4 _MatCap_ST;
			float _Angle;
			
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_MatCap : TEXCOORD1;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_MatCap = TRANSFORM_TEX(v.texcoord, _MatCap);
								
				float3 worldNormal = normalize(_World2Object[0].xyz * v.normal.x + _World2Object[1].xyz * v.normal.y + _World2Object[2].xyz * v.normal.z);
				worldNormal = mul((float3x3)UNITY_MATRIX_V, worldNormal);
				float2 direction;
				float s = sin(_Angle);
				float c = cos(_Angle);
				direction.x = dot(worldNormal.xy, float2(c, -s));
				direction.y = dot(worldNormal.xy, float2(s, c));
				o.uv_MatCap.xy = direction * 0.5 + 0.5;
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 matCapColor = tex2D(_MatCap, i.uv_MatCap);
				fixed4 finalColor = (mainColor + matCapColor * 2.0 - 1.0) * _Color;
				fixed grayScale = Luminance(finalColor.rgb);
				finalColor = float4(grayScale,grayScale,grayScale,1);
				
				return finalColor;
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}


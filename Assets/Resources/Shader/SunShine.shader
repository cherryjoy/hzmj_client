Shader "CM/SunShine" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "b" {}
	}

	Category 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
		Blend SrcAlpha One
		Cull Off 
		Lighting Off 
		ZTest Off
		ZWrite Off
		Fog { Mode Off }
		SubShader {
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				uniform float scale_;
				sampler2D _MainTex;
				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};
				struct v2f 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					float4 tmp = v.vertex;
					tmp.xyz *= scale_;
					tmp.xy = tmp.xy + v.texcoord1;
					o.vertex = mul(UNITY_MATRIX_MVP, tmp);
					o.uv = v.texcoord;
					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{					
					fixed4 col = tex2D(_MainTex, i.uv);
					return col * i.color;
				}

				ENDCG
				/*
				SetTexture[_MainTex]
				{
					combine texture
				}
				*/

				
			}
		} 
	}
}


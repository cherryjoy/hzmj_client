Shader "Custom/DepthMask" 
{
	Properties
	{
		_MainTex ("MainTexture", 2D) = "white" {}
	}
	
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			ColorMask 0
			
			CGPROGRAM
			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			struct v2f
			{
				float4 pos	: SV_POSITION;
				float2 main_uv 	: TEXCOORD0;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.main_uv = TRANSFORM_TEX(v.texcoord, _MainTex);
								
				return o;
			}
								
			float4 frag (v2f i) : COLOR
			{
				float4 mainColor = tex2D(_MainTex, i.main_uv);			
				float4 finalColor = mainColor;
				
				return finalColor;
			}
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}

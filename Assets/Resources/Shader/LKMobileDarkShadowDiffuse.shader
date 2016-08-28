Shader "Custom/LKMobileDarkShadowDiffuse" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Color ("Color" , Color ) = (1,1,1,1)
		_ShadowOffset("ShadowOffset",Float) = 0.5
	}
	
	SubShader 
	{
		Tags{"RenderType"="Opaque"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass 
		{
			CGPROGRAM
			//#pragma target 2.0
			//#pragma exclude_renderers gles
			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _ShadowOffset;
									
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				SHADOW_COORDS(2)
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o, o.pos);
				TRANSFER_SHADOW(o);
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 finalColor = mainColor * _Color;
				UNITY_APPLY_FOG(i.fogCoord, finalColor.rgb);
				
				half attenuation = SHADOW_ATTENUATION(i);
				finalColor.rgb *= saturate(attenuation + _ShadowOffset);
				
				return finalColor;
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

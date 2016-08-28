Shader "CM/MobileDiffuseMask" 
{
	Properties 
	{
		_MainTex ("Base1 ", 2D) = "white" {}
		_MainTex2 ("Base2", 2D) = "white" {}
		_MaskTex ("Mask", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags {"RenderQueue"="AlphaTest-1" "RenderType"="Opaque" }
		LOD 150

		Pass 
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			//#pragma target 2.0
			//#pragma exclude_renderers gles
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _MainTex2;
			float4 _MainTex2_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;

			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_Texture0: TEXCOORD0;
				float2 uv_Texture1: TEXCOORD1;
				float2 uv_MaskTexture: TEXCOORD3;
				#if defined(LIGHTMAP_ON)
					float2 lmap : TEXCOORD4;
				#endif
				UNITY_FOG_COORDS(5)
			};
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_Texture0 = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_Texture1 = TRANSFORM_TEX(v.texcoord, _MainTex2);
				o.uv_MaskTexture = TRANSFORM_TEX(v.texcoord, _MaskTex);
				
				#if defined(LIGHTMAP_ON)
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				
				UNITY_TRANSFER_FOG(o,o.pos);
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{	
				fixed4 color1 = tex2D(_MainTex, i.uv_Texture0);
				fixed4 color2 = tex2D(_MainTex2, i.uv_Texture1);
				fixed4 maskColor = tex2D(_MaskTex, i.uv_MaskTexture);
				
				fixed4 finalColor;
				finalColor.rgb = color1.rgb * maskColor.r + color2.rgb * (1 - maskColor.r);
				finalColor.a = color1.a * maskColor.r + color2.a * (1 - maskColor.r);
				
				#if defined(LIGHTMAP_ON)
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
					finalColor.rgb *= lm;
				#endif
							
				UNITY_APPLY_FOG(i.fogCoord,finalColor);
				UNITY_OPAQUE_ALPHA(finalColor.a);
							
				return finalColor;
			}
			
			ENDCG
		}
	}

	Fallback "Mobile/VertexLit"
}

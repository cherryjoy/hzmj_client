Shader "CM/MobileDiffuseColorMask" 
{
	Properties 
	{
		_Texture0 ("Texture_R ", 2D) = "white"{}
		_Texture1 ("Texture_G", 2D) = "white"{}
		_Texture2 ("Texture_B",2D) = "white"{}
		_MaskTexture ("MaskTexture", 2D) = "white"{}
		_MaskAlphaPercent("MaskAlphaPercent",Range(0,1)) = 0
	}
	
	SubShader
	{
		Tags { "RenderQueue"="AlphaTest - 1" "IgnoreProjector"="true" "RenderType"="Opaque"}
		LOD 200
		
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
			
			sampler2D _Texture0;
			float4 _Texture0_ST;
			sampler2D _Texture1;
			float4 _Texture1_ST;
			sampler2D _Texture2;
			float4 _Texture2_ST;
			sampler2D _MaskTexture;
			float4 _MaskTexture_ST;
			float _MaskAlphaPercent;

			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_Texture0: TEXCOORD0;
				float2 uv_Texture1: TEXCOORD1;
				float2 uv_Texture2: TEXCOORD2;
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
				o.uv_Texture0 = TRANSFORM_TEX(v.texcoord, _Texture0);
				o.uv_Texture1 = TRANSFORM_TEX(v.texcoord, _Texture1);
				o.uv_Texture2 = TRANSFORM_TEX(v.texcoord, _Texture2);
				o.uv_MaskTexture = TRANSFORM_TEX(v.texcoord, _MaskTexture);
				
				#if defined(LIGHTMAP_ON)
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				
				return o;
			}
				
			half4 frag (v2f i) : COLOR
			{	
				fixed4 color1 = tex2D(_Texture0, i.uv_Texture0);
				fixed4 color2 = tex2D(_Texture1, i.uv_Texture1);
				fixed4 color3 = tex2D(_Texture2, i.uv_Texture2);
				fixed4 maskColor = tex2D(_MaskTexture, i.uv_MaskTexture);
				
				fixed4 finalColor;
				finalColor.rgb = color1.rgb * color1.a * maskColor.r + color2.rgb * color2.a * maskColor.g  + color3.rgb * color3.a * maskColor.b;
				finalColor.a = _MaskAlphaPercent;
				
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

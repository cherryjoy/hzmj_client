Shader "CM/RimAlpha" 
{
	Properties
	{
		_MainTexture ("MainTexture", 2D) = "white" {}
		_MaskTexture ("MaskTexture", 2D) = "white" {}
		_ScrollTexture("ScrollTexture", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
		_Alpha("Alpha",Float) = 1
		_AlphaPower("AlphaPower",Float) = 1
		_AlphaScale("AlphaScale",Float) = 1
		_AlphaBias("_AlphaBias",Float) = 0
		_AlphaTest("_AlphaTest",Float) = 0
		_HSpeed("HSpeed",Float) = 1
		_VSpeed("VSpeed",Float) = 1
	}
	
	Subshader
	{
		LOD 100
		Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque"}			
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Greater [_AlphaTest]
			ZTest Always
			ColorMask 0
					
			CGPROGRAM
			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
						
			struct v2f
			{
				float4 pos	: SV_POSITION;
				float2 main_uv 	: TEXCOORD0;
				float2 mask_uv : TEXCOORD1;
				float2 scroll_uv : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				float3 normal : TEXCOORD4;
				fixed4 color :COLOR;
			};

			sampler2D _MainTexture;
			float4 _MainTexture_ST;
			sampler2D _MaskTexture;
			float4 _MaskTexture_ST;
			sampler2D _ScrollTexture;
			float4 _ScrollTexture_ST;
			float _Alpha;
			float _AlphaPower;
			float _AlphaScale;
			float _AlphaBias;
			float _AlphaTest;
			fixed4 _Color;
			float _HSpeed;
			float _VSpeed;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.main_uv = TRANSFORM_TEX(v.texcoord, _MainTexture);
				o.mask_uv = TRANSFORM_TEX(v.texcoord, _MaskTexture);
				o.scroll_uv = TRANSFORM_TEX(v.texcoord, _ScrollTexture);
				
				o.scroll_uv.x += _Time.y * _HSpeed;
				o.scroll_uv.y += _Time.y * _VSpeed;
				
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				
				float3 worldNormal = normalize(_World2Object[0].xyz * v.normal.x + _World2Object[1].xyz * v.normal.y + _World2Object[2].xyz * v.normal.z);
				o.normal = normalize(worldNormal);
				
				o.color = v.color * _Color;
								
				return o;
			}
							
			float4 frag (v2f i) : COLOR
			{
				float4 mainColor = tex2D(_MainTexture, i.main_uv);
				float4 maskColor = tex2D(_MaskTexture,i.mask_uv);
				float4 scrollColor = tex2D(_ScrollTexture,i.scroll_uv);
				
				float nDotV = saturate(dot(normalize(i.viewDir), i.normal));
				float fresnel = pow(nDotV,_AlphaPower) * _AlphaScale + _AlphaBias;
				
				float4 finalColor;
				finalColor.rgb = mainColor.rgb * scrollColor.rgb;
				finalColor.a = maskColor.a * _Alpha * fresnel * i.color.a;
				
				return finalColor;
			}
			ENDCG
		}
		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			AlphaTest Greater [_AlphaTest]
			
			CGPROGRAM
			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos	: SV_POSITION;
				float2 main_uv 	: TEXCOORD0;
				float2 mask_uv : TEXCOORD1;
				float2 scroll_uv : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				float3 normal : TEXCOORD4;
				fixed4 color :COLOR;
			};

			sampler2D _MainTexture;
			float4 _MainTexture_ST;
			sampler2D _MaskTexture;
			float4 _MaskTexture_ST;
			sampler2D _ScrollTexture;
			float4 _ScrollTexture_ST;
			float _Alpha;
			float _AlphaPower;
			float _AlphaScale;
			float _AlphaBias;
			float _AlphaTest;
			fixed4 _Color;
			float _HSpeed;
			float _VSpeed;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.main_uv = TRANSFORM_TEX(v.texcoord, _MainTexture);
				o.mask_uv = TRANSFORM_TEX(v.texcoord, _MaskTexture);
				o.scroll_uv = TRANSFORM_TEX(v.texcoord, _ScrollTexture);
				
				o.scroll_uv.x += _Time.y * _HSpeed;
				o.scroll_uv.y += _Time.y * _VSpeed;
				
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				
				float3 worldNormal = normalize(_World2Object[0].xyz * v.normal.x + _World2Object[1].xyz * v.normal.y + _World2Object[2].xyz * v.normal.z);
				o.normal = normalize(worldNormal);
				
				o.color = v.color * _Color;
								
				return o;
			}
							
			float4 frag (v2f i) : COLOR
			{
				float4 mainColor = tex2D(_MainTexture, i.main_uv);
				float4 maskColor = tex2D(_MaskTexture,i.mask_uv);
				float4 scrollColor = tex2D(_ScrollTexture,i.scroll_uv);
				
				float nDotV = saturate(dot(normalize(i.viewDir), i.normal));
				float fresnel = pow(nDotV,_AlphaPower) * _AlphaScale + _AlphaBias;
				
				float4 finalColor;
				finalColor.rgb = mainColor.rgb * scrollColor.rgb;
				finalColor.a = maskColor.a * _Alpha * fresnel * i.color.a;
				
				return finalColor;
			}
			ENDCG
		}	
	
	}
		
	Fallback "VertexLit"
}

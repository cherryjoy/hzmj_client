Shader "CM/SoftRimAlpha"
{
	Properties
	{
		_MainTexture ("MainTexture", 2D) = "white" {}
		_MaskTexture ("MaskTexture", 2D) = "white" {}
		_ScrollTexture("ScrollTexture", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
		_AllPower("AllPower",Float) = 1
		_InnerColor("InnerColor",Color) = (1,1,1,1)
		_InnerPower("InnerPower",Float) = 1
		_InnerAlphaPower("InnerAlphaPower",Float) = 1
		_InnerBias("InnerBias",Float) = 0
		_RimColor("RimColor",Color) = (1,1,1,1)
		_RimPower("RimPower",Float) = 2.5
		_RimAlphaPower("RimAlphaPower",Float) = 4.0
		_RimBias("RimBias",Float) = 0
		_HSpeed("HSpeed",Float) = 1
		_VSpeed("VSpeed",Float) = 1
	}

	Subshader
	{
		LOD 100
		Tags { "Queue" = "Transparent" }

		Pass
		{
			BlendOp Add
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Less
			ZWrite Off
			
			CGPROGRAM
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
			fixed4 _Color;
			float _AllPower;
			fixed4 _InnerColor;
			float _InnerPower;
			float _InnerAlphaPower;
			float _InnerBias;
			fixed4 _RimColor;
			float _RimPower;
			float _RimAlphaPower;
			float _RimBias;
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
				o.normal = normalize(mul(_Object2World, float4(v.normal,0)).xyz);
				
				o.color = v.color * _Color;
				
				return o;
			}

			float4 frag (v2f i) : COLOR
			{
				float4 mainColor = tex2D(_MainTexture, i.main_uv);
				float4 maskColor = tex2D(_MaskTexture,i.mask_uv);
				float4 scrollColor = tex2D(_ScrollTexture,i.scroll_uv);
				float nDotV = saturate(dot(normalize(i.viewDir), i.normal));

				fixed4 rimColor;
				rimColor.rgb = _RimColor.rgb * (saturate(pow(nDotV,_RimPower))) * _AllPower + _RimBias;
				rimColor.a = saturate(pow(nDotV,_RimAlphaPower)) * _AllPower;

				fixed4 innerColor;
				innerColor.rgb = _InnerColor.rgb * saturate(pow(nDotV,_InnerPower)) * _AllPower + _InnerBias;
				innerColor.a = saturate(pow(nDotV,_InnerAlphaPower)) * _AllPower;

				float4 finalColor;
				finalColor.rgb = mainColor.rgb * scrollColor.rgb * rimColor.rgb * innerColor.rgb * i.color.rgb;
				finalColor.a = mainColor.a * maskColor.r * rimColor.a * i.color.a;

				return finalColor;
			}
			ENDCG
		}
	}

	Fallback "VertexLit"
}

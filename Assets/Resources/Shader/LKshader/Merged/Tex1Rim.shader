Shader "LK/Transparent/Tex1Rim"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", float) = 1
		_MainTex ("Main Tex", 2D) = "white" {}

		_RimColor ("Rim Color", Color) = (0.5,0.5,0.5,0.5)
		_InnerColor ("Inner Color", Color) = (0.5,0.5,0.5,0.5)
		_InnerColorPower ("Inner Color Power", Range(0.0,1.0)) = 0.5
		_RimPower ("Rim Power", Range(0.0,5.0)) = 2.5
		_AlphaPower ("Alpha Rim Power", Range(0.0,8.0)) = 4.0
		_AllPower ("All Power", Range(0.0, 10.0)) = 1.0
		
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blend Op", float) = 0 //Add
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend Src Factor", float) = 5  //SrcAlpha
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend Dst Factor", float) = 10 //OneMinusSrcAlpha
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", float) = 2 //Back
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", float) = 4 //LessEqual
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", float) = 0 //Off
	}

	CGINCLUDE
	ENDCG

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Pass
		{
			Tags { "LIGHTMODE"="Always" }
			Lighting Off
			Fog { Mode Off }
			BlendOp [_BlendOp]
			Blend [_BlendSrc] [_BlendDst]
			Cull [_CullMode]
			ZTest [_ZTest]
			ZWrite [_ZWrite]

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
	
			float4 _RimColor;
			float _RimPower;
			float _AlphaPower;
			float _AlphaMin;
			float _InnerColorPower;
			float _AllPower;
			float4 _InnerColor;

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				half3 normal: TEXCOORD1;
				half3 viewDir : TEXCOORD2;
                float2 uv : TEXCOORD0;
			};
			
			float4 _Color;
			float _Intensity;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _AddTex;
			float4 _AddTex_ST;
			
			float _UVRotateSpeed;
			float _USpeed;
			float _VSpeed;
			float _USpeed1;
			float _VSpeed1;

			half _ForceX;
			half _ForceY;
			float _HeatTime;
			
			inline float2 Calculate_UVAnim(float2 uv, float uSpeed, float vSpeed)
			{
				float time = _Time.z;
				float absUOffsetSpeed   = abs(uSpeed);
				float absVOffsetSpeed   = abs(vSpeed);

				if (absUOffsetSpeed > 0)
				{
					uv.x += frac(time * uSpeed);
				}

				if (absVOffsetSpeed > 0)
				{
					uv.y += frac(time * vSpeed);
				}

				return uv;
			}

			inline float2 Calculate_UVRotate(float2 uv, float uvRotateSpeed)
			{
				const half TWO_PI = 3.14159 * 2;
				const half2 VEC_CENTER = half2(0.5h, 0.5h);

				float time = _Time.z;
				float absUVRotSpeed = abs(uvRotateSpeed);
				half2 finaluv = uv;
				if (absUVRotSpeed > 0)
				{
					finaluv -= VEC_CENTER;
					half rotation = TWO_PI * frac(time * uvRotateSpeed);
					half sin_rot = sin(rotation);
					half cos_rot = cos(rotation);
					finaluv = half2(
						finaluv.x * cos_rot - finaluv.y * sin_rot,
						finaluv.x * sin_rot + finaluv.y * cos_rot);
					finaluv += VEC_CENTER;
				}
				uv = finaluv;
				return uv;
			}

			inline float2 Calculate_NoiseFromTex(float2 uv, sampler2D addTex)
			{
				float4 time = _Time;
				half offsetColor1 = tex2D(addTex, uv + frac(time.xz * _HeatTime));
				half offsetColor2 = tex2D(addTex, uv + frac(time.yx * _HeatTime));
				uv.x += (offsetColor1 - 0.5h) * _ForceX;
				uv.y += (offsetColor2 - 0.5h) * _ForceY;
				return uv;
			}

			v2f vert(appdata i)
			{
				v2f o;
				o.color = i.color * _Color;
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _MainTex);
				
				float3 worldPos = mul(_Object2World, i.vertex).xyz;
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
				o.normal = normalize(mul(_Object2World, float4(i.normal,0)).xyz);

				return o;
			}

			fixed4 frag(v2f i) : COLOR0
			{
				fixed4 color = i.color * _Intensity;
				color *= tex2D(_MainTex, i.uv);

				half rim = 1.0 - saturate(dot (i.viewDir, i.normal));
				half3 rimColor = _RimColor.rgb * pow (rim, _RimPower)*_AllPower+(_InnerColor.rgb*2*_InnerColorPower);
				half rimA = (pow (rim, _AlphaPower))*_AllPower;
				color *= fixed4(rimColor, rimA);

				return color;
			}
	
			ENDCG
		} 
	}
	//Fallback "VertexLit"
}
Shader "LK/Transparent/Tex2Detail"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", float) = 1
		_MainTex ("Main Tex", 2D) = "white" {}
		_USpeed ("USpeed", Float) = 0
		_VSpeed ("VSpeed", Float) = 0
		_AddTex ("Add Tex", 2D) = "white" {}
		_UVRotateSpeed ("UVRotateSpeed", Float) = 0

		_USpeed1 ("USpeed1", Float) = 0
		_VSpeed1 ("VSpeed1", Float) = 0

		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blend Op", float) = 0 //Add
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend Src Factor", float) = 5  //SrcAlpha
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend Dst Factor", float) = 10 //OneMinusSrcAlpha
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", float) = 2 //Back
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", float) = 4 //LessEqual
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", float) = 0 //Off
		[KeywordEnum(Method1,Method2)] _Option("Option",Int) = 0
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
			#pragma shader_feature __ _OPTION_METHOD1 _OPTION_METHOD2

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
                float2 uv[2] : TEXCOORD0;
				fixed4 color : COLOR;
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
				o.uv[0] = TRANSFORM_TEX(i.uv, _MainTex);
				o.uv[1] = TRANSFORM_TEX(i.uv, _AddTex);

#if _OPTION_METHOD1
				//Add tex uv rotate
				o.uv[1] = Calculate_UVRotate(o.uv[1], _UVRotateSpeed);
#else
				//main tex uv offset
				o.uv[0] = Calculate_UVAnim(o.uv[0], _USpeed, _VSpeed);

				//Add tex uv offset
				o.uv[1] = Calculate_UVAnim(o.uv[1], _USpeed1, _VSpeed1);
#endif

				return o;
			}

			fixed4 frag(v2f i) : COLOR0
			{
				fixed4 color = i.color * _Intensity;
				fixed4 mainColor = tex2D(_MainTex, i.uv[0]);
				fixed4 addColor = tex2D(_AddTex, i.uv[1]);
				mainColor = mainColor * addColor * (mainColor + addColor);
				color *= mainColor;
				return color;
			}

			ENDCG
		}
	}
	//Fallback "VertexLit"
}

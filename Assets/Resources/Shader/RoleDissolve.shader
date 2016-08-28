Shader "CM/RoleDissolve" 
{
	Properties 
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_NoiseTex ("NoiseTex", 2D) = "white" {}
		_FireColor ("FireColor", Color) = (1, 1, 0, 1)
		_TimeSinceEnter ("TimeSinceEnter", float) = 0
		_TimeDuration ("TimeDuration", float) = 2			//恢复时间
		_DissolveSpeed ("DissolveSpeed", float) = 0.3	//消融速度
		_ColorAnimate ("ColorAnimate", vector) = (0,1,0,0)
		_StartAmount("StartAmount", Range(0.01, 1)) = 0.2
		_MatCap ("MatCap", 2D) = "white" {}
		_Angle ("Angle", Range(-3.14, 3.14)) = 0
	}

	SubShader 
	{
		Pass
		{
			Tags {  "RenderType"="Opaque"}
			LOD 250

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_NoiseTex : TEXCOORD1;
				float2 uv_MatCap : TEXCOORD2;
			};
			
			float _TimeSinceEnter;
			float _TimeDuration;
			float _DissolveSpeed;
			fixed4 _FireColor;
			half4 _ColorAnimate;
			float _StartAmount;
			sampler2D _MainTex;
			sampler2D _NoiseTex;
			float4 _MainTex_ST;
			float4 _NoiseTex_ST;
			
			sampler2D _MatCap;
			float _Angle;
			
			static half3 Color = float3(1,1,1);
							
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_NoiseTex = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				
				float3 worldNorm = normalize(_World2Object[0].xyz * v.normal.x + _World2Object[1].xyz * v.normal.y + _World2Object[2].xyz * v.normal.z);
				worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
				float2 direct;
				float s = sin(_Angle);
				float c = cos(_Angle);
				direct.x = dot(worldNorm.xy, float2(c, -s));
				direct.y = dot(worldNorm.xy, float2(s, c));
				o.uv_MatCap.xy = direct * 0.5 + 0.5;
				
				return o;
			}
									
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 finalColor;
				
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 capColor = tex2D(_MatCap,i.uv_MatCap);
				
				finalColor.rgb = mainColor.rgb;
				fixed4 noi =  tex2D(_NoiseTex, i.uv_NoiseTex);
				half timeVal = (_Time.y - _TimeSinceEnter) * _DissolveSpeed;
				half clipAmount = noi.r - timeVal;
				clip(clipAmount);
				if (clipAmount < _StartAmount)
				{
					half lit = clipAmount / _StartAmount;
					Color.x = lerp(_FireColor.r, lit, _ColorAnimate.x);
					Color.y = lerp(_FireColor.g, lit, _ColorAnimate.y);
					Color.z = lerp(_FireColor.b, lit, _ColorAnimate.z);
					half mulFactor = Color.x + Color.y + Color.z;
					finalColor.rgb *= Color.xyz * mulFactor * mulFactor;
				}
				
				finalColor = finalColor + capColor * 2 - 1.0;
				finalColor.a = mainColor.a;
				
				return finalColor;
			}
			ENDCG
		}
	}

	FallBack "Mobile/VertexLit"
}


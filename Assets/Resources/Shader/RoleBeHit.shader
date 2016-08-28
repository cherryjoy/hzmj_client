Shader "CM/RoleHit" 
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_MatCap ("MatCap", 2D) = "white" {}
		_Angle ("Angle", Range(-3.14, 3.14)) = 0
		_Color("Color",Color) = (1,1,1,1)
		_Alpha ("Alpha", Range(0, 1)) = 1		
		_SpecularColor ("Specular Color", Color) = (1,1,1,1)
		_FresnelPower ("Fresnel Power", Range(2,15)) = 5
		_FresnelScale ("Fresnel Scale", Range(0,20)) = 0.5
		_TimeDuration ("duration", float) = 0.5
	}
	
	Subshader
	{
		LOD 250
			
		Pass
		{
			CGPROGRAM
			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
				
			struct v2f
			{
				float4 pos	: SV_POSITION;
				float2 uv 	: TEXCOORD0;
				float2 cap	: TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				//float3 lightDir : TEXCOORD3;
				float3 normal : TEXCOORD3;
			};

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform sampler2D _MatCap;
			uniform float _Angle;
			fixed4 _Color;
			float _Alpha;
			float4 _SpecularColor;
			float _FresnelPower;
			float _FresnelScale;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				
				float3 worldNorm = normalize(_World2Object[0].xyz * v.normal.x + _World2Object[1].xyz * v.normal.y + _World2Object[2].xyz * v.normal.z);
				o.normal = normalize(worldNorm);
				
				worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
				float2 direct;
				float s = sin(_Angle);
				float c = cos(_Angle);
				direct.x = dot(worldNorm.xy, float2(c, -s));
				direct.y = dot(worldNorm.xy, float2(s, c));
				o.cap.xy = direct * 0.5 + 0.5;
								
				return o;
			}
							
			float4 frag (v2f i) : COLOR
			{
				float nDotV = saturate(dot(normalize(i.viewDir), i.normal));
				
				float4 mainColor = tex2D(_MainTex, i.uv);
				
				float gray = 0.333f * (mainColor.r + mainColor.g + mainColor.b);
				half rim = 1.0 - nDotV;
				float fresnel = pow(rim, _FresnelPower) * _FresnelScale * gray ;
				
				float4 woundColor;
				woundColor.rgb = mainColor + (fresnel * _SpecularColor.rgb);
				woundColor.a = mainColor.a;			
								
				fixed4 matCapColor = tex2D(_MatCap, i.cap);
				fixed4 finalColor = (woundColor + (matCapColor * 2.0) - 1.0);
				finalColor = finalColor * _Color;
				finalColor.a = mainColor.a * _Alpha * _Color.a;
				clip(finalColor.a-0.3);
				
				return finalColor;
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

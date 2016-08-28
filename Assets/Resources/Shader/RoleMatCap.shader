Shader "CM/Role-MatCap"
{
	Properties
	{
		_MainTex ("MainTex", 2D) = "white" {}
		_MatCap ("MatCap", 2D) = "white" {}
		_MatCapFactor("MatCapFactor",Float) = 1
		_Angle ("Angle", Range(-3.14, 3.14)) = 0
		_Color("Color",Color) = (1,1,1,1)
		
	}
	
	Subshader
	{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 100		
		
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On ZTest LEqual

			CGPROGRAM
			
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}
				
		Pass
		{
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#pragma multi_compile_fog
			
			struct v2f
			{
				float4 pos	: SV_POSITION;
				float2 uv 	: TEXCOORD0;
				float2 cap	: TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};
			
			uniform float4 _MainTex_ST;
			uniform float _Angle;
			float _MatCapScale;
			float _MatCapOffset;
			uniform fixed4 _Color;
			float _MatCapFactor;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				float3 worldNorm = normalize(_World2Object[0].xyz * v.normal.x + _World2Object[1].xyz * v.normal.y + _World2Object[2].xyz * v.normal.z);
				worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
				float2 direct;
				//direct.x = worldNorm.x * cos(_Angle) - sin(_Angle) * worldNorm.y;
				//direct.y = worldNorm.y * cos(_Angle) + sin(_Angle) * worldNorm.x;
				float s = sin(_Angle);
				float c = cos(_Angle);
				direct.x = dot(worldNorm.xy, float2(c, -s));
				direct.y = dot(worldNorm.xy, float2(s, c));
				o.cap.xy = direct * 0.5 + 0.5;
									
				UNITY_TRANSFER_FOG(o,o.pos);
				
				return o;
			}
			
			uniform sampler2D _MainTex;
			uniform sampler2D _MatCap;
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed4 mc = tex2D(_MatCap, i.cap);
				fixed4 o = (tex + mc * _MatCapFactor * 2.0  - 1.0);
				o.rgb =o.rgb * _Color.rgb;
				o.a = tex.a;
				
				clip(o.a - 0.3);
				
				UNITY_APPLY_FOG(i.fogCoord,o);
				UNITY_OPAQUE_ALPHA(o.a);
				
				return o;
			}
			ENDCG
		}
	}
}
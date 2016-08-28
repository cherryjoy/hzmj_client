Shader "CM/Grass" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Main Texture", 2D) = "white" {}
	_SwingScale ("Swing Scale", Range(0.001, 1)) = 0.1
	_Cutoff ("Cut Off Alpha", Range(0.01, 1)) = 0.4
	//_WindTex ("Wind Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="AlphaTest+1" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Blend SrcAlpha OneMinusSrcAlpha
	//AlphaTest Greater .05
	ColorMask RGB
	Cull Off// Lighting Off ZWrite Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			//sampler2D _WindTex;
			fixed4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				//float3 nor : NORMAL;	//pos offset
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};
			
			float4 _MainTex_ST;
			float _SwingScale;
			float _Cutoff;

			v2f vert (appdata_t v)
			{
				//v.vertex += float4(v.nor, 0);
				//float3 worldPos = mul((float4x4)_Object2World, v.vertex).xyz;
				float cosA, sinA;
				sincos(_Time.y, cosA, sinA);
				v.vertex.xz += float2(cosA, sinA) * v.texcoord.y * _SwingScale;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 finalColor = i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				clip(finalColor.a - _Cutoff);
				
				UNITY_APPLY_FOG(i.fogCoord,finalColor);
				UNITY_OPAQUE_ALPHA(finalColor.a);
				
				return finalColor;
			}
			ENDCG 
		}
	}	
}
}

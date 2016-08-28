Shader "CM/Transparent/ShadowPlane" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

	Category 
	{
		Tags {"Queue"="Geometry+1" "IgnoreProjector"="true" "RenderType"="Transparent"}
		Blend SrcAlpha  OneMinusSrcAlpha
		Cull Off 
		Lighting Off 
		ZWrite Off
		ZTest Off
		SubShader {
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _Color;
				struct appdata_t 
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};
				struct v2f 
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.texcoord;
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{					
					fixed4 col = tex2D(_MainTex, i.uv);
					return col * _Color;
				}
				ENDCG
			}
		}
	}

	Fallback "Transparent/VertexLit"
}


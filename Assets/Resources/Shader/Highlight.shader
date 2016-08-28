Shader "Unlit/Transparent Highlight" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Color ("Color",Color) = (1,1,1,1)
		_Power ("Power",Float) = 1.2
	}
	
	SubShader 
	{
		LOD 100
		Cull Off
		Lighting Off
		ZWrite Off
		Offset -1, -1
		Fog { Mode Off }
		ColorMask RGB
		AlphaTest Off
		Blend SrcAlpha OneMinusSrcAlpha
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Power;
						
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_MatCap : TEXCOORD1;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);

				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = v.texcoord;
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 finalColor = mainColor * _Color * _Power;
							
				return finalColor;
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}


Shader "Custom/FadeTexture" 
{
	Properties 
	{
		_FirstTexture ("FirstTexture", 2D) = "white" {}
		_SecondTexture ("FirstTexture", 2D) = "white" {}
		_Percent ("Percent",Range(0,1)) = 0
	}

	SubShader 
	{
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			uniform sampler2D _FirstTexture;
			uniform sampler2D _SecondTexture;
			uniform fixed _Percent;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert( appdata_img v )
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv =  v.texcoord.xy;

				return o;
			}

			half4 frag (v2f i) : Color
			{
				half4 finalColor;
				half4 firstColor = tex2D (_FirstTexture, i.uv);
				half4 secondColor = tex2D (_SecondTexture,i.uv);

				finalColor.rgb = firstColor.rgb * (1.0 - _Percent) + secondColor.rgb * _Percent;
				finalColor.a = firstColor.a * (1.0 - _Percent) + secondColor.a * _Percent;

				return finalColor;
			}
			
			ENDCG
		}
	}
	Fallback "VertexLit"
}

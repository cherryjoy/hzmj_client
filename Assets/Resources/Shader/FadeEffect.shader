Shader "Custom/FadeEffect"
{

	Properties 
	{
		_ColorBuffer ("Color", 2D) = "" {}
		_Color ("EffectColor", Color) = (1,1,1,1)
		_Percent("Percent",Range(0,1)) = 0
	}

	SubShader 
	{
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _ColorBuffer;
			uniform fixed4 _Color;
			uniform half _Percent;
 
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert( appdata_img v )
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv.xy =  v.texcoord.xy;

				return o;
			}

			half4 frag (v2f i) : Color
			{
				half4 finalColor;
				half4 sourceColor = tex2D (_ColorBuffer, i.uv.xy);

				finalColor.rgb = sourceColor.rgb * (1.0 - _Percent) + _Color.rgb * _Percent;
				finalColor.a = sourceColor.a * (1.0 - _Percent) + _Color.a * _Percent;

				return finalColor;
			}

			ENDCG
		}
	}

	Fallback off
}

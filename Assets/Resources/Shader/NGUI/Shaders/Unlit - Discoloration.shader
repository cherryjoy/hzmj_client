Shader "Unlit/Discoloration"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Offset -1, -1
			Fog { Mode Off }
			ColorMask RGB
			AlphaTest Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			struct v2f
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv_MainTex : TEXCOORD0;
			};

			v2f vert (appdata_full
			v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 finalColor = mainColor * _Color;
				fixed grayScale = Luminance(finalColor.rgb);
				finalColor = float4(grayScale,grayScale,grayScale,finalColor.a);
				
				return finalColor;
			}

			ENDCG
		}
	}
}

Shader "Unlit/CircleScreenMask (SoftClip)"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
		_X1("X",Float) = 0.51
		_Y1("Y",Float) = 0.46
		_Radius("Radius",Float) = 0.48
		MaskAlpha("MaskAlpha",Range(0,1)) = 1
		SoftX("SoftX",Float) = 60
		SoftY("SoftY",Float) = 60
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
			float _X1;
			float _Y1;
			float _Radius;
			float MaskAlpha;
			float2 _ClipSharpness;
			float SoftX;
			float SoftY;

			struct appdata_t
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				o.worldPos = TRANSFORM_TEX(v.vertex.xy, _MainTex);
				return o;
			}

			fixed4 frag (v2f IN) : COLOR
			{
				float2 factor = (float2(1.0, 1.0) - abs(IN.worldPos)) * _ClipSharpness;
			    float factorValue = clamp( min(factor.x, factor.y), 0.0, 1.0);

				fixed4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;

				float2 center;
				center.x = _X1;
				center.y = _Y1;

				float2 o;
				o.x = IN.texcoord.x;
				o.y = IN.texcoord.y;

				float pointDistance = length(center - o);
				float alphaFactor = _Radius - pointDistance;

				_ClipSharpness.x = SoftX;
				_ClipSharpness.y = SoftY;

				fixed4 finalColor = mainColor * _Color;
				finalColor.a *= alphaFactor * SoftX * factorValue * MaskAlpha;

				return finalColor;
			}

			ENDCG
		}
	}
}

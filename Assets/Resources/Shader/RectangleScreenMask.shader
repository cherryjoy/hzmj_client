Shader "CM/ScreenMask"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Color("Color",Color) = (0.5,0.5,0.5,1)
		_X1("_X1",Float) = 0
		_Y1("_Y1",Float) = 0
		_X2("_X2",Float) = 0
		_Y2("_Y2",Float) = 0
		MaskAlpha("MaskAlpha",Range(0,1)) = 1
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
			float _X2;
			float _Y2;
			float MaskAlpha;
			
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
				fixed4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;
				
				float2 bottomLeft;
				bottomLeft.x = _X1;
				bottomLeft.y = _Y1;
								
				float2 topRight;
				topRight.x = _X2;
				topRight.y = _Y2;
								
				float2 o;
				o.x = IN.texcoord.x;
				o.y = IN.texcoord.y;
				
				float width = abs(_X2 - _X1) + 0.001;
				float height = abs(_Y2 - _Y1) + 0.001;
				
				float x1 = abs(o.x - bottomLeft.x);
				float x2 = abs(o.x - topRight.x);
				float y1 = abs(o.y - bottomLeft.y);
				float y2 = abs(o.y - topRight.y);
				
				float result1 = (x1 + x2) - width;
				float result2 = (y1 + y2) - height;
				float result = result1 + result2;
										
				float alphaFactor = sign(result);
				
				fixed4 finalColor = mainColor * _Color;
				finalColor.a *= alphaFactor;
				finalColor.a *= MaskAlpha;
				
				return finalColor;
			}
						
			ENDCG
		}
	}
}

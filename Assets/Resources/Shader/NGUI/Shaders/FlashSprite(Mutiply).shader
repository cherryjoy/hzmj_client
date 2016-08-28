Shader "GUI/FlashSprite(Mutiply)" {
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "black" {}
		_MaskColor ("MaskColor (RGB)", Color) = (1,1,1,0)
		_Speed ("Speed" , float) = 0.1
		_TextLength("TextLength", float) = 1
		_TransScaleX ("TransScaleX" , float) = 0.1
		_TransScaleY ("TransScaleY" , float) = 0.1
		_TextOffsetX ("TextOffsetX" , float) = 0
		_TextOffsetY ("TextOffsetY" , float) = 0
	}
	
	SubShader
	{
		LOD 100

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
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			AlphaTest Off
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				
				struct appdata_t
				{
					float4 vertex : POSITION;
					half4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				struct v2f
				{
					float4 vertex : POSITION;
					half4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				sampler2D _MainTex;
				sampler2D _MaskTex;
				float4 _MainTex_ST;
				float4 _MaskTex_ST;
				float _Speed;
				float _TransScaleX;
				float _TransScaleY;
				float _TextLength;
				float _TextOffsetX;
				float _TextOffsetY;
				fixed4 _MaskColor;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					v.vertex.x -= _TextOffsetX;
					v.vertex.y -= _TextOffsetY;
					float currentPos = (v.vertex.x / _TransScaleX)-fmod(_Time.x*_Speed,_TextLength);
					float2 temp = float2(currentPos,v.vertex.y / _TransScaleY);
					
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord1 = TRANSFORM_TEX(temp, _MaskTex); 
					
					o.color = v.color;
					return o;
				}

				half4 frag (v2f i) : COLOR
				{
					half4 col = i.color;
					col = tex2D(_MainTex, i.texcoord);
					half4 colMask = tex2D(_MaskTex,i.texcoord1);
					colMask.rgb = _MaskColor;
					//col.rgb = (1-(1-col.rgb)*col.a)*(1-(1-colMask.rgb)*colMask.a);
					col.rgb = (col.rgb*col.a)+(colMask.rgb*colMask.a);
					
					return col;
				}
			ENDCG
		}
	}
}
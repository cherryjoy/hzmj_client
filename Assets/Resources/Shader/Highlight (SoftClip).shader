Shader "Unlit/Transparent Highlight (SoftClip)" 
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
			float2 _ClipSharpness = float2(20.0, 20.0);
						
			struct v2f 
			{
				float4 pos : POSITION;
				fixed4 color : COLOR;
				float2 uv_MainTex : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.uv_MainTex = v.texcoord;
				o.worldPos = TRANSFORM_TEX(v.vertex.xy, _MainTex);
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float2 factor = (float2(1.0, 1.0) - abs(i.worldPos)) * _ClipSharpness;
				
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 finalColor = mainColor * _Color * _Power;
				finalColor.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);
				
				return finalColor;
				
				
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}


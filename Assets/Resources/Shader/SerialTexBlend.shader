Shader "CM/SerialTexBlend" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_Row("row", int) = 2
	_Col("col", int) = 2
	_Duration("duration", float) = 1
	_TimeSinceEnter("time since enter", float) = 0.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend DstColor One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			int _Row;
			int _Col;
			float _Duration;
			float _TimeSinceEnter;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

				int totoal = _Row * _Col;
				float reci_Col = 1.0 / _Col;
				float reci_Row = 1.0 / _Row;
				int currIdx = ((int)((_Time.y - _TimeSinceEnter) / _Duration * totoal)) % totoal;
				int row = currIdx * reci_Col;
				int col = currIdx - row * _Col;//currIdx % _Col;
				row = _Row - row - 1;
				o.texcoord.x = (o.texcoord.x + col) * reci_Col;
				o.texcoord.y = (o.texcoord.y + row) * reci_Row;
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : COLOR
			{
				return i.color * tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
	}
}
}


Shader "Custom/LKParticleAdditive"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off
		Lighting Off
		ZWrite Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.color = v.color;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 mainColor = tex2D(_MainTex, i.uv);
				fixed4 finalColor = mainColor * i.color;
				
				return finalColor;
			}
			ENDCG
		}
	}
}

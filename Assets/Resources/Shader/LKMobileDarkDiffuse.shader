Shader "Custom/LKMobileDarkDiffuse" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Color ("Color" , Color ) = (1,1,1,1)
		_Clip("Clip",Float) = 0.01
		
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", float) = 4 //LessEqual
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", float) = 1 //On
	}
	
	SubShader 
	{
		Tags{"RenderType"="Opaque"}
		LOD 100
		ZTest [_ZTest]
		ZWrite [_ZWrite]

		Pass 
		{
			CGPROGRAM
			//#pragma target 2.0
			//#pragma exclude_renderers gles
			#pragma multi_compile_fwdbase

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Clip;
									
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				fixed4 finalColor = mainColor * _Color;
				UNITY_APPLY_FOG(i.fogCoord, finalColor.rgb);
				
				clip(finalColor.a - _Clip);
				
				return finalColor;
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

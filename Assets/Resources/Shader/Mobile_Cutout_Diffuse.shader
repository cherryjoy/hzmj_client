Shader "CM/Transparent/CutoutDiffuse" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_Cutoff ("Alpha cutoff", Range(0.01,1)) = 0.5
	}
	
	SubShader 
	{
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Pass 
		{
			Tags { "LightMode" = "ForwardBase" }
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
			fixed4 _Color;
			float _Cutoff;
			
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv_MainTex : TEXCOORD0;
				#if defined(LIGHTMAP_ON)
					float2 lmap : TEXCOORD1;
				#endif
				UNITY_FOG_COORDS(2)
			};
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				#if defined(LIGHTMAP_ON)
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
			
				UNITY_TRANSFER_FOG(o,o.pos);
			
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{	
				fixed4 mainColor = tex2D(_MainTex, i.uv_MainTex);
				
				fixed4 finalColor = mainColor * _Color;
				
				#if defined(LIGHTMAP_ON)
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
					finalColor.rgb *= lm;
				#endif
				
				clip(finalColor.a - _Cutoff);
				
				UNITY_APPLY_FOG(i.fogCoord,finalColor);
				UNITY_OPAQUE_ALPHA(finalColor.a);
				
				return finalColor;
			}
			ENDCG
		}
	}
}

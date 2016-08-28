Shader "CM/Gamma" {
	Properties {
		_MainTex ("Screen Blended", 2D) = "" {}
		_gLevel0 ("gLevel0", Range(-100, 60)) = 1
		_gLevel1 ("gLevel1", Range(-500, 500)) = 1
		_gLevel2 ("gLevel2", Range(-10, 10)) = 1
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f_bp 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f_erode {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	sampler2D _MainTex;
	
	float _gLevel0;
	float _gLevel1;
	float _gLevel2;

	v2f_erode vertErode( appdata_img v ) {
		v2f_erode o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;

		return o;
	}

	
	half4 fragDownSampling (v2f_bp i) : COLOR {
		half4 color = tex2D(_MainTex, i.uv);
		half4 outColor = pow((color * 255.0 - _gLevel0) / (_gLevel1 - _gLevel0), 1 / _gLevel2);
		return outColor;
	}
	
	ENDCG 

Subshader {
	ZTest Always Cull Off ZWrite Off
	Fog { Mode off }
	Pass {
		CGPROGRAM
		#pragma vertex vertErode
		#pragma fragment fragDownSampling
		ENDCG
	}
	
}//subshader

Fallback off
	
} // shader

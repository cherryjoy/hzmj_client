Shader "Custom/CoolDown" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CullOff ("Alpha Test", Range(0.0, 1.0)) = 1.0
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Pass{
		//Tags { "RenderType"="Opaque" }
		Cull Off
		ZWrite Off
		ZTest Always
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float _CullOff;

		struct v2f {
   	 	 float4 pos : SV_POSITION;
    	 float2  uv : TEXCOORD0;
		};

		v2f vert (appdata_base v)
		{
    		v2f o;
   		    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    		o.uv = v.texcoord.xy;
    		return o;
		}

		half4 frag (v2f i) : COLOR
		{
    		half4 texcol = tex2D (_MainTex, i.uv);
    		clip(texcol.a - _CullOff);

			if (texcol.a > 0)
    			texcol.a = 0.86;

    		return texcol;
		}
		ENDCG
	} 
	}
}

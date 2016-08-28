Shader "Hidden/SimpleBloom" {
	Properties {
		_MainTex ("Screen Blended", 2D) = "" {}
		_ColorBuffer ("Color", 2D) = "" {}
	}
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f_ds {
		float4 pos : POSITION;
		float2 uv[5] : TEXCOORD0;
	};
	struct v2f_erode {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float4 uv12 : TEXCOORD1;
		float4 uv34 : TEXCOORD2;
		float4 uv56 : TEXCOORD3;
		float4 uv78 : TEXCOORD4;
		float4 uv910 : TEXCOORD5;
		float4 uv1112 : TEXCOORD6;
	};
	struct v2f_bp 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	struct v2f_blur {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
		half4 uv01 : TEXCOORD1;
		half4 uv23 : TEXCOORD2;
		half4 uv45 : TEXCOORD3;
		half4 uv67 : TEXCOORD4;
	};	
	struct v2f_add {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
	sampler2D _ColorBuffer;
	sampler2D _MainTex;
	
	half4 _Threshhold;//bright pass
	half4 _Offsets;//gauss blur offset
	half _Intensity;//bloom intensity
	half _SourceIntensity;//source intensity
	half4 _ColorBuffer_TexelSize;
	half4 _MainTex_TexelSize;

	v2f_ds vertDownSampling( appdata_img v ) {
		v2f_ds o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv[4] = v.texcoord.xy;
		o.uv[0] = v.texcoord.xy + _MainTex_TexelSize.xy * 0.5;
		o.uv[1] = v.texcoord.xy - _MainTex_TexelSize.xy * 0.5;
		o.uv[2] = v.texcoord.xy - _MainTex_TexelSize.xy * half2(1,-1) * 0.5;
		o.uv[3] = v.texcoord.xy + _MainTex_TexelSize.xy * half2(1,-1) * 0.5;	
		return o;
	}
	
	v2f_erode vertErode( appdata_img v ) {
		v2f_erode o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;

		o.uv12 = v.texcoord.xyxy + _MainTex_TexelSize.xyxy * half4(1, 1, -1, -1) * 0.5;
		o.uv34 = v.texcoord.xyxy + _MainTex_TexelSize.xyxy * half4(1, -1, -1, 1) * 0.5;
		o.uv56 = v.texcoord.xyxy + _MainTex_TexelSize.xyxy * half4(1, 0, -1, 0) * 0.5;
		o.uv78 = v.texcoord.xyxy + _MainTex_TexelSize.xyxy * half4(0, 1, 0, -1) * 0.5;
		o.uv910  = v.texcoord.xyxy + _MainTex_TexelSize.xyxy * half4(2, 0, -2, 0) * 0.5;
		o.uv1112 = v.texcoord.xyxy + _MainTex_TexelSize.xyxy * half4(0, 2, 0, -2) * 0.5;
		return o;
	}
	
	v2f_erode vertGaussBlur (appdata_img v) {
		v2f_erode o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;

		o.uv12 = v.texcoord.xyxy + _Offsets.xyxy * half4(1, 1, -1, -1) * 0.5;
		o.uv34 = v.texcoord.xyxy + _Offsets.xyxy * half4(1, -1, -1, 1) * 0.5;
		o.uv56 = v.texcoord.xyxy + _Offsets.xyxy * half4(1, 0, -1, 0) * 0.5;
		o.uv78 = v.texcoord.xyxy + _Offsets.xyxy * half4(0, 1, 0, -1) * 0.5;
		o.uv910  = v.texcoord.xyxy + _Offsets.xyxy * half4(2, 0, -2, 0) * 0.5;
		o.uv1112 = v.texcoord.xyxy + _Offsets.xyxy * half4(0, 2, 0, -2) * 0.5;
		return o;
	}

	v2f_bp vertBrightPass( appdata_img v ) 
	{
		v2f_bp o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv =  v.texcoord.xy;
		return o;
	}
	v2f_add vertAdd( appdata_img v ) {
		v2f_add o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv[0] =  v.texcoord.xy;
		o.uv[1] =  v.texcoord.xy;
		o.uv[1].y += _ColorBuffer_TexelSize.y;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_ColorBuffer_TexelSize.y < 0)
		{
			o.uv[1].y -= _ColorBuffer_TexelSize.y;
			o.uv[1].y = 1-o.uv[1].y;
		}
		#endif
		return o;
	}
	//=========================================================
	half4 fragBrightPass(v2f_bp i) : COLOR 
	{
		half4 color = tex2D(_MainTex, i.uv);
		color.rgb = color.rgb;
		color.rgb = max(half3(0,0,0), color.rgb-_Threshhold.xxx);
		return color;
	}
	//down sampling with max color
	half4 fragDownSamplingMax (v2f_ds i) : COLOR {
		half4 outColor = tex2D(_MainTex, i.uv[4].xy);
		outColor = max(outColor, tex2D(_MainTex, i.uv[0].xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv[1].xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv[2].xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv[3].xy));
		return outColor;
	}
	//down sampling with average color
	half4 fragDownSampling (v2f_ds i) : COLOR {
		half4 outColor = 0;
		outColor += tex2D(_MainTex, i.uv[0].xy);
		outColor += tex2D(_MainTex, i.uv[1].xy);
		outColor += tex2D(_MainTex, i.uv[2].xy);
		outColor += tex2D(_MainTex, i.uv[3].xy);
		return outColor/4;
	}
	half4 fragGaussBlur (v2f_erode i) : COLOR {
		half4 color = half4 (0,0,0,0);
		color += 0.2 * tex2D (_MainTex, i.uv);
		color += 0.12 * tex2D (_MainTex, i.uv12.xy);
		color += 0.12 * tex2D (_MainTex, i.uv12.zw);
		color += 0.12 * tex2D (_MainTex, i.uv34.xy);
		color += 0.12 * tex2D (_MainTex, i.uv34.zw);

		color += 0.07 * tex2D (_MainTex, i.uv56.xy);
		color += 0.07 * tex2D (_MainTex, i.uv56.zw);
		color += 0.07 * tex2D (_MainTex, i.uv78.xy);
		color += 0.07 * tex2D (_MainTex, i.uv78.zw);

		color += 0.025 * tex2D (_MainTex, i.uv910.xy);
		color += 0.025 * tex2D (_MainTex, i.uv910.zw);
		color += 0.025 * tex2D (_MainTex, i.uv1112.xy);
		//color += 0.025 * tex2D (_MainTex, i.uv1112.zw);
		return color;
	} 

	half4 fragAdd (v2f_add i) : COLOR {
		half4 addedbloom = tex2D(_MainTex, i.uv[0].xy);
		half4 screencolor = tex2D(_ColorBuffer, i.uv[1]);
		return _Intensity * addedbloom + _SourceIntensity * screencolor;
	}
	//erode sampling with max color
	half4 fragErode (v2f_erode i) : COLOR {
		half4 outColor = tex2D(_MainTex, i.uv);
		outColor = max(outColor, tex2D(_MainTex, i.uv12.xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv12.zw));
		outColor = max(outColor, tex2D(_MainTex, i.uv34.xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv34.zw));
		outColor = max(outColor, tex2D(_MainTex, i.uv56.xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv56.zw));
		outColor = max(outColor, tex2D(_MainTex, i.uv78.xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv78.zw));
		outColor = max(outColor, tex2D(_MainTex, i.uv910.xy));
		outColor = max(outColor, tex2D(_MainTex, i.uv910.zw));
		outColor = max(outColor, tex2D(_MainTex, i.uv1112.xy));
		/*
		//12 registers needed to compile program 
		outColor = max(outColor, tex2D(_MainTex, i.uv1112.zw));
		*/
		return outColor;
	}
	ENDCG 

Subshader {
	ZTest Always Cull Off ZWrite Off
	Fog { Mode off }
	// 0: used for "stable" downsampling (blur)
	Pass {
		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vertDownSampling
		#pragma fragment fragDownSampling
		ENDCG
	}
	// 1: used for max downsampling
	Pass {
		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vertDownSampling
		#pragma fragment fragDownSamplingMax
		ENDCG
	}
	//2:bright pass
	Pass {
		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vertBrightPass
		#pragma fragment fragBrightPass
		ENDCG
	}	
	//3 gauss blur
	Pass {
		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma exclude_renderers flash
		#pragma vertex vertGaussBlur
		#pragma fragment fragGaussBlur
		ENDCG
	} 
 
	//4: final pass , dest = (intensity * bloom + source)
	Pass {
		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vertAdd
		#pragma fragment fragAdd
		ENDCG
	}

  //5: erode
	Pass {
		CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vertErode
		#pragma fragment fragErode
		ENDCG
	}
}//subshader

Fallback off
	
} // shader

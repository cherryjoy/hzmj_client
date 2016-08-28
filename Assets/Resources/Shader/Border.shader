Shader "Custom/Border" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+1"}
		LOD 200
		/*
		Pass {
			Cull Off  
        	ZWrite Off 
        	CGPROGRAM  
        	#include "UnityCG.cginc"  
            #pragma vertex vert  
            #pragma fragment frag  
  
            half _OutlineWidth;  
            fixed4 _OutlineColor;  
  
            struct V2F  
            {  
                float4 pos:SV_POSITION;  
            };  
  
            V2F vert(appdata_base IN)  
            {  
                V2F o;  
  
                IN.vertex.xyz += IN.normal * 0.02;  
                o.pos = mul(UNITY_MATRIX_MVP, IN.vertex);  
                return o;  
            }  
  
            fixed4 frag(V2F IN):COLOR  
            {  
                return fixed4(1, 0, 0, 1);  
            }                     
        	ENDCG  
		}
		*/

		

		Pass {

			CGPROGRAM
			#include "UnityCG.cginc"  
			#pragma vertex vert
			#pragma fragment frag

			

			struct vertexInput 
			{
	            float4 vertex : POSITION;
	            float4 texcoord0 : TEXCOORD0;
		    };

	        struct fragmentInput
	        {
	            float4 position : SV_POSITION;
	            float2 texcoord0 : TEXCOORD0;
	        };

			sampler2D _MainTex;

			fragmentInput vert(vertexInput i)
			{
                fragmentInput o;
                o.position = mul(UNITY_MATRIX_MVP, i.vertex);
                o.texcoord0 = i.texcoord0;
                return o;
            }

			float4 frag (fragmentInput i) : COLOR 
			{
				return tex2D(_MainTex, i.texcoord0);
			}
			ENDCG
		}
	} 
}

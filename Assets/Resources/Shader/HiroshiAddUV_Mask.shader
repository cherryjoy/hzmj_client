Shader "Hiroshiryu/Particles/HiroshiAddUV_Mask" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	_MaskTex ("Mask Texture", 2D) = "white" {}
	_SpeedX ("SpeedX", Float) = 0
	_SpeedY ("SpeedY", Float) = 0
	_RotationSpeed ("Rotation Speed", Float) = 35.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	AlphaTest Greater .01
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
			
			
			sampler2D _MaskTex;
			fixed _SpeedX;
			fixed _SpeedY;
			fixed _RotationSpeed;
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				#endif
				float2 texcoord1 : TEXCOORD2;
			};
			
			float4 _MainTex_ST;
			float4 _MaskTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord,_MaskTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : COLOR
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				#endif
				
				fixed2 uvTemp = i.texcoord - 0.5f;
				fixed s1 = sin(_RotationSpeed * _Time);
				fixed c1 = cos ( _RotationSpeed * _Time );
				fixed2x2 rotationMatrix = float2x2( c1, -s1, s1, c1);
				uvTemp = mul ( uvTemp.xy, rotationMatrix );
				uvTemp += 0.5f;
				uvTemp.x += _SpeedX * _Time;
				uvTemp.y += _SpeedY * _Time; 
				fixed4 txColor = tex2D(_MainTex, uvTemp);
				txColor.a *= tex2D(_MaskTex, i.texcoord1).r;
				return 2.0f * i.color * _TintColor * txColor;
			}
			ENDCG 
		}
	}	
}
}


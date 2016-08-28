Shader "Custom/FlowLight"
{
    Properties
    {
        _MainTexture("MainTexture", 2D) = "white" {}
        _LightTexture("LightTexture", 2D) = "white" {}
        _OverLayColor ("OverLayColor", Color) = (1, 1, 1, 1)
		_FlowRateX ("FlowRateX",Range(0,10)) = 1.0
		_FlowRateY ("FlowRateY",Range(0,10)) = 4.0
		_LightPower ("LightPower",Range(0,10)) = 2.0
		_FlowSpeed ("FlowSpeed",Range(0,10)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="transparent" }
        LOD 200
        pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			
            sampler2D _MainTexture;
            sampler2D _LightTexture;
            float4 _OverLayColor;
			float _FlowRateX;
			float _FlowRateY;
			float _LightPower;
			float _FlowSpeed;
  
            struct appdata       
            {      
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };      
  
            struct v2f      
            {      
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float2 uvLight : TEXCOORD1;
            };      

            v2f vert (appdata v)      
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                o.uv = v.texcoord;
				
                half2 normal;
                normal.x = dot(UNITY_MATRIX_IT_MV[0].xyz,v.normal);
                normal.y = dot(UNITY_MATRIX_IT_MV[1].xyz,v.normal);
				
				normal.x = v.normal.x;
				normal.y = v.normal.y;
				
                o.uvLight = normal * 0.5 + 0.5;
				o.uvLight.x += _Time.x * _FlowSpeed * _FlowRateX;
				o.uvLight.y += _Time.x * _FlowSpeed * _FlowRateY;
				
                return o;
            }
			
            float4 frag(v2f i) : COLOR
            {
                float4 textureColor = tex2D(_MainTexture, i.uv);
                float4 lightColor = tex2D(_LightTexture, i.uvLight);
                return textureColor + lightColor * _LightPower * _OverLayColor;
            }
			
            ENDCG  
        }  
    }  
} 
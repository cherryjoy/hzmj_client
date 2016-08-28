Shader "Custom/ScrollUV"
{
    Properties
    {
        _MainTexture("MainTexture", 2D) = "white" {}
		_HSpeed("HSpeed",Float) = 1
		_VSpeed("VSpeed",Float) = 1
    }

    SubShader
    {
        LOD 100
		Tags {"Queue"="Transparent"}
		BLEND SrcAlpha OneMinusSrcAlpha
		
        pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			
            sampler2D _MainTexture;
			float _HSpeed;
			float _VSpeed;
  
            struct appdata       
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
  
            struct v2f      
            {      
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };      

            v2f vert (appdata v)      
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                o.uv = v.texcoord;
				
				o.uv.x += _Time.y * _HSpeed;
				o.uv.y += _Time.y * _VSpeed;
								
                return o;
            }
			
            float4 frag(v2f i) : COLOR
            {
                float4 textureColor = tex2D(_MainTexture, i.uv);
				
				return textureColor;
            }
			
            ENDCG  
        }  
    }  
} 
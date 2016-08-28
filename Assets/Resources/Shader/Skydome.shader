Shader "CM/SkyDome" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader 
	{
		
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off
		ZWrite Off
		
		Pass 
		{
			SetTexture [_MainTex] { combine texture }
		}
	}
	Fallback "Mobile/VertexLit"
}

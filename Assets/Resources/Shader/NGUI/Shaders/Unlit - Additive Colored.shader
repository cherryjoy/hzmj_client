Shader "Unlit/Additive Colored"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		//LOD 100
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		ColorMask RGB
		AlphaTest Greater .01
		//Blend One One
		Blend SrcAlpha One  
		//Blend SrcAlpha OneMinusSrcAlpha
		//Offset -1, -1
		//ColorMaterial AmbientAndDiffuse
		
		Pass
		{
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
}

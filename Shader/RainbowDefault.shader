Shader "Custom/RainbowDefault"
{
    Properties
    {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Vector) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
		_Phase ("Phase", Float) = 0
		_TimeFrequency ("Time Frequency", Float) = 0
        _XFrequency ("X Frequency", Float) = 0
        _YFrequency ("Y Frequency", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
        Cull Off
		ZWrite Off
        Pass
        {
            CGPROGRAM

            #pragma vertex SpriteVert // This comes with UnitySprites.cginc
            #pragma fragment RainbowFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            
            #include "Rainbow.cginc"
            #include "UnitySprites.cginc"
			
			fixed4 RainbowFrag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c = _Color * c;
				c.rgb = hueshift(IN.texcoord, c.rgb);
				return c;
			}
            
            ENDCG
        }
    	
    	
    	
    }
}
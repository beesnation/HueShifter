Shader "Custom/RainbowGrassLit" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Vector) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_SwaySpeed ("SwaySpeed", Float) = 1
		_SwayAmount ("Sway Amount", Float) = 1
		_WorldOffset ("World Offset", Float) = 1
		_HeightOffset ("Height Offset", Float) = 0
		_ClampZ ("Clamp Z Position", Float) = 1
		[PerRendererData] _PushAmount ("Push Amount (Player)", Float) = 0
		_Phase ("Phase", Float) = 0
        _Frequency ("Frequency", Vector) = (0,0,0,0) 
	}
	
	SubShader{
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
        Cull Off
		ZWrite Off
		
		CGPROGRAM
		
        #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
        #pragma target 2.0
        #pragma multi_compile_local _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        
        #include "Rainbow.cginc"
        #include "UnitySprites.cginc"

		float _SwaySpeed;
		float _SwayAmount;
		float _WorldOffset;
		float _HeightOffset;
		float _ClampZ;
		float _PushAmount;
		
		struct Input
		{
            float2 uv_MainTex;
            fixed4 color;
        	fixed3 worldPos;
		};

		void vert(inout appdata_full v, out Input o)
		{
			float offset = _PushAmount + 1.25 * _SwayAmount * sin(_Time.y*_SwaySpeed + (unity_ObjectToWorld[0].w + unity_ObjectToWorld[2].w)*_WorldOffset);
			offset *= v.vertex.y + _HeightOffset * (unity_ObjectToWorld[1].x + unity_ObjectToWorld[1].y + unity_ObjectToWorld[1].z);
			offset *= 1+abs(clamp(unity_ObjectToWorld[2].w, -_ClampZ, _ClampZ));
			
			v.vertex.x += offset;
			
            v.vertex = UnityFlipSprite(v.vertex, _Flip);
        	// o.worldPos = mul(unity_ObjectToWorld, v.vertex);

            #if defined(PIXELSNAP_ON)
            v.vertex = UnityPixelSnap (v.vertex);
            #endif

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color * _Color * _RendererColor;
		}
		
		void surf(Input IN, inout SurfaceOutput o)
		{
            fixed4 c = SampleSpriteTexture (IN.uv_MainTex) * IN.color;
            o.Albedo = hueshift(IN.worldPos, c.rgb);
            o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Sprites/Diffuse"
}
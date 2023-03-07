Shader "Custom/RainbowGrassDefault"
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
    	
		_SwaySpeed ("SwaySpeed", Float) = 1
		_SwayAmount ("Sway Amount", Float) = 1
		_WorldOffset ("World Offset", Float) = 1
		_HeightOffset ("Height Offset", Float) = 0
		_ClampZ ("Clamp Z Position", Float) = 1
		[PerRendererData] _PushAmount ("Push Amount (Player)", Float) = 0 
    	
		_Phase ("Phase", Float) = 0
        _Frequency ("Frequency", Vector) = (0,0,0,0) 
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

            #pragma vertex SpriteVert2
            #pragma fragment RainbowFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
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
            
			struct fragData
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
            	float3 worldPos : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
            
			fragData SpriteVert2(appdata_t IN)
			{
				fragData OUT;

				UNITY_SETUP_INSTANCE_ID (IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				float offset = _PushAmount + 1.25 * _SwayAmount * sin(_Time.y*_SwaySpeed + (unity_ObjectToWorld[0].w + unity_ObjectToWorld[2].w)*_WorldOffset);
				offset *= IN.vertex.y + _HeightOffset * (unity_ObjectToWorld[1].x + unity_ObjectToWorld[1].y + unity_ObjectToWorld[1].z);
				offset *= 1+abs(clamp(unity_ObjectToWorld[2].w, -_ClampZ, _ClampZ));	
				IN.vertex.x += offset;
				
				OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
				OUT.vertex = UnityObjectToClipPos(OUT.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
				OUT.color = IN.color * _Color * _RendererColor;

				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}
            
			fixed4 RainbowFrag(fragData IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb = hueshift(IN.worldPos, c.rgb);
				return c;
			}
            
            ENDCG
        }
    }
}
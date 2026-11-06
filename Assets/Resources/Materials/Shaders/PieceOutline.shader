Shader "Custom/PieceOutline" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5

		_ValueThreshold("Value Threshold", Range(0,1)) = 0.05
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "TransparentCutout"
		}
		LOD 200

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

		CGPROGRAM

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade

		sampler2D _MainTex;

		float _ValueThreshold;


		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
            float3 worldPos;
		};

		fixed4 LightingUnlit(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
    		fixed4 c;
    		c.rgb = s.Albedo; 
    		c.a = s.Alpha;
    		return c;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			float2 uv = IN.uv_MainTex;
			fixed4 col = tex2D(_MainTex, uv);// * IN.color;
			
			fixed m = max(col.r, max(col.g, col.b));
			if (col.a > 0.1) {
				if (m > _ValueThreshold) {
					col = fixed4(0.5,0.5,0.5,col.a * 0.5);
				} else {
					col = fixed4(1,1,1,col.a);
				}
			}

			fixed4 c = col * IN.color;
			o.Albedo = c;
			o.Alpha =  c.a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
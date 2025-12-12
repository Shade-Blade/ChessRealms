Shader "Custom/SquareEffectShader" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_DeltaX("DeltaX", float) = 0
		_DeltaY("DeltaY", float) = 0
		_XVelocity("X Velocity", float) = 0
		_YVelocity("Y Velocity", float) = 0
		_BaseAlpha("Base Alpha Mult", float) = 0
	}
	SubShader {
		Tags
		{
			"Queue" = "Transparent-100"
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
		float _DeltaX;
		float _DeltaY;
		float _XVelocity;
		float _YVelocity;
		float _BaseAlpha;

		float4 _MainTex_TexelSize;


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


		/*
		struct SurfaceOutput {
			fixed3 Albedo;  // diffuse color
			fixed3 Normal;  // tangent space normal, if written
			fixed3 Emission;
			half Specular;  // specular power in 0..1 range
			fixed Gloss;    // specular intensity
			fixed Alpha;    // alpha for transparencies
		}
		*/

		void surf(Input IN, inout SurfaceOutput o) {
			float2 uv = IN.uv_MainTex;
			uv += float2(_DeltaX - (frac(100 * IN.worldPos.z)), _DeltaY + (frac(100 * IN.worldPos.z)));
			uv += float2(frac(_Time.x * (_XVelocity * (0.9 + 0.2 * frac(100 * IN.worldPos.z)))), frac(_Time.x * (_YVelocity * (0.9 + 0.2 * frac(100 * IN.worldPos.z)))));
			fixed4 col = tex2D(_MainTex, uv) * IN.color;

			col.rgb = fixed3(1,1,1)* IN.color.rgb;
			fixed4 newcol = col;
			newcol.a = col.a;

			fixed4 c = newcol * IN.color;
			o.Albedo = newcol;
			o.Alpha =  newcol.a;
			//clip(o.Alpha - 0.05);

			o.Alpha = max(o.Alpha,IN.color.a*_BaseAlpha);
		}

		ENDCG
	}

	FallBack "Diffuse"
}
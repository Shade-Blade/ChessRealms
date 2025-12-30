Shader "Custom/PieceModifier" {
	Properties{
		[PerRendererData]_MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.5
		_LightDesaturation("Light Desaturtion", float) = 0.5

		[HDR]
		_BlackColor("Black Color", Color) = (0,0,1,1)
		[HDR]
		_GrayColor("Gray Color", Color) = (0,0,1,1)
		[HDR]
		_WhiteColor("White Color", Color) = (0,0,1,1)
		_Midpoint("Midpoint", Range(0,1)) = 0.5
		_Leak("Leak", Range(0,1)) = 0.5

		_OverlayColor("Overlay Color", Color) = (0,0,0,0)
		_OverlayTex("Overlay Texture", 2D) = "white" {}
		_PerPixelScale("Overlay Pixel Scale", float) = 1
		_XOffset("X Offset", Range(0,1)) = 1	//cyclic so values outside 0,1 are equivalent to something in that range
		_YOffset("Y Offset", Range(0,1)) = 1	//cyclic so values outside 0,1 are equivalent to something in that range		
		_TimeScale("Time Scale", float) = 1
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

		float _LightDesaturation;

		half4 _BlackColor;
		half4 _GrayColor;
		half4 _WhiteColor;

		float _Leak;
		float _Midpoint;

		float4 _MainTex_TexelSize;

		sampler2D _OverlayTex;
		fixed4 _OverlayColor;
		float _PerPixelScale;

		float _TimeScale;
		float _XOffset;
		float _YOffset;


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

		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
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
			fixed4 col = tex2D(_MainTex, uv);// * IN.color;
			
			fixed4 b = fixed4(0,0,0,0);
			if (col.a != 0) {
				float timeFactor = _Time.x * _TimeScale;

				float2 buv = IN.uv_MainTex;
				buv.x /= _MainTex_TexelSize.x * _PerPixelScale;
				buv.y /= _MainTex_TexelSize.y * _PerPixelScale;

				buv.x = buv.x + _XOffset;
				buv.y = buv.y + _YOffset;
				//buv.y = buv.y - timeFactor;

				//need to add a correction factor because red is too dim compared to cyan
				float bc = max(1, 3 - _OverlayColor.x - _OverlayColor.y - _OverlayColor.z);

				float br = tex2D(_OverlayTex, buv);

				//Time factor is used to cycle things
				br += timeFactor;
				br = frac(br);

				b = (br * _OverlayColor * bc) * 0.8;
				b *= 0.8 + 0.2 * sin(timeFactor * 1.2);
			}
			o.Emission = b;

			fixed4 newcol = col;

			float lightness = IN.color.r + IN.color.g + IN.color.b;

			float factor = lerp(0, 1, saturate(lightness * 2 - 1));
			float darkfactor = 1 - factor;

			//new: saturated colors are lighter if dark
			float m = min(col.r, min(col.g, col.b));
			float saturation = max(col.r - m, max(col.g - m, col.b - m));

			//White pieces have pastel ish colors while Black pieces can have fully saturated colors
			//(This is probably the best way to have a bit of color on pieces?)
			newcol.rgb = lerp(newcol.rgb, float3(1,1,1), factor * _LightDesaturation * (1 - saturation * 0.75));
			newcol.a = col.a;

			newcol *= lerp(IN.color, float4(1,1,1,1), saturation * 0.75);


			fixed lerpCoeff = RGBToHSV(col.rgb).z;

			fixed4 gradColor;

			if (lerpCoeff > _Midpoint) {
				gradColor = lerp(_GrayColor,_WhiteColor,(lerpCoeff - _Midpoint)/(1 - _Midpoint));
			} else {
				gradColor = lerp(_BlackColor,_GrayColor,(lerpCoeff)/(_Midpoint));
			}

			fixed4 newcolB = lerp(gradColor, newcol, _Leak);
			newcolB.a = min(col.a, gradColor.a);

			fixed4 c = newcolB * IN.color;
			o.Albedo = newcolB;
			o.Alpha =  newcolB.a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
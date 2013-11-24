Shader "Custom/Diffuse with Detail Texture" {
	Properties {
		_Color ("Color & Transparency", Color) = (1,1,1,1)
		_Detail ("Detail", 2D) = "gray" {}
		_DetailTiling ("Detail Tiling", Float) = 5.0
	}
    SubShader {
      LOD 200
      Lighting Off
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float4 color : COLOR;
		  float3 viewDir;
		  float4 screenPos;
      };
	  float4 _Color;
	  sampler2D _Detail;
	  float _DetailTiling;
      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = _Color.rgb;
		  half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
		  float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
          screenUV *= float2(_DetailTiling, _DetailTiling * 2);
          o.Albedo *= tex2D (_Detail, screenUV).rgb;
      }
      ENDCG
    }
    Fallback "Diffuse"
  }
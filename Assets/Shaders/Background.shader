Shader "Custom/Background" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_bwBlend ("Black & White blend", Range (0, 1)) = 0.5
		_PulseSpeed ("Pulse Speed", float) = 150
		_PulseWidth ("Pulse Width", float) = 10
		_PulseDistance ("Pulse Distance", float) = 50
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
 
			#include "UnityCG.cginc"
 
			float _TimeSinceLevelLoad;

			float4 frag(v2f_img i) : COLOR {
				float2 fragScreenPos = uv * _ScreenParams.xy;
				float distToCenter = distance(uv, float2(0.5, 0.5));
				float distStepped = round(distToCenter * 5) / 5 + 0.2;

				return distStepped;
			}
			ENDCG
		}
	}
}
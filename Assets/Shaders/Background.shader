Shader "Custom/Background" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_GradientTex ("Gradient", 2D) = "white" {}
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
 
			#include "UnityCG.cginc"
 
			float _TimeSinceLevelLoad;
			sampler2D _GradientTex;

			float4 frag(v2f_img i) : COLOR {
				float PI = 3.14159;

				float2 fragScreenPos = i.uv * _ScreenParams.xy;
				
				float distToCenter = distance(sin(i.uv * 1000 + _TimeSinceLevelLoad), float2(0.5, 0.5));

				// Spiral effects
				float angle = atan2(i.uv.y - 0.5, i.uv.x - 0.5);
				angle = (angle + PI) / (2 * PI);

				//distToCenter = distToCenter + angle * 0.2;

				// Fishbowl effects
				//distToCenter = distToCenter - sin(i.uv.x) * 0.2;
				distToCenter = distance(i.uv, float2(
					0.5 + cos(_TimeSinceLevelLoad * 0.5) * 0.02 * (1 - distToCenter), 
					0.5 + sin(_TimeSinceLevelLoad * 0.5) * 0.02 * (1 - distToCenter)));
				distToCenter = pow(distToCenter, 1);

				float distStepped = round(distToCenter * 5) / 5;

				//float4 light = float4(50/255., 51/255., 83/255., 1);
				//float4 dark = float4(46/255., 34/255., 47/255., 1);

				float4 c = tex2D(_GradientTex, float2(1 - distStepped - 0.2, 0.5));

				return c;
				//return lerp(light, dark, 1 - distStepped);
			}
			ENDCG
		}
	}
}
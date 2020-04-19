﻿Shader "Custom/Pulse" {
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
 
			uniform sampler2D _MainTex;
			uniform float _bwBlend;
			float _TimeSinceLevelLoad;
			float4 _Pulse;
			float _PulseSpeed;
			float _PulseWidth;
			float _PulseDistance;

			float4 frag(v2f_img i) : COLOR {
				float2 pulseUV = _Pulse.xy;
				float pulseStartTime = _Pulse.z;
				float2 fragScreenPos = i.uv * _ScreenParams.xy;
				float2 pulseScreenPos = pulseUV * _ScreenParams.xy;
				float distToPulse = distance(pulseScreenPos, fragScreenPos);
				float pulseTime = _TimeSinceLevelLoad - pulseStartTime;
				float pulseRadius = pulseTime * _PulseSpeed;
				float distToPulseRadius = abs(distToPulse - pulseRadius);

				bool inPulse = distToPulseRadius < _PulseWidth && distToPulse < _PulseDistance;

				float pulseAmount = cos(distToPulseRadius / _PulseWidth * 3) * 0.05;

				pulseAmount *= inPulse;

				float4 c = tex2D(_MainTex, i.uv * (1 - pulseAmount));
				
				return c * (1 - pulseAmount * 2);
			}
			ENDCG
		}
	}
}
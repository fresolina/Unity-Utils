Shader "Lotec/FlameURP" {
    Properties {
        [HDR] _BaseColor ("Flame Color", Color) = (1,0.5,0,1)
        _VolumeTex ("Volume Texture (3D)", 3D) = "" {}
        _MarchSteps ("Marching Steps", Range(10,128)) = 50
        _VolumeIntensity ("Volume Intensity", float) = 1
        _VolumeScale ("Volume Scale", float) = 1
        _NoiseSpeed ("Noise Speed", float) = 1
        _NoiseIntensity ("Noise Intensity", float) = 1
    }

    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "SRPBatcher"="true" }
        LOD 200

        // Pass for volumetric flame effect on a box mesh (object space bounds assumed to be [-0.5, 0.5])
        Pass {
            Name "VolumetricFlame"
            Tags { "LightMode"="UniversalForward" "Queue"="Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex volVert
            #pragma fragment volFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Vertex structure: We only need position.
            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            // Pass object space position to the fragment shader.
            struct v2f {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                half2 uv          : TEXCOORD1;
            };

            // Uniforms
            TEXTURE3D(_VolumeTex); SAMPLER(sampler_VolumeTex);
            float4 _BaseColor;
            int _MarchSteps;
            float _VolumeIntensity;
            float _VolumeScale;
            float _NoiseSpeed;
            float _NoiseIntensity;

            v2f volVert(appdata v) {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.vertex);
                o.positionOS = v.vertex.xyz;
                o.uv = v.uv;
                return o;
            }

            // Helper to get the camera position in world space.
            float3 GetCameraPosWS() {
                return _WorldSpaceCameraPos;
            }

            // Define a constant maximum iteration count.
            #define MAX_MARCH_STEPS 128

            // Helper function: a simple 2D noise using a sine/hash trick.
            float perlinNoise2D(float2 uv) {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 volFrag(v2f i) : SV_Target {
                float3 startPos = i.positionOS;
                float3 camPosOS = TransformWorldToObject(GetCameraPosWS());
                float3 rayDirOS = normalize(startPos - camPosOS);
                float3 samplePos = startPos;

                // Initialize the accumulated density.
                float densityAccum = 0.0;

                // Compute a world-up vector (0,1,0) in object space.
                float3 upOS = normalize(TransformWorldToObject(float4(0,1,0,0)).xyz);

                // Compute our effective step size based on _MarchSteps.
                float stepSize = 1.0 / (float)_MarchSteps;

                // March along the ray.
                for (int s = 0; s < MAX_MARCH_STEPS; s++) {
                    if (s >= _MarchSteps)
                        break;
                    
                    float nX = perlinNoise2D(i.uv + float2(_Time.y, _Time.y));
                    float nY = perlinNoise2D(i.uv + float2(_Time.y + 10.0, _Time.y + 10.0));
                    float3 noiseOffset = float3(nX, nY, 0) * _NoiseSpeed;
                    noiseOffset += upOS * _NoiseSpeed * 0.5;
                    
                    float3 posOffset = samplePos + noiseOffset;
                    float3 sampleUV = (posOffset * _VolumeScale) + 0.5;
                    
                    if (any(sampleUV < 0) || any(sampleUV > 1))
                        break;
                    
                    float sampleDensity = SAMPLE_TEXTURE3D(_VolumeTex, sampler_VolumeTex, sampleUV).r;
                    sampleDensity *= _NoiseIntensity;

                    // Falloff
                    // float verticalGrad = smoothstep(0.5, 0.2, samplePos.y + 0.5);
                    // float horizontalFall = saturate(1.0 - length(samplePos.xz) * 2.0);
                    // sampleDensity *= verticalGrad * horizontalFall;
                    
                    densityAccum += sampleDensity * stepSize;
                    samplePos += rayDirOS * stepSize;
                }
                
                float intensity = saturate(densityAccum * _VolumeIntensity);
                float3 flameColor = lerp(float3(0,0,0), _BaseColor.rgb, intensity);
                return half4(flameColor, intensity);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

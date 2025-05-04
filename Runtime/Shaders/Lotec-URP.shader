// TODO: Performance-testa metallic map, andra maps. Stöd andra maps?
// * Color Tint override toggle
// * Separat map för roughness. tvinga ingen att använda albedo.
// Kanske:
// * Support realtime Main light.
// * Support additional light specular highlight.
// * Support mixed lights.
// * Runtime reflection probes, uppdaterar när ett dynamiskt objekt i närheten rört sig tillräckligt mycket.
//      - Lägg dem nära väggar som ska lysas upp med dynamiskt ljus? Det dynamiska ljuset måste då även ha ett HDR-material matchande ljuset med rätt intensity och färg (kanske går scripta?)
Shader "Lotec/URP" {
    Properties {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Color Tint", Color) = (1,1,1,1)

        _BumpMap ("Normal Map", 2D) = "bump" {}

        _RoughnessMap ("Roughness (A)", 2D) = "white" {}
        [Toggle] _RoughnessOverride("Roughness Override", Float) = 0
        _Roughness ("Roughness", Range(0,1)) = 1.0

        _MetallicGlossMap("Metallic (R)", 2D) = "white" {}
        [Toggle] _MetallicOverride("Metallic Override (base reflectance)", Float) = 1
        _BaseReflectance ("Metallic (base reflectance)", Range(0,1)) = 0.04

        // Toggle stuff on/off.
        [Toggle] _EnvironmentReflections("Specular Environment Reflections", Float) = 1
        [Toggle] _RealtimeLighting("Realtime Lighting", Float) = 1
    }

    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "SRPBatcher"="true" }

        // Main forward pass for the dominant (directional/main) light.
        Pass {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _NORMALMAP
            #pragma multi_compile_instancing
            #pragma multi_compile _ LIGHTMAP_ON
            // #pragma multi_compile _ _FORWARD_PLUS
            // TODO: Maybe support forward+ in the future.
            // See: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/use-built-in-shader-methods-additional-lights-fplus.html

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Support additional lights (point lights and spot lights).
            #pragma multi_compile _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            // GI and reflections (GlossyEnvironmentReflection)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
            // Support Box projection reflection probes.
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION

            // Light Probes (SampleSH)
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"

            // Specular ambient lighting
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_ON
            #pragma shader_feature_local_fragment _REALTIMELIGHTING_ON
            #pragma shader_feature_local_fragment _ROUGHNESSOVERRIDE_ON
            #pragma shader_feature_local_fragment _METALLICOVERRIDE_ON

            struct appdata {
                float4 position : POSITION;
                float2 uv       : TEXCOORD0;
                float2 uv2      : TEXCOORD1;
                #ifdef _NORMALMAP
                    float4 tangent : TANGENT;
                    float3 normal  : NORMAL;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 positionCS  : SV_POSITION;
                half2 uv           : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                #ifdef _NORMALMAP
                    half3 normal  : TEXCOORD2;
                    half3 tangent : TEXCOORD3;
                    half3 binormal: TEXCOORD4;
                #endif
                half2 lightmapUV   : TEXCOORD5;
                half2 positionSS   : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
                TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
                TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
                TEXTURE2D(_RoughnessMap); SAMPLER(sampler_RoughnessMap);
                float4 _BaseMap_ST;
                float4 _BaseColor;
                half _Roughness;
                half _BaseReflectance;
            CBUFFER_END

            CBUFFER_START(UnityPerCamera)
                TEXTURE2D(urp_ReflProbes_Atlas); SAMPLER(sampler_urp_ReflProbes_Atlas);
            CBUFFER_END

            v2f vert (appdata vertexData) {
                v2f output = (v2f)0;
                UNITY_SETUP_INSTANCE_ID(vertexData);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                // UNITY_TRANSFER_INSTANCE_ID(vertexData, output);
                // UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexData);

                output.positionWS = TransformObjectToWorld(vertexData.position.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                // Apply tiling and offset using _BaseMap_ST
                output.uv = TRANSFORM_TEX(vertexData.uv, _BaseMap);
                // Compute lightmap UV using transformation with unity_Lightmap_ST.
                output.lightmapUV = vertexData.uv2 * unity_LightmapST.xy + unity_LightmapST.zw;
                #ifdef _NORMALMAP
                    output.normal = normalize(TransformObjectToWorldNormal(vertexData.normal));
                    output.tangent = normalize(TransformObjectToWorldDir(vertexData.tangent.xyz));
                    output.binormal = cross(output.normal, output.tangent) * vertexData.tangent.w;
                #endif
                // Compute the screen space position.
                output.positionSS = ComputeNormalizedDeviceCoordinates(output.positionCS);
                return output;
            }

            half4 frag (v2f input) : SV_Target {
                half3 cameraPosition = GetCameraPositionWS();
                half3 directionToCamera = normalize(cameraPosition - input.positionWS);
                // Sample albedo and extract glossiness from its alpha.
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                #ifdef _ROUGHNESSOVERRIDE_ON
                    half roughness = _Roughness;
                #else
                    half roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).a;
                #endif

                // --- Setup normal vector ---
                half3 normal;
                #ifdef _NORMALMAP
                    // Convert tangent space normal from normal map to world space.
                    half4 normSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
                    half3 normTex = UnpackNormal(normSample);
                    half3x3 TBN = half3x3(normalize(input.tangent),
                                          normalize(input.binormal),
                                          normalize(input.normal));
                    normal = normalize(mul(normTex, TBN));
                #else
                    // Use the vertex normal as the normal.
                    normal = normalize(TransformObjectToWorldNormal(float3(0,0,1)));
                #endif

                // --- Baked light, GI and shadows from Lightmap ---
                #ifdef LIGHTMAP_ON
                    // Diffuse ambient lighting from lightmap.
                    half3 ambientDiffuse = SampleLightmap(input.lightmapUV, normal);
                #else
                    // Diffuse ambient lighting from reflection probes.
                    half3 ambientEnvironmentDiffuse = GlossyEnvironmentReflection(normal, input.positionWS, 1, 1, input.positionSS);

                    // Sample light probes. TODO: Use 100% from environment if there is no light probe.
                    half3 ambientProbeDiffuse = SampleSH(normal);
                    half3 ambientDiffuse = ambientEnvironmentDiffuse + ambientProbeDiffuse * 0.5h;
                #endif
               
                // Additional lighting. Loop over the 4 additional lights.
                half3 realtimeLighting = half3(0, 0, 0);
                #if defined(_REALTIMELIGHTING_ON) && defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint i = 0; i < pixelLightCount; ++i) {
                        Light light = GetAdditionalLight(i, input.positionWS);
                        float NdotL = dot(normal, normalize(light.direction));
                        NdotL = (NdotL + 1) * 0.5;
                        realtimeLighting += saturate(NdotL) * light.color * light.distanceAttenuation * light.shadowAttenuation;

                        // // Light source specular highlight.
                        // // NOTE: specularity from main light is rarely useful. Better to use environment reflections.
                        // half3 halfVecDir = normalize(light.direction + directionToCamera);
                        // // If the half vector is parallel to the normal (1.0), then the specular is maximum.
                        // half NdotH = saturate(dot(normal, halfVecDir));
                        // half smoothness = 1 - roughness;
                        // half glossExponent = lerp(1, 1000, smoothness);
                        // half3 specular;
                        // if (smoothness > 0.99 && NdotH > 0.9998) {
                        //     specular = light.color;
                        // } else {
                        //     specular = pow(NdotH, glossExponent) * smoothness * light.color;
                        // }
                        // realtimeLighting += specular;
                    }
                #endif

                // --- Specular ambient lighting - reflections
                half3 ambientReflection = half3(0, 0, 0);
                #ifdef _ENVIRONMENTREFLECTIONS_ON
                    if (roughness < 0.999) {
                        #ifdef _METALLICOVERRIDE_ON
                            half baseReflectance = _BaseReflectance;
                        #else
                            half baseReflectance = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, input.uv).r;
                        #endif
                        // --- Reflections from reflection probes ---
                        // Sample reflection (or sky) probe for reflections.
                        half3 ReflectionVector = reflect(-directionToCamera, normal);
                        half3 environmentReflection = GlossyEnvironmentReflection(ReflectionVector, input.positionWS, roughness, 1, input.positionSS);

                        // Compute Fresnel factor using Schlick's approximation:
                        // F = F₀ + (1 - F₀) * (1 - dot(N, V))⁵
                        half F0 = baseReflectance; // Normal values: 0.04 (non-metal) - 0.16 (metal).
                        half Fresnel = F0 + (1 - F0) * pow(1 - saturate(dot(normal, directionToCamera)), 5);

                        // Blend the environment reflection using the Fresnel factor.
                        ambientReflection = environmentReflection * Fresnel;
                    }
                #endif

                // Combine lighting.
                // half3 finalColor = albedo.rgb * (ambientDiffuse + ambientReflection);
                half3 finalColor = albedo.rgb * ambientDiffuse + ambientReflection;
                finalColor += realtimeLighting * albedo.rgb * roughness;
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

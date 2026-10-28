Shader "FartRafters/SculptLit"
{
    Properties
    {
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.32
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Occlusion ("Ambient Strength", Range(0, 2)) = 1
        [NoScaleOffset] _DetailMap ("Detail Albedo", 2D) = "grey" {}
        [NoScaleOffset][Normal] _DetailNormal ("Detail Normal", 2D) = "bump" {}
        _DetailScale ("Detail Scale", Float) = 3
        _DetailAlbedo ("Detail Albedo Strength", Range(0, 1)) = 0.35
        _DetailColorMix ("Detail Color Mix", Range(0, 1)) = 0
        _VertexColor ("Use Vertex Color", Range(0, 1)) = 1
        _DetailNormalStrength ("Detail Normal Strength", Range(0, 2)) = 0.6
        _RimColor ("Rim Color", Color) = (1, 0.97, 0.92, 1)
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.12
        _Translucency ("Translucency", Range(0, 1)) = 0
        [Toggle(_DETAIL)] _Detail ("Use Detail", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _Smoothness;
            half _Metallic;
            half _Occlusion;
            float _DetailScale;
            half _DetailAlbedo;
            half _DetailColorMix;
            half _VertexColor;
            half _DetailNormalStrength;
            half4 _RimColor;
            half _RimStrength;
            half _Translucency;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _DETAIL
            #pragma multi_compile_fragment _ FR_LOW_DETAIL
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_DetailMap);
            SAMPLER(sampler_DetailMap);
            TEXTURE2D(_DetailNormal);
            SAMPLER(sampler_DetailNormal);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                float3 normalOS : TEXCOORD3;
                half4 color : COLOR;
                half fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 normalWS = normalize(input.normalWS);
                half3 albedo = lerp(half3(1, 1, 1), input.color.rgb, _VertexColor) * _BaseColor.rgb;

            #if defined(_DETAIL) && !defined(FR_LOW_DETAIL)
                float3 n = normalize(input.normalOS);
                float3 weights = pow(abs(n), 4.0);
                weights /= max(dot(weights, 1.0), 0.0001);
                float3 p = input.positionOS * _DetailScale;
                float2 uvX = p.zy;
                float2 uvY = p.xz;
                float2 uvZ = p.xy;
                half3 detailColor = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uvX).rgb * weights.x
                                  + SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uvY).rgb * weights.y
                                  + SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, uvZ).rgb * weights.z;
                half detail = dot(detailColor, half3(0.3333, 0.3333, 0.3333));
                albedo = lerp(albedo * lerp(1.0, detail * 2.0, _DetailAlbedo), detailColor * _BaseColor.rgb, _DetailColorMix);

                half3 tX = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormal, sampler_DetailNormal, uvX), _DetailNormalStrength);
                half3 tY = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormal, sampler_DetailNormal, uvY), _DetailNormalStrength);
                half3 tZ = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormal, sampler_DetailNormal, uvZ), _DetailNormalStrength);
                tX = half3(tX.xy + n.zy, abs(tX.z) * n.x);
                tY = half3(tY.xy + n.xz, abs(tY.z) * n.y);
                tZ = half3(tZ.xy + n.xy, abs(tZ.z) * n.z);
                float3 detailNormalOS = normalize(tX.zyx * weights.x + tY.xzy * weights.y + tZ.xyz * weights.z);
                normalWS = normalize(TransformObjectToWorldNormal(detailNormalOS));
            #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(normalWS) * _Occlusion;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo;
                surface.metallic = _Metallic;
                surface.smoothness = _Smoothness;
                surface.occlusion = 1.0;
                surface.alpha = 1.0;
                surface.normalTS = half3(0, 0, 1);

                half4 color = UniversalFragmentPBR(inputData, surface);

                Light mainLight = GetMainLight(inputData.shadowCoord);
                half facing = 1.0 - saturate(dot(normalWS, inputData.viewDirectionWS));
                half rim = pow(facing, 3.0) * _RimStrength * (0.4 + 0.6 * mainLight.shadowAttenuation);
                color.rgb += rim * _RimColor.rgb * mainLight.color;
                half back = saturate(dot(-normalWS, mainLight.direction)) * _Translucency * mainLight.shadowAttenuation;
                color.rgb += albedo * back * mainLight.color * 0.6;

                color.rgb = MixFog(color.rgb, input.fogFactor);
                return half4(color.rgb, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirection = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirection = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirection));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            half4 frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half4 frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}

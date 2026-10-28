Shader "FartRafters/Ground"
{
    Properties
    {
        [NoScaleOffset] _GrassMap ("Grass", 2D) = "white" {}
        [NoScaleOffset][Normal] _GrassNormal ("Grass Normal", 2D) = "bump" {}
        [NoScaleOffset] _DirtMap ("Dirt", 2D) = "white" {}
        [NoScaleOffset][Normal] _DirtNormal ("Dirt Normal", 2D) = "bump" {}
        [NoScaleOffset] _MudMap ("Mud", 2D) = "white" {}
        [NoScaleOffset][Normal] _MudNormal ("Mud Normal", 2D) = "bump" {}
        [NoScaleOffset] _PebbleMap ("Pebbles", 2D) = "white" {}
        [NoScaleOffset][Normal] _PebbleNormal ("Pebble Normal", 2D) = "bump" {}
        [NoScaleOffset] _MacroMap ("Macro Noise", 2D) = "grey" {}
        _GrassTint ("Grass Tint", Color) = (0.86, 0.95, 0.72, 1)
        _DirtTint ("Dirt Tint", Color) = (1, 0.95, 0.88, 1)
        _MudTint ("Mud Tint", Color) = (0.95, 0.9, 0.84, 1)
        _PebbleTint ("Pebble Tint", Color) = (1, 0.97, 0.92, 1)
        _GrassScale ("Grass Scale", Float) = 1
        _DirtScale ("Dirt Scale", Float) = 1.2
        _MudScale ("Mud Scale", Float) = 1
        _PebbleScale ("Pebble Scale", Float) = 1.6
        _MacroScale ("Macro Scale", Float) = 0.035
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1
        _WetHeight ("Wet Height", Float) = 0.35
        _DirtNoise ("Dirt Noise", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _GrassTint;
            half4 _DirtTint;
            half4 _MudTint;
            half4 _PebbleTint;
            float _GrassScale;
            float _DirtScale;
            float _MudScale;
            float _PebbleScale;
            float _MacroScale;
            half _NormalStrength;
            float _WetHeight;
            half _DirtNoise;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ FR_LOW_DETAIL

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_GrassMap); SAMPLER(sampler_GrassMap);
            TEXTURE2D(_GrassNormal);
            TEXTURE2D(_DirtMap);
            TEXTURE2D(_DirtNormal);
            TEXTURE2D(_MudMap);
            TEXTURE2D(_MudNormal);
            TEXTURE2D(_PebbleMap);
            TEXTURE2D(_PebbleNormal);
            TEXTURE2D(_MacroMap); SAMPLER(sampler_MacroMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = normals.normalWS;
                output.tangentWS = float4(normals.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                half macro = SAMPLE_TEXTURE2D(_MacroMap, sampler_MacroMap, input.positionWS.xz * _MacroScale).r;
            #if defined(FR_LOW_DETAIL)
                half macro2 = macro;
            #else
                half macro2 = SAMPLE_TEXTURE2D(_MacroMap, sampler_MacroMap, input.positionWS.xz * _MacroScale * 4.3 + 0.37).r;
            #endif

                float2 grassUV = uv * _GrassScale;
                float2 grassUV2 = uv * _GrassScale * 0.37 + float2(0.21, 0.67);
            #if defined(FR_LOW_DETAIL)
                half3 grass = SAMPLE_TEXTURE2D(_GrassMap, sampler_GrassMap, grassUV).rgb * _GrassTint.rgb;
            #else
                half3 grass = lerp(SAMPLE_TEXTURE2D(_GrassMap, sampler_GrassMap, grassUV).rgb, SAMPLE_TEXTURE2D(_GrassMap, sampler_GrassMap, grassUV2).rgb, 0.4) * _GrassTint.rgb;
            #endif
                grass *= lerp(0.78, 1.18, macro);
                half3 dirt = SAMPLE_TEXTURE2D(_DirtMap, sampler_GrassMap, uv * _DirtScale).rgb * _DirtTint.rgb;
                half3 mud = SAMPLE_TEXTURE2D(_MudMap, sampler_GrassMap, uv * _MudScale).rgb * _MudTint.rgb;
                half3 pebble = SAMPLE_TEXTURE2D(_PebbleMap, sampler_GrassMap, uv * _PebbleScale).rgb * _PebbleTint.rgb;

                half dirtWeight = saturate(input.color.r + (macro2 - 0.55) * _DirtNoise * 2.0);
                half mudWeight = saturate(input.color.g);
                half pebbleWeight = saturate(input.color.b);
                half3 albedo = lerp(grass, dirt, smoothstep(0.35, 0.65, dirtWeight));
                albedo = lerp(albedo, mud, smoothstep(0.3, 0.7, mudWeight));
                albedo = lerp(albedo, pebble, smoothstep(0.3, 0.7, pebbleWeight));

            #if defined(FR_LOW_DETAIL)
                half3 normalTS = half3(0, 0, 1);
            #else
                half3 nGrass = UnpackNormalScale(SAMPLE_TEXTURE2D(_GrassNormal, sampler_GrassMap, grassUV), _NormalStrength);
                half3 nDirt = UnpackNormalScale(SAMPLE_TEXTURE2D(_DirtNormal, sampler_GrassMap, uv * _DirtScale), _NormalStrength);
                half3 nMud = UnpackNormalScale(SAMPLE_TEXTURE2D(_MudNormal, sampler_GrassMap, uv * _MudScale), _NormalStrength);
                half3 nPebble = UnpackNormalScale(SAMPLE_TEXTURE2D(_PebbleNormal, sampler_GrassMap, uv * _PebbleScale), _NormalStrength);
                half3 normalTS = lerp(nGrass, nDirt, smoothstep(0.35, 0.65, dirtWeight));
                normalTS = lerp(normalTS, nMud, smoothstep(0.3, 0.7, mudWeight));
                normalTS = normalize(lerp(normalTS, nPebble, smoothstep(0.3, 0.7, pebbleWeight)));
            #endif

                float wet = saturate((_WetHeight - input.positionWS.y) / 0.6);
                albedo *= lerp(1.0, 0.55, wet);
                half smoothness = lerp(0.08, 0.62, wet) + mudWeight * 0.08;

                float3 bitangent = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
                float3 normalWS = normalize(TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent, input.normalWS)));

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo;
                surface.smoothness = smoothness;
                surface.occlusion = lerp(0.85, 1.0, macro);
                surface.alpha = 1.0;
                surface.normalTS = normalTS;

                half4 color = UniversalFragmentPBR(inputData, surface);
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
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

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half4 frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}

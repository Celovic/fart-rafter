Shader "FartRafters/ToonCharacter"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _ShadeColor ("Shade Tint", Color) = (0.62, 0.55, 0.7, 1)
        _Wrap ("Wrap", Range(0, 1)) = 0.35
        _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.45
        _RampSmooth ("Ramp Smooth", Range(0.001, 0.5)) = 0.18
        _Gloss ("Gloss", Range(2, 256)) = 40
        _Specular ("Specular", Range(0, 1)) = 0.18
        _RimColor ("Rim Color", Color) = (1, 0.96, 0.9, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3.2
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.35
        _Ambient ("Ambient", Range(0, 2)) = 0.85
        _DetailScale ("Detail Scale", Float) = 38
        _DetailStrength ("Detail Strength", Range(0, 0.5)) = 0.08
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _ShadeColor;
            half _Wrap;
            half _RampThreshold;
            half _RampSmooth;
            float _Gloss;
            half _Specular;
            half4 _RimColor;
            half _RimPower;
            half _RimStrength;
            half _Ambient;
            float _DetailScale;
            half _DetailStrength;
        CBUFFER_END

        float Hash3(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }

        float ValueNoise(float3 x)
        {
            float3 i = floor(x);
            float3 f = frac(x);
            f = f * f * (3.0 - 2.0 * f);
            return lerp(lerp(lerp(Hash3(i), Hash3(i + float3(1, 0, 0)), f.x),
                             lerp(Hash3(i + float3(0, 1, 0)), Hash3(i + float3(1, 1, 0)), f.x), f.y),
                        lerp(lerp(Hash3(i + float3(0, 0, 1)), Hash3(i + float3(1, 0, 1)), f.x),
                             lerp(Hash3(i + float3(0, 1, 1)), Hash3(i + float3(1, 1, 1)), f.x), f.y), f.z);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                half4 color : COLOR;
                half fogFactor : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half3 albedo = input.color.rgb * _BaseColor.rgb;
                float detail = ValueNoise(input.positionOS * _DetailScale) * 0.65 + ValueNoise(input.positionOS * _DetailScale * 2.7) * 0.35;
                albedo *= 1.0 + (detail - 0.5) * 2.0 * _DetailStrength;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float ndl = dot(normalWS, mainLight.direction);
                float wrapped = saturate((ndl + _Wrap) / (1.0 + _Wrap));
                float lit = wrapped * lerp(1.0, mainLight.shadowAttenuation, 0.85);
                float ramp = smoothstep(_RampThreshold - _RampSmooth, _RampThreshold + _RampSmooth, lit);

                half3 shade = albedo * _ShadeColor.rgb;
                half3 color = lerp(shade, albedo, ramp) * mainLight.color;
                color += SampleSH(normalWS) * albedo * _Ambient;

                float3 halfDir = normalize(mainLight.direction + viewWS);
                float spec = pow(saturate(dot(normalWS, halfDir)), _Gloss) * _Specular * ramp;
                color += spec * mainLight.color;

                float rim = pow(1.0 - saturate(dot(normalWS, viewWS)), _RimPower) * _RimStrength * (0.35 + 0.65 * ramp);
                color += rim * _RimColor.rgb;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes input) : SV_POSITION
            {
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

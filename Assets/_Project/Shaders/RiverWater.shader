Shader "FartRafters/RiverWater"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.32, 0.55, 0.5, 1)
        _DeepColor ("Deep Color", Color) = (0.05, 0.2, 0.24, 1)
        _ScatterColor ("Scatter Color", Color) = (0.2, 0.55, 0.45, 1)
        _FoamColor ("Foam Color", Color) = (0.95, 0.97, 0.96, 1)
        [NoScaleOffset][Normal] _WaveNormal ("Wave Normal", 2D) = "bump" {}
        [NoScaleOffset] _FoamTex ("Foam Noise", 2D) = "grey" {}
        _NormalScale ("Normal Scale", Float) = 0.16
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.7
        _FlowSpeed ("Flow Speed", Float) = 0.6
        _DepthDistance ("Depth Fade", Float) = 1.6
        _ClarityDistance ("Clarity", Float) = 0.7
        _ShoreFoam ("Shore Foam", Range(0, 1)) = 0.6
        _FoamDistance ("Foam Distance", Float) = 0.45
        _Smoothness ("Smoothness", Range(0, 1)) = 0.94
        _ReflectionStrength ("Reflection", Range(0, 1.5)) = 1
        _SunStrength ("Sun Highlight", Range(0, 4)) = 1.6
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-50" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _ShallowColor;
            half4 _DeepColor;
            half4 _ScatterColor;
            half4 _FoamColor;
            float _NormalScale;
            half _NormalStrength;
            float _FlowSpeed;
            float _DepthDistance;
            float _ClarityDistance;
            half _ShoreFoam;
            float _FoamDistance;
            half _Smoothness;
            half _ReflectionStrength;
            half _SunStrength;
        CBUFFER_END

        float4 _FR_Wave;
        float _FR_WaterTime;

        float3 DisplaceWater(float3 positionWS, out float3 normalWS, out float height)
        {
            float amplitude = _FR_Wave.x;
            float k = 6.2831853 / max(_FR_Wave.y, 0.1);
            float t = _FR_WaterTime * _FR_Wave.z;
            float a = (positionWS.x * 0.7 + positionWS.z) * k + t;
            float b = (positionWS.x - positionWS.z * 0.6) * k * 1.7 + t * 1.3;
            height = amplitude * (sin(a) + 0.5 * sin(b));
            float dx = amplitude * (cos(a) * 0.7 * k + 0.5 * cos(b) * 1.7 * k);
            float dz = amplitude * (cos(a) * k - 0.5 * cos(b) * 0.6 * 1.7 * k);
            normalWS = normalize(float3(-dx, 1.0, -dz));
            positionWS.y += _FR_Wave.w + height;
            return positionWS;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_WaveNormal);
            SAMPLER(sampler_WaveNormal);
            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float height : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS;
                float height;
                positionWS = DisplaceWater(positionWS, normalWS, height);
                output.positionWS = positionWS;
                output.normalWS = normalWS;
                output.height = height;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                output.color = input.color;
                return output;
            }

            half3 SampleWaves(float2 uv)
            {
                return UnpackNormalScale(SAMPLE_TEXTURE2D(_WaveNormal, sampler_WaveNormal, uv), _NormalStrength);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 flow = input.color.gb * 2.0 - 1.0;
                if (dot(flow, flow) < 0.0001)
                {
                    flow = float2(0.0, 0.3);
                }
                float t = _FR_WaterTime * _FlowSpeed;
                float2 p = input.positionWS.xz * _NormalScale;
                float2 side = float2(-flow.y, flow.x);

                half3 n1 = SampleWaves(p + flow * t * 0.35);
                half3 n2 = SampleWaves(p * 1.9 + flow * t * 0.6 + side * 0.13 + 0.37);
                half3 n3 = SampleWaves(p * 0.42 - side * t * 0.05 + flow * t * 0.12 + 0.71);
                half3 detail = normalize(half3(n1.xy + n2.xy * 0.7 + n3.xy * 0.9, n1.z * n2.z * n3.z));
                float3 baseNormal = normalize(input.normalWS);
                float3 normalWS = normalize(float3(baseNormal.x + detail.x, baseNormal.y, baseNormal.z + detail.y));

                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float bedDepth = input.color.a * 3.0;
                float waterDepth = bedDepth / max(0.3, viewWS.y);
                float shore = saturate(input.color.r);
                float depthFactor = 1.0 - exp(-waterDepth / max(_DepthDistance, 0.01));
                float clarity = 1.0 - exp(-waterDepth / max(_ClarityDistance, 0.01));

                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half shadow = lerp(0.55, 1.0, light.shadowAttenuation);

                half3 body = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);
                half3 ambient = SampleSH(float3(0, 1, 0));
                body *= ambient * 0.55 + light.color * shadow * (0.35 + 0.25 * saturate(light.direction.y));
                half scatter = saturate(input.height * 4.0 + 0.25) * saturate(dot(viewWS, -light.direction) * 0.5 + 0.6);
                body += _ScatterColor.rgb * scatter * 0.12 * light.color;

                float ndv = saturate(dot(normalWS, viewWS));
                half fresnel = 0.02 + 0.98 * pow(1.0 - ndv, 5.0);
                float3 reflectDir = reflect(-viewWS, normalWS);
                reflectDir.y = abs(reflectDir.y);
                half perceptualRoughness = 1.0 - _Smoothness;
                half3 reflection = GlossyEnvironmentReflection(reflectDir, perceptualRoughness, 1.0) * _ReflectionStrength;

                float3 halfDir = normalize(light.direction + viewWS);
                half specPower = exp2(10.0 * _Smoothness + 1.0);
                half sun = pow(saturate(dot(normalWS, halfDir)), specPower) * _SunStrength * light.shadowAttenuation;

                half3 color = lerp(body, reflection, fresnel) + sun * light.color;

                half foamNoise = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, input.positionWS.xz * 0.35 + flow * t * 0.3).r;
                half foamNoise2 = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, input.positionWS.xz * 0.9 - flow * t * 0.2 + 0.5).r;
                float edge = 1.0 - saturate(bedDepth / max(_FoamDistance, 0.01));
                float foamMask = saturate(edge * 1.4 + shore * _ShoreFoam * 0.6 - 0.25);
                float foam = smoothstep(0.45, 0.75, foamMask * (foamNoise * 0.65 + foamNoise2 * 0.55));
                color = lerp(color, _FoamColor.rgb * (ambient * 0.5 + light.color * shadow * 0.6), foam * 0.85);

                half alpha = saturate(lerp(0.25, 1.0, clarity) + fresnel * 0.5 + foam);
                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}

Shader "FartRafters/Clouds"
{
    Properties
    {
        [NoScaleOffset] _NoiseTex ("Noise", 2D) = "grey" {}
        _Color ("Light", Color) = (1, 1, 1, 1)
        _Shade ("Shade", Color) = (0.74, 0.82, 0.95, 1)
        _Scale ("Scale", Float) = 0.004
        _Speed ("Speed", Float) = 0.004
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-60" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _Shade;
                float _Scale;
                float _Speed;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Speed;
                float2 p = input.positionWS.xz * _Scale;
                half a = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, p + float2(t, t * 0.6)).r;
                half b = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, p * 2.7 - float2(t * 1.3, 0.37)).r;
                half c = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, p * 7.1 + float2(0.71, t * 2.0)).r;
                half n = a * 0.55 + b * 0.32 + c * 0.13;
                half3 color = lerp(_Shade.rgb, _Color.rgb, smoothstep(0.38, 0.72, n));
                half alpha = saturate(smoothstep(0.3, 0.6, n) * 0.85 + 0.15);
                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}

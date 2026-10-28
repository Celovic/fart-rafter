Shader "FartRafters/Whirlpool"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.02, 0.12, 0.2, 1)
        _MidColor ("Mid Color", Color) = (0.08, 0.36, 0.45, 1)
        _FoamColor ("Foam Color", Color) = (0.92, 0.97, 0.98, 1)
        [NoScaleOffset] _FoamTex ("Foam Noise", 2D) = "grey" {}
        _Twist ("Twist", Float) = 7
        _Speed ("Speed", Float) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-40" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _MidColor;
                half4 _FoamColor;
                float _Twist;
                float _Speed;
            CBUFFER_END

            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fogFactor : TEXCOORD1;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0 - 1.0;
                float r = length(p);
                float angle = atan2(p.y, p.x);
                float t = _Time.y * _Speed;
                float spiral = angle / 6.2831853 + r * _Twist * 0.15 + t;
                half streak = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, float2(spiral * 2.0, r * 1.5 - t * 0.4)).r;
                half streak2 = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, float2(spiral * 5.0 + 0.3, r * 3.0 - t * 0.9)).r;
                half bands = saturate(sin(spiral * 6.2831853 * 3.0) * 0.5 + 0.5);
                half foam = smoothstep(0.5, 0.85, streak * 0.6 + streak2 * 0.4 + bands * 0.25) * smoothstep(0.05, 0.35, r);
                half3 body = lerp(_DeepColor.rgb, _MidColor.rgb, smoothstep(0.0, 0.9, r));
                half3 color = lerp(body, _FoamColor.rgb, foam * 0.8);
                half alpha = input.color.a * lerp(0.75, 1.0, foam);
                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}

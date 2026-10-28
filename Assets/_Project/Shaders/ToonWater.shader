Shader "FartRafters/ToonWater"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.32, 0.84, 0.88, 1)
        _DeepColor ("Deep Color", Color) = (0.1, 0.44, 0.72, 1)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _RippleColor ("Ripple Color", Color) = (0.8, 0.95, 1, 1)
        _RippleScale ("Ripple Scale", Float) = 0.35
        _RippleSpeed ("Ripple Speed", Float) = 0.5
        _RippleWidth ("Ripple Width", Range(0.01, 0.3)) = 0.07
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.35
        _ShoreFoam ("Shore Foam", Range(0, 1)) = 0.75
        _CrestHeight ("Crest Height", Float) = 0.13
        _Gloss ("Gloss", Range(4, 256)) = 90
        _SpecularStrength ("Specular", Range(0, 2)) = 0.7
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _ShallowColor;
            half4 _DeepColor;
            half4 _FoamColor;
            half4 _RippleColor;
            float _RippleScale;
            float _RippleSpeed;
            float _RippleWidth;
            half _RippleStrength;
            half _ShoreFoam;
            float _CrestHeight;
            float _Gloss;
            half _SpecularStrength;
            half _ShadowStrength;
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
            positionWS.y = _FR_Wave.w + height;
            return positionWS;
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

            float Hash(float2 p)
            {
                float3 q = frac(float3(p.xyx) * 0.1031);
                q += dot(q, q.yzx + 33.33);
                return frac((q.x + q.y) * q.z);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1.0, 0.0));
                float c = Hash(i + float2(0.0, 1.0));
                float d = Hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

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

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = normalize(GetCameraPositionWS() - input.positionWS);
                float t = _FR_WaterTime * _RippleSpeed;
                float2 p = input.positionWS.xz * _RippleScale;

                float n1 = ValueNoise(p + float2(t, t * 0.6));
                float n2 = ValueNoise(p * 1.3 - float2(t * 0.7, -t * 0.4) + 17.3);
                float ripple = 1.0 - smoothstep(0.0, _RippleWidth, abs(n1 - n2));

                float shore = saturate(input.color.r);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewWS)), 3.0);
                half3 color = lerp(_DeepColor.rgb, _ShallowColor.rgb, saturate(shore * 0.85 + fresnel * 0.35));

                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = saturate(dot(normalWS, light.direction)) * 0.35 + 0.65;
                half shadow = lerp(1.0 - _ShadowStrength, 1.0, light.shadowAttenuation);
                color *= light.color * diffuse * shadow;

                float3 halfDir = normalize(light.direction + viewWS);
                float specular = pow(saturate(dot(normalWS, halfDir)), _Gloss);
                specular = smoothstep(0.45, 0.55, specular) * _SpecularStrength * shadow;

                color += _RippleColor.rgb * ripple * _RippleStrength * (1.0 - shore * 0.5);
                color += specular;

                float foamNoise = ValueNoise(input.positionWS.xz * 1.6 + float2(t * 0.8, -t * 0.5));
                float shoreFoam = smoothstep(0.5, 0.56, shore * _ShoreFoam + (foamNoise - 0.5) * 0.45 + 0.08);
                float crest = smoothstep(_CrestHeight, _CrestHeight + 0.04, input.height + (foamNoise - 0.5) * 0.1);
                color = lerp(color, _FoamColor.rgb * light.color, saturate(max(shoreFoam, crest * 0.8)));

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
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

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 normalWS;
                float height;
                float3 positionWS = DisplaceWater(TransformObjectToWorld(input.positionOS.xyz), normalWS, height);
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half frag(Varyings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }

    FallBack Off
}

Shader "Hidden/UriEscapist/VHSFilter"
{
    Properties
    {
        _Intensity("Intensity", Range(0, 1)) = 0.35
        _ScanlineStrength("Scanline Strength", Range(0, 1)) = 0.12
        _NoiseStrength("Noise Strength", Range(0, 0.2)) = 0.025
        _JitterStrength("Jitter Strength", Range(0, 0.05)) = 0.008
        _ChromaticAberration("Chromatic Aberration", Range(0, 0.01)) = 0.0015
        _TrackingBandStrength("Tracking Band Strength", Range(0, 0.25)) = 0.06
        _TrackingBandSpeed("Tracking Band Speed", Range(0.1, 5)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "VHSFilterPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _Intensity;
            float _ScanlineStrength;
            float _NoiseStrength;
            float _JitterStrength;
            float _ChromaticAberration;
            float _TrackingBandStrength;
            float _TrackingBandSpeed;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float TrackingBand(float2 uv, float timeValue)
            {
                float center = frac(timeValue * 0.12 * _TrackingBandSpeed);
                return 1.0 - smoothstep(0.0, 0.18, abs(uv.y - center));
            }

            float4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float timeValue = _Time.y;

                float scanlinePhase = sin((uv.y * _ScreenParams.y * 1.35) + timeValue * 2.2);
                float scanlineMask = 1.0 - ((scanlinePhase * 0.5 + 0.5) * _ScanlineStrength * _Intensity);

                float lineNoise = Hash21(float2(floor(uv.y * _ScreenParams.y * 0.65), floor(timeValue * 28.0)));
                float trackingBand = TrackingBand(uv, timeValue);
                float horizontalJitter = (lineNoise - 0.5) * _JitterStrength * _Intensity;
                horizontalJitter += sin((uv.y + timeValue * 0.85) * _ScreenParams.y * 0.075) * trackingBand * _TrackingBandStrength * 0.045 * _Intensity;

                float2 shiftedUv = float2(saturate(uv.x + horizontalJitter), uv.y);
                float chromaOffset = _ChromaticAberration * _Intensity * (1.0 + trackingBand * _TrackingBandStrength * 6.0);

                half3 shiftedColor;
                shiftedColor.r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, saturate(shiftedUv + float2(chromaOffset, 0.0)), _BlitMipLevel).r;
                shiftedColor.g = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, shiftedUv, _BlitMipLevel).g;
                shiftedColor.b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, saturate(shiftedUv - float2(chromaOffset, 0.0)), _BlitMipLevel).b;

                float grain = Noise(uv * float2(_ScreenParams.x * 0.55, _ScreenParams.y * 0.55) + timeValue * float2(51.0, 17.0));
                float grainValue = (grain - 0.5) * _NoiseStrength * _Intensity * 2.0;

                half3 vhsColor = shiftedColor * scanlineMask;
                vhsColor += grainValue.xxx;
                vhsColor += trackingBand * _TrackingBandStrength * _Intensity * half3(0.035, 0.018, 0.012);

                half4 originalColor = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                half3 finalColor = lerp(originalColor.rgb, saturate(vhsColor), saturate(_Intensity));
                return half4(finalColor, originalColor.a);
            }
            ENDHLSL
        }
    }
}


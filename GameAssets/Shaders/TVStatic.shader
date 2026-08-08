Shader "BeanShootout/TVStatic2"
{
    Properties
    {
        _Speed ("Speed", Vector, 2) = (1, 1, 0, 0)
        _Scale ("Scale", Float) = 500
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half2 _Speed;
                half _Scale;
            CBUFFER_END
            
            float Unity_SimpleNoise_ValueNoise_Deterministic_float (float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0; Hash_Tchou_2_1_float(c0, r0);
                float r1; Hash_Tchou_2_1_float(c1, r1);
                float r2; Hash_Tchou_2_1_float(c2, r2);
                float r3; Hash_Tchou_2_1_float(c3, r3);
                float bottomOfGrid = lerp(r0, r1, f.x);
                float topOfGrid = lerp(r2, r3, f.x);
                float t = lerp(bottomOfGrid, topOfGrid, f.y);
                return t;
            }
            
            void Unity_SimpleNoise_Deterministic_float(float2 UV, float Scale, out float Out)
            {
                float freq, amp;
                Out = 0.0f;
                freq = pow(2.0, float(0));
                amp = pow(0.5, float(3-0));
                Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            }
            
            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half2 time = _Time * _Speed;
                
                half2 outUv;
                
                Unity_TilingAndOffset_float(IN.uv, time, half2(0, 0), outUv);
                
                IN.uv = outUv;
                
                half outNoise;
                
                Unity_SimpleNoise_Deterministic_float(outUv, _Scale, outNoise);
                
                // half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return half4(outNoise.xxx, 1);
            }
            ENDHLSL
        }
    }
}

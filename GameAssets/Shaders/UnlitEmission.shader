Shader "BeanShootout/UnlitEmission"
{
    Properties
    {
        [HDR] _EmissionColor("Emission Color", Color) = (1, 1, 1, 1)
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

            struct Attributes
            {
                float4 positionOS : POSITION;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _EmissionColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                ZERO_INITIALIZE(Varyings, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _EmissionColor;
            }
            ENDHLSL
        }
    }
}

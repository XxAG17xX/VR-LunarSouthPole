Shader "Custom/EarthDayNight"
{
    Properties
    {
        _DayTex ("Day Texture", 2D) = "white" {}
        _NightTex ("Night Texture", 2D) = "black" {}
        _CloudTex ("Cloud Texture", 2D) = "white" {}
        _CloudOpacity ("Cloud Opacity", Range(0,1)) = 0.6
        _TerminatorSharpness ("Terminator Sharpness", Range(1, 20)) = 8
        _SunDirection ("Sun Direction", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Required for Quest Single Pass Instanced
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_DayTex);   SAMPLER(sampler_DayTex);
            TEXTURE2D(_NightTex); SAMPLER(sampler_NightTex);
            TEXTURE2D(_CloudTex); SAMPLER(sampler_CloudTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _DayTex_ST;
                float  _CloudOpacity;
                float  _TerminatorSharpness;
                float4 _SunDirection;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _DayTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 sunDir = normalize(_SunDirection.xyz);
                float  NdotL  = dot(normalize(IN.normalWS), sunDir);
                float dayFactor = saturate(NdotL * _TerminatorSharpness + 0.5);

                half4 dayColor   = SAMPLE_TEXTURE2D(_DayTex,   sampler_DayTex,   IN.uv);
                half4 nightColor = SAMPLE_TEXTURE2D(_NightTex, sampler_NightTex, IN.uv);
                half4 cloudColor = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, IN.uv);

                half4 earthColor = lerp(nightColor, dayColor, dayFactor);
                half cloudMask = cloudColor.r * _CloudOpacity * dayFactor;
                earthColor.rgb = lerp(earthColor.rgb, half3(1,1,1), cloudMask);

                return earthColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}

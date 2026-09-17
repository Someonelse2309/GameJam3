Shader "Shader/RoofCutout"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        [PowerSlider(0.1)] _Radius ("Cutout Radius", Float) = 2.0
        [PowerSlider(0.1)] _Feather ("Feather Edge", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            float4 _PlayerPos;
            float _Radius;
            float _Feather;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.worldPos = mul(unity_ObjectToWorld, input.positionOS).xy;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = tex * _Color;

                float dist = distance(input.worldPos, _PlayerPos.xy);
                float alpha = smoothstep(_Radius, _Radius + _Feather, dist);
                color.a *= alpha;

                // DEBUG: kalau alpha = 0 everywhere, tampilkan warna merah untuk test
                // Hapus line di bawah ini setelah verify
                // if (alpha < 0.01) return half4(1, 0, 0, 1);

                return color;
            }
            ENDHLSL
        }
    }
}

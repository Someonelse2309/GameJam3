Shader "Custom/OldFilmOverlay"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}

        [Header(Sepia)]
        _SepiaStrength ("Sepia Strength", Range(0,1)) = 0.7
        _SepiaTint ("Sepia Tint", Color) = (0.85, 0.75, 0.6, 1)

        [Header(Grain)]
        _GrainAmount ("Grain Amount", Range(0,1)) = 0.12
        _GrainSize ("Grain Size", Range(1,80)) = 40
        _GrainSpeed ("Grain Speed", Range(0,5)) = 1.5

        [Header(Scratches)]
        _ScratchAmount ("Scratch Amount", Range(0,1)) = 0.15
        _ScratchSpeed ("Scratch Speed", Range(0,5)) = 2
        _ScratchWidth ("Scratch Width", Range(0.001,0.01)) = 0.003

        [Header(Dust)]
        _DustAmount ("Dust Amount", Range(0,1)) = 0.1
        _DustSize ("Dust Size", Range(1,50)) = 25
        _DustSpeed ("Dust Speed", Range(0,3)) = 0.8

        [Header(Vignette)]
        _VignetteStrength ("Vignette Strength", Range(0,1)) = 0.4
        _VignetteColor ("Vignette Color", Color) = (0, 0, 0, 1)

        [Header(Flicker)]
        _FlickerAmount ("Flicker Amount", Range(0,0.2)) = 0.05
        _FlickerSpeed ("Flicker Speed", Range(0,20)) = 10

        [Header(Edge)]
        _EdgeDarkness ("Edge Darkness", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Sepia
            float _SepiaStrength;
            float4 _SepiaTint;

            // Grain
            float _GrainAmount;
            float _GrainSize;
            float _GrainSpeed;

            // Scratches
            float _ScratchAmount;
            float _ScratchSpeed;
            float _ScratchWidth;

            // Dust
            float _DustAmount;
            float _DustSize;
            float _DustSpeed;

            // Vignette
            float _VignetteStrength;
            float4 _VignetteColor;

            // Flicker
            float _FlickerAmount;
            float _FlickerSpeed;

            // Edge
            float _EdgeDarkness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Hash function
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // Noise
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Film grain (animated)
            float filmGrain(float2 uv)
            {
                float t = _Time.y * _GrainSpeed;
                float2 grainUV = floor(uv * _GrainSize) * float2(37.0, 17.0);
                grainUV += float2(t, t * 0.7);
                return hash(grainUV);
            }

            // Animated scratches
            float scratches(float2 uv)
            {
                float scratch = 0.0;
                float t = _Time.y * _ScratchSpeed;

                // Multiple scratch lines
                for (int i = 0; i < 5; i++)
                {
                    float fi = float(i);
                    float xPos = hash(float2(fi * 13.7, floor(t * 0.3 + fi)));
                    float width = _ScratchWidth * hash(float2(fi * 7.3, 0.0));
                    float alpha = step(0.92, hash(float2(fi * 23.1, floor(t * 0.5 + fi * 2.0))));

                    float lineX = step(abs(uv.x - xPos), width);
                    scratch += lineX * alpha;
                }

                return scratch;
            }

            // Dust spots (animated)
            float dustSpots(float2 uv)
            {
                float t = _Time.y * _DustSpeed;
                float2 dustUV = floor(uv * _DustSize) * float2(23.0, 31.0);
                dustUV += float2(t * 0.3, t * 0.2);

                float d = hash(dustUV);
                return step(0.96, d) * _DustAmount * 3.0;
            }

            // Small dust particles
            float smallDust(float2 uv)
            {
                float t = _Time.y * _DustSpeed * 1.5;
                float2 dustUV = uv * _DustSize * 2.0;
                dustUV += float2(t, t * 0.8);
                dustUV = floor(dustUV);

                float d = hash(dustUV);
                return step(0.985, d) * _DustAmount * 2.0;
            }

            // Vignette
            float vignette(float2 uv)
            {
                float2 centered = (uv - 0.5) * 2.0;
                float dist = length(centered);
                return smoothstep(0.3, 0.9, dist);
            }

            // Edge darkness
            float edgeDarkness(float2 uv)
            {
                float2 e = abs(uv - 0.5) * 2.0;
                float edge = max(e.x, e.y);
                edge = smoothstep(0.7, 1.0, edge);
                return edge * _EdgeDarkness;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                half4 col = tex2D(_MainTex, uv);
                float3 color = col.rgb;

                // ================================
                // SEPIA EFFECT
                // ================================
                float3 sepia;
                sepia.r = dot(color, float3(0.393, 0.769, 0.189));
                sepia.g = dot(color, float3(0.349, 0.686, 0.168));
                sepia.b = dot(color, float3(0.272, 0.534, 0.131));

                // Mix sepia with tint
                float3 sepiaColor = lerp(sepia, sepia * _SepiaTint.rgb, 0.3);
                color = lerp(color, sepiaColor, _SepiaStrength);

                // ================================
                // FILM GRAIN
                // ================================
                float grain = filmGrain(uv);
                grain = (grain - 0.5) * 2.0;
                color += grain * _GrainAmount;

                // ================================
                // SCRATCHES
                // ================================
                float scratch = scratches(uv);
                color -= scratch * _ScratchAmount;

                // ================================
                // DUST SPOTS
                // ================================
                float dust = dustSpots(uv) + smallDust(uv);
                color -= dust * 0.5;

                // ================================
                // FLICKER
                // ================================
                float flicker = noise(float2(_Time.y * _FlickerSpeed, 0.5));
                flicker = (flicker - 0.5) * 2.0;
                color += flicker * _FlickerAmount;

                // ================================
                // VIGNETTE
                // ================================
                float vig = vignette(uv);
                color = lerp(color * (1.0 - _VignetteStrength), color, vig);

                // Edge darkness
                float edge = edgeDarkness(uv);
                color *= 1.0 - edge;

                // ================================
                // FINAL
                // ================================
                color = saturate(color);

                return half4(color, col.a);
            }
            ENDCG
        }
    }
}

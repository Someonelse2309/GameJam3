Shader "Custom/BrownFilmVignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Overlay)]
        _BrownColor ("Brown Color", Color) = (0.55, 0.35, 0.15, 1)
        _BrownStrength ("Brown Strength", Range(0, 1)) = 0.35

        [Header(Spots)]
        _SpotAmount ("Spot Amount", Range(0, 1)) = 0.25
        _SpotSize ("Spot Size", Range(1, 20)) = 8
        _SpotSpeed ("Spot Speed", Range(0, 2)) = 0.3

        [Header(Vignette)]
        _VigStrength ("Vignette Strength", Range(0, 1)) = 0.6
        _VigSize ("Vignette Size", Range(0.5, 2)) = 1.0
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

            float4 _BrownColor;
            float _BrownStrength;

            float _SpotAmount;
            float _SpotSize;
            float _SpotSpeed;

            float _VigStrength;
            float _VigSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Hash
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Animated dark spots (bokeh-like)
            float darkSpots(float2 uv)
            {
                float time = _Time.y * _SpotSpeed;

                // Multiple layers of spots
                float spots = 0.0;

                for (int i = 0; i < 3; i++)
                {
                    float fi = float(i);
                    float t = time + fi * 100.0;

                    // Moving UV
                    float2 spotUV = uv * (_SpotSize + fi * 2.0);
                    spotUV += float2(sin(t * 0.5) * 0.5, cos(t * 0.3) * 0.5);

                    // Grid
                    float2 grid = floor(spotUV);
                    float2 fracUV = frac(spotUV) - 0.5;

                    float h = hash(grid + fi * 10.0);

                    // Random size
                    float size = h * 0.4 + 0.1;

                    // Circle
                    float circle = 1.0 - smoothstep(size, size + 0.1, length(fracUV));

                    // Random opacity
                    float opacity = hash(grid * 2.0 + fi);

                    spots += circle * step(0.7, h) * opacity * _SpotAmount;
                }

                return spots;
            }

            // Animated vignette
            float vignette(float2 uv)
            {
                float2 center = uv - 0.5;
                center *= _VigSize;

                float dist = length(center);
                float vig = smoothstep(0.2, 0.8, dist);

                // Add some variation
                float time = _Time.y * 0.2;
                float variation = sin(uv.x * 10.0 + time) * sin(uv.y * 10.0 + time * 0.7) * 0.05;

                return vig + variation;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                half4 col = tex2D(_MainTex, uv);

                // Brown overlay
                float3 color = col.rgb;
                color = lerp(color, _BrownColor.rgb, _BrownStrength);

                // Dark spots
                float spots = darkSpots(uv);
                color *= 1.0 - spots;

                // Vignette
                float vig = vignette(uv);
                color *= 1.0 - vig * _VigStrength;

                // Clamp
                color = saturate(color);

                return half4(color, col.a);
            }
            ENDCG
        }
    }
}

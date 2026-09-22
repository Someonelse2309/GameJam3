Shader "Custom/OldFilmEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SepiaIntensity ("Sepia Intensity", Range(0, 1)) = 0.8
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.3
        _GrainSize ("Grain Size", Range(1, 10)) = 3.0
        _GrainSpeed ("Grain Speed", Range(0, 5)) = 1.0
        _ScratchIntensity ("Scratch Intensity", Range(0, 1)) = 0.2
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _PanSpeed ("Pan Speed", Vector) = (0.01, 0.005, 0, 0)
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _SepiaIntensity;
            float _GrainIntensity;
            float _GrainSize;
            float _GrainSpeed;
            float _ScratchIntensity;
            float _FlickerIntensity;
            float _ScanlineIntensity;
            float2 _PanSpeed;

            // Hash function
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.13);
                p3 += dot(p3, p3.yzx + 3.333);
                return frac((p3.x + p3.y) * p3.z);
            }

            // Noise
            float noise(float2 x)
            {
                float2 i = floor(x);
                float2 f = frac(x);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Sepia
            float3 toSepia(float3 color)
            {
                float3 sepia;
                sepia.r = dot(color, float3(0.393, 0.769, 0.189));
                sepia.g = dot(color, float3(0.349, 0.686, 0.168));
                sepia.b = dot(color, float3(0.272, 0.534, 0.131));
                return sepia;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Panning UV untuk efek bergerak
                float2 panUV = i.uv + float2(_Time.y * _PanSpeed.x, _Time.y * _PanSpeed.y);

                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply sepia
                float3 sepiaColor = toSepia(col.rgb);
                col.rgb = lerp(col.rgb, sepiaColor, _SepiaIntensity);

                // Moving film grain (noise yang bergerak)
                float grain = noise(panUV * _GrainSize * 100.0) * 2.0 - 1.0;
                col.rgb += grain * _GrainIntensity;

                // Moving scratches
                float scratch = noise(float2(i.uv.x * 200.0 + _Time.y * 0.5, floor(_Time.y * 3.0)));
                if (scratch > 0.98)
                {
                    col.rgb += _ScratchIntensity;
                }

                // Horizontal lines (vhs style)
                float scanline = sin(i.uv.y * 300.0 + _Time.y * 2.0) * 0.5 + 0.5;
                col.rgb -= scanline * _ScanlineIntensity;

                // Flicker
                float flicker = 1.0 - (noise(float2(_Time.y * 5.0, 0.0)) * _FlickerIntensity);
                col.rgb *= flicker;

                // Vignette
                float2 vigUV = i.uv * (1.0 - i.uv.xy);
                float vig = vigUV.x * vigUV.y * 15.0;
                vig = pow(vig, 0.4);
                col.a *= vig * 0.6;

                return col;
            }
            ENDCG
        }
    }
}

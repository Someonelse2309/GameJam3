Shader "Custom/Vignette"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 0.5
        _Color ("Color", Color) = (0, 0, 0, 1)
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

            float _Intensity;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * (1 - i.uv.xy);
                float vig = uv.x * uv.y * 15.0;
                vig = pow(vig, _Intensity * 2);
                vig = clamp(vig, 0, 1);
                return fixed4(_Color.rgb, (1 - vig) * _Color.a);
            }
            ENDCG
        }
    }
}

Shader "Unlit/SoundSinusWave"
{
    Properties
    {
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

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


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float _BandCount;
            float _FrequencyBand[512];
            
            real4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv * 2 - 1;

                float time = _Time.y * 1.0;

                float3 color = 0;

                for (float i = 0.0; i < _BandCount + 1.0; i++)
                {
                    float freq = _FrequencyBand[i] * _BandCount/20;

                    float2 p = float2(uv);

                    p.x += i * 0.04 + freq * 0.03;
                    p.y += sin(p.x * 10.0 + time) * cos(p.x * 2.0) * freq * 0.2 * ((i + 1.0) / _BandCount);
                    float intensity = abs(0.02 / p.y) * clamp(freq, 0.35, 2.0);
                    color += float3(1.0 * intensity * (i / 5.0), 0.5 * intensity, 1.75 * intensity) * (3.0 / _BandCount);
                }
                color = clamp(color,0,1);
                return float4(color, color.r + color.g + color.b);
            }
            ENDHLSL
        }
    }
}
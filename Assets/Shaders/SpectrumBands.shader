Shader "Unlit/SpectrumBands"
{
    Properties
    {
        _SegmentCount("Segment Count",Range(4,128)) = 10
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
            float _SegmentCount;

            real4 frag(v2f IN) : SV_Target
            {
                float2 uv = float2(IN.uv.x * _BandCount, IN.uv.y * _SegmentCount);

                float c = _FrequencyBand[floor(uv.x)] * 0.5;
                float3 color = float3( IN.uv.x, 0.75 -.5 * IN.uv.x, 0.4 - .4 * IN.uv.x) * step(frac(IN.uv.y), c);
                color *= step(0.2, frac(uv.y)) * step(frac(uv.x), 0.8) * step(0.2, frac(uv.x));

                return float4(color, color.r + color.g + color.b);
            }
            ENDHLSL
        }
    }
}
Shader "Custom/SdfLayers"
{
    Properties
    {
        [NoScaleOffset] _SDF ("Texture", 3D) = "white" {}
        [NoScaleOffset] _Colors ("Colors", 2D) = "white" {}
        _Cut ("Cut", float) = 0
        _ColorsLength ("Colors Length", int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler3D _SDF;
            sampler2D _Colors;
            float _Cut;
            int _ColorsLength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex;
                return o;
            }
            
            const float smoothing = 1.0/64.0;
            
            float sdfstep(float distance){
                return saturate(smoothstep(.5 -smoothing , .5 + smoothing, distance));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 testOffset = float3(0, 0, _Cut);
                fixed4 col = tex3D(_SDF, i.uv + float3(.5,.5,.5) + testOffset);
                
                // Alpha test
                int edges = _ColorsLength - 1;
                float colorBand = round(col.r * edges) / edges;
                float3 color = tex2D(_Colors, float2(colorBand, 0));
                return float4(color, 1);
            }
            ENDCG
        }
    }
}

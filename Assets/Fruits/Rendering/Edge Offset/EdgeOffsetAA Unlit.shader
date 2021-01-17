Shader "Custom/EdgeOffsetAA/Unlit"
{
    Properties
    {
        [NoScaleOffset] _MainTexture ("Main Texture", 3D) = "white" {}
        [NoScaleOffset] _Normals ("Normals", 3D) = "black" {}
        _SampleOffset ("Sample Offset", float) = 0.005
        _Uv("UV", Vector) = (1, 1, 1, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler3D _MainTexture;
            sampler3D _Normals;
            float _SampleOffset;
            float3 _Uv;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 uv = (i.uv / _Uv + float3(.5, .5, .5));
                
                // Get normal pointing to the edge
                float3 normal = (tex3D(_Normals, uv) - float3(.5, .5, .5)) * 2;
                // Calculate uv offset in direction of normal
                float3 uvOffset = -normalize(normal) * _SampleOffset;
                
                // Debug mark edge
                //if(length(normal) < _SampleOffset){
                //    return float4(1, 0, 0, 1);
                //}
                
                float3 color = tex3D(_MainTexture, uv + uvOffset);
                return float4(color.rgb, 1);
            }
            ENDCG
        }
    }
}

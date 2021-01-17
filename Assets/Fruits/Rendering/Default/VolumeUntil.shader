Shader "Custom/VolumeUntil"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
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

            sampler3D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex3D(_MainTex, float3(i.uv.x, i.uv.z, -i.uv.y) + float3(.5,.5,.5));
            }
            ENDCG
        }
    }
}

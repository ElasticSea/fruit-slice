Shader "Custom/EdgeOffsetAA/Standard Surface"
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
        
        CGPROGRAM
        
        #pragma surface surf Lambert vertex:vert
        
        struct Input {
            float3 eUv;
        };
        
        sampler3D _MainTexture;
        sampler3D _Normals;
        float _SampleOffset;
        float3 _Uv;
        
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.eUv = v.vertex;
        }
        
        void surf (Input i, inout SurfaceOutput o) {
            float3 uv = (i.eUv / _Uv + float3(.5, .5, .5));
            
            // Get normal pointing to the edge
            float3 normal = (tex3D(_Normals, uv) - float3(.5, .5, .5)) * 2;
            // Calculate uv offset in direction of normal
            float3 uvOffset = -normalize(normal) * _SampleOffset;
            
            // Debug mark edge
            //if(length(normal) < _SampleOffset){
            //    o.Albedo = float4(1, 0, 0, 1);
            //}
            
            o.Albedo = tex3D(_MainTexture, uv + uvOffset);
        }
        ENDCG
    } 
    Fallback "Diffuse"
}
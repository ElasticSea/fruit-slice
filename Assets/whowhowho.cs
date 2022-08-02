using BLINDED_AM_ME;
using Fruits;
using UnityEngine;

public class whowhowho : MonoBehaviour
{
    [SerializeField] private VolumeMesh[] volumes;
    [SerializeField] private bool cut = true;
    private GameObject main;
    private GameObject other;
    private VolumeMesh volume;

    void Update()
    {
        if (main)
        {
            Destroy(main.gameObject);
        }

        if (volume)
        {
            var go = new GameObject();
            go.AddComponent<MeshFilter>().mesh = volume.OutsideMesh;
            go.AddComponent<MeshRenderer>().material = volume.OutsideMaterial;
            
            var insideMat = new Material(Shader.Find("Custom/EdgeOffsetAA/Standard Surface"));
            insideMat.SetTexture("_MainTexture", volume.ColorTexture);
            insideMat.SetTexture("_Normals", volume.EdgeTexture);
            insideMat.SetFloat("_SampleOffset", 0.0075f);
            insideMat.SetVector("_Uv", new Vector4(1, 1, 1, 0));

            if (cut)
            {
                var instance = go;
                var cuts = MeshCut.Cut(instance.gameObject, transform.position, transform.forward, insideMat);
                main = cuts[0];
                other = cuts[1];

                Destroy(other);
            }
            else
            {
                main = go;
            }
        }
    }

    public void SetVolume(VolumeMesh volume)
    {
        this.volume = volume;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = nameof(VolumeMesh), menuName = "Create New "+nameof(VolumeMesh))]
public class VolumeMesh : ScriptableObject
{
    public GameObject BlendFile;
    public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one);
    public Texture3D ColorTexture;
    public Texture3D EdgeTexture;
    public Mesh OutsideMesh;
    public Material OutsideMaterial;
}
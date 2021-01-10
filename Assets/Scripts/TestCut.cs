using BLINDED_AM_ME;
using UnityEngine;

public class TestCut : MonoBehaviour
{
    [SerializeField] private GameObject fruit;
    [SerializeField] private Material volumetricMat;
    
    private void Start()
    {
        MeshCut.Cut(fruit, transform.position, transform.up, volumetricMat);
    }
}

using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME;
using Fruits;
using Packages.Core.Scripts.Extensions;
using UnityEngine;

public class AuSomething : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Material insideMat;
    [SerializeField] private Transform cutTransform;
    private Mesh originalMesh;

    // Start is called before the first frame update
    void Start()
    {
        originalMesh = meshFilter.mesh;
    }

    // Update is called once per frame
    void Update()
    {
        meshFilter.mesh = originalMesh;
        
        var objs = MeshCut.Cut(gameObject, cutTransform.position, cutTransform.up, insideMat);

        // var main = objs[0];
        var seconday = objs[1];

        // meshFilter.mesh = main.GetComponent<MeshFilter>().mesh;
        
        // Destroy(main.gameObject);
        Destroy(seconday.gameObject);

    }
}

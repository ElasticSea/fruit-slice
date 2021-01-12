using System.Collections.Generic;
using System.Linq;
using BLINDED_AM_ME;
using UnityEngine;
using UnityEngine.XR;

public class Cut : MonoBehaviour
{
    [SerializeField] private GameObject fruit;
    [SerializeField] private Material volumetricMat;

    private bool IsTriggerHeld()
    {
        var allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        var right = allDevices.FirstOrDefault(d => d.name.Contains("Right"));
        if (right.name != null)
        {
            right.TryGetFeatureValue(CommonUsages.triggerButton, out var triggered);
            if (triggered)
            {
                return true;
            }
        }

        return false;
    }
    
    private void Start()
    {
    }

    private bool triggerRegistered;
    private void Update()
    {
        if (IsTriggerHeld() && triggerRegistered == false)
        {
            CutFruits();
            triggerRegistered = true;
        }

        if (IsTriggerHeld() == false)
        {
            triggerRegistered = false;
        }
    }

    private void CutFruits()
    {
        var objs = MeshCut.Cut(fruit, transform.position, transform.up, volumetricMat);

        var main = objs[0];
        var seconday = objs[1];
        
        main.GetComponent<MeshCollider>().sharedMesh = main.GetComponent<MeshFilter>().mesh;
        main.GetComponent<Rigidbody>().isKinematic = true;
        
        seconday.AddComponent<Rigidbody>();
        var addComponent = seconday.AddComponent<MeshCollider>();
        addComponent.convex = true;
        addComponent.sharedMesh = seconday.GetComponent<MeshFilter>().mesh;
    }
}

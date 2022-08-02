using System;
using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME;
using Core.Extensions;
using Fruits;
using Packages.Core.Scripts.Extensions;
using UnityEngine;

public class PreviewCut : MonoBehaviour
{
    [SerializeField] private Fruit fruit;
    [SerializeField] private Material fruitInside;

    [Range(0, 1)]
    [SerializeField] private float delta;
    [SerializeField] private float scale = 1;
    [SerializeField] private Vector3 rotation = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        // fruit.Cut(transform.position, transform.up);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private GameObject fruitClone;
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (fruitClone)
            {
                Destroy(fruitClone);
            }

            fruitClone = Instantiate(fruit).gameObject;
            fruitClone.transform.localScale = Vector3.one * scale;
            fruitClone.transform.rotation = Quaternion.Euler(rotation);
            var from = new Vector3(-scale/2f, -scale/2f, scale/2f);
            var to = new Vector3(scale/2f, scale/2f, -scale/2f);
            var origin = from.Lerp(to, delta);
            var normal = to;
            
            var objs = MeshCut.Cut(fruitClone, origin, normal, fruitInside);

            fruitClone = objs[0];
            // main.AddComponent<Fruit>().inside = inside;

            Destroy(objs[1]);
            // var main = objs[0];
            // var mainVolume = main.GetComponent<MeshFilter>().mesh.Volume();
            // var secondaryVolume = seconday.GetComponent<MeshFilter>().mesh.Volume();
            //
            // if (main.GetComponent<Fruit>() == false)
            // {
            // }
            //
            // if (seconday.GetComponent<Fruit>() == false)
            // {
            //     seconday.AddComponent<Fruit>().inside = inside;
            // }
            
            
        }
    }
}

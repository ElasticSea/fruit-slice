using BLINDED_AM_ME;
using Packages.Core.Scripts.Extensions;
using UnityEngine;

namespace Fruits
{
    public class Fruit : MonoBehaviour
    {
        [SerializeField] public Material inside;

        public GameObject Cut(Vector3 point, Vector3 normal)
        {
            var objs = MeshCut.Cut(gameObject, point, normal, inside);

            var main = objs[0];
            var seconday = objs[1];

            var mainVolume = main.GetComponent<MeshFilter>().mesh.Volume();
            var secondaryVolume = seconday.GetComponent<MeshFilter>().mesh.Volume();

            if (main.GetComponent<Fruit>() == false)
            {
                main.AddComponent<Fruit>().inside = inside;
            }

            if (seconday.GetComponent<Fruit>() == false)
            {
                seconday.AddComponent<Fruit>().inside = inside;
            }

            return main;

            // if (mainVolume > secondaryVolume)
            // {
            //     Destroy(seconday.gameObject);
            //     return main;
            // }
            // else
            // {
            //     Destroy(main.gameObject);
            //     return seconday;
            // }

            // main.GetComponent<MeshCollider>().sharedMesh = main.GetComponent<MeshFilter>().mesh;
            // main.GetComponent<Rigidbody>().isKinematic = true;
            //
            // seconday.AddComponent<Rigidbody>();
            // var addComponent = seconday.AddComponent<MeshCollider>();
            // addComponent.convex = true;
            // addComponent.sharedMesh = seconday.GetComponent<MeshFilter>().mesh;
        }
    }
}
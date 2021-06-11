using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Extensions;
using Core.Util;
using Fruits.Sdf;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Fruits.Builder.EdgeTexture
{
    public class EdgeTextureBuilder : MonoBehaviour
    {
        [SerializeField] private string path = "Fruits/Builder/edgeTexture";
        [SerializeField] private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        [SerializeField] private Vector3 resolution = Vector3.one * 128;
        [SerializeField] private int raycastsPerPixel = 64;
        [SerializeField] private BitsPerChannel channelDepth = BitsPerChannel.Ch16;

        public void GenerateTexture()
        {
            foreach (var mf in GetComponentsInChildren<MeshFilter>())
            {
                SetupMeshes(mf);
            }
            
            var width = (int) resolution.x;
            var height = (int) resolution.y;
            var depth = (int) resolution.z;
            
            using (var raycaster = new RaycastEdgeNormals(transform, bounds, width, height, depth, raycastsPerPixel, channelDepth))
            {
                var sw = Stopwatch.StartNew();
                var tex = raycaster.Run();
                Debug.Log($"Raycast Edge Texture Builder took: {sw.ElapsedMilliseconds} ms");
                
                var file = new FileInfo(Path.Combine(Application.dataPath, path));
                Utils.EnsureDirectory(file.Directory.FullName);
                AssetDatabase.CreateAsset(tex, $"Assets/{path}.asset");
            }
        }

        private void SetupMeshes(MeshFilter meshFilter)
        {
            var originalMesh = Instantiate(meshFilter.sharedMesh);
            var invertedMesh = Instantiate(originalMesh);
            invertedMesh.SetTriangles(Utils.FlipIndices(invertedMesh.triangles), 0);
            
            meshFilter.gameObject.layer = LayerMask.NameToLayer("Inside");
            meshFilter.mesh = originalMesh;
            meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = originalMesh;
            
            var inner = new GameObject($"{meshFilter.name} inverted");
            inner.transform.SetParent(meshFilter.transform, false);
            var innerMf = inner.AddComponent<MeshFilter>();
            innerMf.gameObject.layer = LayerMask.NameToLayer("Inside");
            innerMf.mesh = invertedMesh;
            innerMf.gameObject.AddComponent<MeshCollider>().sharedMesh = invertedMesh;
        }
    }
}
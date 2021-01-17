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
        
        [SerializeField] private bool showPoints = false;
        
        [Header("Show Rays")]
        [SerializeField] private bool showRays = false;
        [SerializeField] private bool showEnds = false;
        [SerializeField] private int skipRaysCount = 0;
        [SerializeField] private int showRaysCount = 10;
        [SerializeField] private Texture3D edges;
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            if (showPoints)
            {
                for (var i = 0; i < resolution.x; i++)
                {
                    for (var j = 0; j < resolution.y; j++)
                    {
                        for (var k = 0; k < resolution.z; k++)
                        {
                            if (i * j * k == 0)
                            {
                                var proc = new Vector3(i, j, k).Divide(resolution - Vector3.one);
                                var position = bounds.min + (bounds.size.Multiply(proc));

                                Gizmos.DrawSphere(position, 0.005f);
                            }
                        }
                    }
                }
            }

            if (showRays && edges)
            {
                int width = edges.width;
                int height = edges.height;
                int depth = edges.depth;

                var nativeArray = edges.GetPixelData<byte>(0);
                var bytes = nativeArray.GetSubArray(0, Mathf.Min(showRaysCount * 6, nativeArray.Length)).ToArray();
                var step = (1 + Mathf.Max(skipRaysCount, 0)) * 6;
                for (var i = 0; i < bytes.Length; i += step)
                {
                    var f0 = (float) (BitConverter.ToUInt16(bytes, i + 0) / (double) ushort.MaxValue);
                    var f1 = (float) (BitConverter.ToUInt16(bytes, i + 2) / (double) ushort.MaxValue);
                    var f2 = (float) (BitConverter.ToUInt16(bytes, i + 4) / (double) ushort.MaxValue);

                    var vec = (new Vector3(f0, f1, f2) - new Vector3(0.5f, 0.5f, 0.5f)) * 2;
                    var pointIndex = i / 6;
                    var point = EdgeTextureUtils.IndexToPoint(pointIndex, bounds, transform.localToWorldMatrix, width, height, depth);
                    
                    Gizmos.color = new Color(f0, f1, f2);
                    Gizmos.DrawLine(point, point + vec);
                    if (showEnds)
                    {
                        Gizmos.DrawSphere(point + vec, .01f);
                    }
                }
            }
        }
    }
}
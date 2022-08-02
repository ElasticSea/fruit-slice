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
    public class EdgeTextureGizmo : MonoBehaviour
    {
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool showPoints = true;
        [SerializeField] private bool showRays = true;
        [SerializeField] private bool showEnds = true;
        [SerializeField] private int skipRaysCount = 0;
        [SerializeField] private int showRaysCount = 1000;
        [SerializeField] private float maxRayLength = 1;
        [SerializeField] private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        [SerializeField] private Texture3D edges;
        
        private void OnDrawGizmosSelected()
        {
            if (showGizmos == false)
                return;
            
            var resolution = new Vector3(edges.width, edges.height, edges.depth);
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

            if (showRays || edges)
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

                    Gizmos.color = Color.HSVToRGB(vec.magnitude / maxRayLength, 1, 1);
                    if (showRays)
                    {
                        Gizmos.DrawLine(point, point + vec);
                    }

                    if (showEnds)
                    {
                        Gizmos.DrawSphere(point + vec, .01f);
                    }
                }
            }
        }
    }
}
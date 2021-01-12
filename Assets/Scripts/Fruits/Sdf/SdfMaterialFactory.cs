using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Extensions;
using Core.Util;
using UnityEditor;
using UnityEngine;

namespace Fruits.Sdf
{
    public class SdfMaterialFactory : MonoBehaviour
    {
        [SerializeField] private Bounds bounds;
        [SerializeField] private Vector3 resolution;
        [SerializeField] private int raycastsPerPixel = 512;

        [Header("Source")]
        [SerializeField] private MeshFilter[] meshes;
        [SerializeField] private Color[] colors;
        [SerializeField] private Texture3D[] generatedTextures;

        public Texture3D[] GeneratedTextures
        {
            get => generatedTextures;
            set => generatedTextures = value;
        }
        
        public List<Texture3D> Generate()
        {
            var textures = new List<Texture3D>();
            foreach (var mesh in meshes)
            {
                var tx = SetupMeshes(mesh);
                tx.name = mesh.name;
                textures.Add(tx);

                foreach (var oldMfs in mesh.GetComponentsInChildren<MeshFilter>())
                {
                    oldMfs.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
            

            var outputDir = Path.Combine(Application.dataPath, "Generated");
            Utils.EnsureDirectory(outputDir);
      
            foreach (var tx in textures)
            {
                AssetDatabase.CreateAsset(tx, $"Assets/Generated/tx_{tx.name}.asset");
            }

            return textures;
        }
        
        public void Bake()
        {
            if (colors.Length != generatedTextures.Length + 1)
            {
                throw new ArgumentException("Incorrect number of colors compared to textures.");
            }
            
            var combinedTexture = CombineTextures(generatedTextures.ToArray());
            var colorsTexture = GenerateColors(colors.ToArray());

            var outputDir = Path.Combine(Application.dataPath, "Generated");
            Utils.EnsureDirectory(outputDir);
      
            AssetDatabase.CreateAsset(combinedTexture, "Assets/Generated/combined.asset");
            AssetDatabase.CreateAsset(colorsTexture, "Assets/Generated/colors.asset");
            
            var material = new Material(Shader.Find("SDF/Layers"));
            material.SetTexture("_SDF", combinedTexture);
            material.SetTexture("_Colors", colorsTexture);
            material.SetInt("_ColorsLength", colors.Length);
            AssetDatabase.CreateAsset(material, "Assets/Generated/mat.asset");
        }

        private Texture3D SetupMeshes(MeshFilter meshFilter)
        {
            meshFilter.gameObject.layer = LayerMask.NameToLayer("Inside");
            meshFilter.mesh = Instantiate(meshFilter.sharedMesh);
            meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh =  meshFilter.mesh;
            
            var inner = new GameObject($"{meshFilter.name} inverted");
            inner.transform.SetParent(meshFilter.transform, false);
            var innerMf = inner.AddComponent<MeshFilter>();
            innerMf.gameObject.layer = LayerMask.NameToLayer("Outside");
            var invertedMesh = Instantiate(meshFilter.mesh);
            invertedMesh.SetTriangles(Utils.FlipIndices(invertedMesh.triangles), 0);
            innerMf.mesh = invertedMesh;
            innerMf.gameObject.AddComponent<MeshCollider>().sharedMesh = invertedMesh;

            return Raycast();
        }
        
        private Texture3D Raycast()
        {
            var tex = new Texture3D((int) resolution.x, (int) resolution.y, (int) resolution.z, TextureFormat.RFloat, false);
            var bytes = new byte[tex.width * tex.height * tex.depth * 4];

            var pointsOnSphere = Utils.PointsOnSphere(raycastsPerPixel);
            var raycaster = new Raycaster(pointsOnSphere.Length);
            var voxelMagnitude = bounds.size.Divide(resolution).magnitude;
            var count = 0;
            for (var i = 0; i < resolution.x; i++)
            {
                for (var j = 0; j < resolution.y; j++)
                {
                    for (var k = 0; k < resolution.z; k++)
                    {
                        var proc = new Vector3(i, j, k).Divide(resolution - Vector3.one);
                        var position = transform.localPosition + bounds.min + (bounds.size.Multiply(proc));
                        var wolrdPos = transform.InverseTransformPoint(position);

                        var shortestRay = raycaster.Run(wolrdPos, pointsOnSphere);
                        var normalized = Mathf.Clamp(shortestRay / voxelMagnitude, -1, 1);

                        var floatBytes = BitConverter.GetBytes(normalized / 2 + 0.5f);
                        Array.Copy(floatBytes, 0, bytes, count * 4, floatBytes.Length);

                        count++;
                    }
                }
            }

            tex.SetPixelData(bytes, 0);

            raycaster.Dispose();
        
            return tex;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        
        private Texture3D CombineTextures(Texture3D[] textures)
        {
            var size = textures[0].width;
            
            foreach (var tex in textures)
            {
                if (tex.width != size || tex.height != size || tex.depth != size)
                {
                    throw new ArgumentException($"All textures have to be {size}x{size}x{size}");
                }
            }

            var oldBytes = new byte[textures.Length][];
            for (var i = 0; i < oldBytes.Length; i++)
            {
                oldBytes[i] = textures[i].GetPixelData<byte>(0).ToArray();
            }

            var count = 0;
            var bytes = new byte[size * size * size * 2];
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    for (var z = 0; z < size; z++)
                    {
                        var value = 0f;
                        for (var i = 0; i < oldBytes.Length; i++)
                        {
                            var oldFloatValue = BitConverter.ToSingle(oldBytes[i], count * 4);
                            value += oldFloatValue;
                        }

                        var normalized = value / oldBytes.Length;
                        var shortNormalized = (ushort) (normalized * ushort.MaxValue);
                        var valueBytes = BitConverter.GetBytes(shortNormalized);
                        Array.Copy(valueBytes, 0, bytes, count * 2, 2);

                        count++;
                    }
                }
            }
        
            var outputTex = new Texture3D(size, size, size, TextureFormat.R16, false);
            outputTex.SetPixelData(bytes, 0);
            outputTex.Apply();

            return outputTex;
        }

        private Texture2D GenerateColors(Color[] colors)
        {
            var tex = new Texture2D(colors.Length, 1, TextureFormat.RGB24, false);
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return tex;
        }
    }
}
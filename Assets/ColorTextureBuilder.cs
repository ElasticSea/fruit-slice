using System.Collections.Generic;
using System.IO;
using Core.Util;
using UnityEditor;
using UnityEngine;

public class ColorTextureBuilder : MonoBehaviour
    {
        [SerializeField] private string sliceTextureDirectory = "slices";
        [SerializeField] private string exportPath = "output";
        [SerializeField] private bool compress;

        public Texture3D Convert()
        {
            return Convert(sliceTextureDirectory, exportPath, compress);
        }

        public Texture3D Convert(string sliceTextureDirectory, string exportPath, bool compress)
        {
            var pngs = Directory.GetFiles(sliceTextureDirectory, "*.png");
            var textures = new Texture2D[pngs.Length];
            for (var i = 0; i < textures.Length; i++)
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                texture.LoadImage(File.ReadAllBytes(pngs[i]));
                textures[i] = texture;
            }

            return  Convert(textures, compress);
        }

        public Texture3D Convert(Texture2D[] slices, bool compress)
        {
            var firstSlice = slices[0];
            
            var width = firstSlice.width;
            var height = slices.Length;
            var depth = firstSlice.height;

            var volumeTexture = new Texture3D(width, height, depth, TextureFormat.RGB24, false);
            var volumeColors = new Color[width * height * depth];

            for (var y = 0; y < height; y++)
            {
                var colors = slices[y].GetPixels();
                for (var x = 0; x < width; x++)
                {
                    for (var z = 0; z < depth; z++)
                    {
                        var index = z + y * depth + x * width * depth;
                        var index2 = (depth - 1 - z) + (width - 1 - x) * depth;
                        volumeColors[index] = colors[index2];
                    }
                }
            }

            volumeTexture.SetPixels(volumeColors);
            volumeTexture.Apply(false);

            if (compress)
            {
                volumeTexture = CompressTexture3D(volumeTexture, TextureFormat.BC7);
            }

            return volumeTexture;
        }
        
        public void Generate()
        {
        }

        // https://forum.unity.com/threads/texture3d-compression-issue.966494/
        private static Texture3D CompressTexture3D(Texture3D original, TextureFormat textureFormat)
        {
            var dataTex = original.GetPixels();

            int rx = original.width;
            int ry = original.height;
            int rz = original.depth;

            var textures2D = new List<Texture2D>();

            for (int z = 0; z < rz; z++)
            {
                var newTex = new Texture2D(rx, ry, TextureFormat.RGBAHalf, false);
                var arrColors = new Color[rx * ry];
                for (int y = 0; y < ry; y++)
                {
                    for (int x = 0; x < rx; x++)
                    {
                        int index = z * ry * rx + y * rx + x;
                        arrColors[y * rx + x] = dataTex[index];
                    }
                }

                newTex.SetPixels(arrColors);
                newTex.filterMode = FilterMode.Bilinear;
                newTex.wrapMode = TextureWrapMode.Clamp;
                newTex.Apply();
                textures2D.Add(newTex);
            }

            for (int i = 0; i < textures2D.Count; i++)
            {
                EditorUtility.CompressTexture(textures2D[i], textureFormat, TextureCompressionQuality.Normal);
                textures2D[i].Apply();
            }

            var dataResult = new List<byte>();
            for (int z = 0; z < rz; z++)
            {
                var tex2DData = textures2D[z].GetRawTextureData<byte>();

                for (int i = 0; i < tex2DData.Length; i++)
                {
                    dataResult.Add(tex2DData[i]);
                }
            }

            var tex = new Texture3D(rx, ry, rz, textureFormat, false);
            tex.SetPixelData(dataResult.ToArray(), 0);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply(false);
            return tex;
        }
    }

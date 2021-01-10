using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Create3DTexture
{
    [MenuItem("Fruit Builder/Generate Textures")]
    public static void GenerateTextures()
    {
        var assetsDir = new DirectoryInfo(Application.dataPath);
        var fruitsDir = new DirectoryInfo(Path.Combine(assetsDir.Parent.FullName, "Content/Fruit"));
        if (fruitsDir.Exists)
        {
            foreach (var fruitDir in fruitsDir.EnumerateDirectories())
            {
                var slicesDir = new DirectoryInfo(Path.Combine(fruitDir.FullName, "Slices"));
                CreateVolumeTextureAsset(slicesDir.FullName, fruitDir.Name);
            }
        }
    }

    private static void CreateVolumeTextureAsset(string path, string assetName)
    {
        // Fixed size 128 for now
        const int size = 128;

        var volumeTexture = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        var volumeColors = new Color[size * size * size];

        var pngs = Directory.GetFiles(path, "*.png");
        for (var i = 0; i < pngs.Length; i++)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(pngs[i]));
            var colors = texture.GetPixels();
            Array.Copy(colors, 0, volumeColors, i * size * size, colors.Length);
        }

        volumeTexture.SetPixels(volumeColors);
        volumeTexture.Apply(false);

        volumeTexture = CompressTexture3D(volumeTexture, TextureFormat.BC7);

        AssetDatabase.CreateAsset(volumeTexture, $"Assets/{assetName}.asset");
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
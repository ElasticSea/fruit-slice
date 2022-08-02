using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Util;
using Fruits.Builder.EdgeTexture;
using Fruits.Sdf;
using UnityEditor;
using UnityEngine;

public class Builders : MonoBehaviour
{
    [SerializeField] private bool compress;
    [SerializeField] private Vector3 edgeTextureResolution = new Vector3(128, 128, 128);
    [SerializeField] private int edgeTextureRaysPerVoxel = 2048;
    [SerializeField] private BitsPerChannel edgeTextureBitsPerChannel = BitsPerChannel.Ch16;

    [SerializeField] private VolumeMesh target;
    
    [SerializeField] private ColorTextureBuilder colorTextureBuilder;
    [SerializeField] private EdgeTextureBuilder edgeTextureBuilder;
    
    public async void Build()
    {
        if (target == null)
        {
            throw  new ArgumentException("Target not setup.");
        }

        var colorTextureTask = BuildColorTexture();
        var edgeTextureTask = Task.FromResult(BuildEdgeTexture());
        
        await Task.WhenAll(colorTextureTask, edgeTextureTask);

        var colorTexture = await colorTextureTask;
        var edgeTexture = await edgeTextureTask;
        
        save(colorTexture, "Color Volume Texture");
        save(edgeTexture, "Edge Volume Texture");
        
        target.ColorTexture = colorTexture;
        target.EdgeTexture = edgeTexture;
        
        AssetDatabase.SaveAssetIfDirty(target);
    }

    private void save(Texture3D texture, string textureName)
    {
        var meshFileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(target.BlendFile));
        var formattableString = $"Builder/{meshFileName}/{textureName}.asset";
        var file = new FileInfo(Path.Combine(Application.dataPath, formattableString));
        Utils.EnsureDirectory(file.Directory.FullName);
        AssetDatabase.CreateAsset(texture, $"Assets/{formattableString}");
    }

    private Texture3D BuildEdgeTexture()
    {
        var instance = Instantiate(target.BlendFile);
        var allGOs = instance.transform.AllChildren(true);
        foreach (var go in allGOs)
        {
            if (go.name.Contains("volume", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                var mf = go.GetComponent<MeshFilter>();
                var mr = go.GetComponent<MeshRenderer>();
                if (mf)
                {
                    DestroyImmediate(mf);
                }
                if (mr)
                {
                    DestroyImmediate(mr);
                }
            }
        }
        
        // Turn off boolean modifiers in Realtime-viewport otherwise they will be applied during import

        var generateTexture = edgeTextureBuilder.GenerateTexture(instance.gameObject, target.Bounds, edgeTextureResolution,
            edgeTextureRaysPerVoxel, edgeTextureBitsPerChannel);
        
        DestroyImmediate(instance.gameObject);
        return generateTexture;
    }

    private async Task<Texture3D> BuildColorTexture()
    {
        var blenderBinaryPath = "C:/Program Files/Blender Foundation/Blender 2.91/blender.exe";
        var meshFilePath = new FileInfo(AssetDatabase.GetAssetPath(target.BlendFile)).FullName;
        var meshFileName = Path.GetFileNameWithoutExtension(meshFilePath);
        var exportPath = new DirectoryInfo("blender_output_temp").FullName;
        
        Utils.EnsureDirectory(exportPath, true);

        var fileName = $"\"{blenderBinaryPath}\" -b \"{meshFilePath}\" -o \"{exportPath}/slice_#####\" -a";
        print(fileName);
        
        await Utils.RunProcessAsync(fileName);
        var colorTexture = colorTextureBuilder.Convert(exportPath, $"{exportPath}/{meshFileName}", compress);
        Directory.Delete(exportPath, true);
        return colorTexture;
    }
}

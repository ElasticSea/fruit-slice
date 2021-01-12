using System;
using System.IO;
using Core.Util;
using UnityEditor;
using UnityEngine;

namespace Fruits.Sdf
{
    public class TextureScaler : MonoBehaviour
    {
        [SerializeField] private Texture3D texture;
        [SerializeField] private Vector3 scale;

        public void Scale()
        {
            var depth = texture.depth;
            var height = texture.height;
            var width = texture.width;
            var bytes = texture.GetPixelData<byte>(0).ToArray();
            
            var floats3d = BytesToFloats3d(bytes, width, height, depth);
            var newFloats3d = Interpolation.Scale(floats3d, (int) scale.x, (int) scale.y, (int) scale.z);
            var newBytes = Floats3dToBytes(newFloats3d);

            var newTexture = new Texture3D((int) scale.x, (int) scale.y, (int) scale.z, texture.graphicsFormat, 0)
            {
                filterMode = texture.filterMode,
                wrapMode = texture.wrapMode,
                name = $"{texture.name}_scaled"
            };
            newTexture.SetPixelData(newBytes,0);
            newTexture.Apply();

            var outputDir = Path.Combine(Application.dataPath, "Generated");
            Utils.EnsureDirectory(outputDir);
            AssetDatabase.CreateAsset(newTexture, $"Assets/Generated/{newTexture.name}.asset");
        }

        private float[,,] BytesToFloats3d(byte[] bytes, int width, int height, int depth)
        {
            var floats = new float[bytes.Length / 4];
            for (var i = 0; i < floats.Length; i++)
            {
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            }

            var floats3d = new float[width, height, depth];
            for (var z = 0; z < depth; z++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        floats3d[x, y, z] = floats[x + y * width + z * height * width];
                    }
                }
            }

            return floats3d;
        }

        private byte[] Floats3dToBytes(float[,,] floats3d)
        {
            var width = floats3d.GetLength(0);
            var height = floats3d.GetLength(1);
            var depth = floats3d.GetLength(2);
            
            var floats = new float[width * height * depth];
            for (var z = 0; z < depth; z++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                       floats[x + y * width + z * height * width] =  floats3d[x, y, z];
                    }
                }
            }

            var bytes = new byte[floats.Length * 4];
            for (var i = 0; i < floats.Length; i++)
            {
                var tempBytes = BitConverter.GetBytes(floats[i]);
                Array.Copy(tempBytes, 0, bytes, i * 4, 4);
            }

            return bytes;
        }
    }
}
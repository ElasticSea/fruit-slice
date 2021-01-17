using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Fruits.Builder.EdgeTexture.Editor
{
    [CustomEditor(typeof(EdgeTextureBuilder))]
    public class EdgeTextureBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                var sw = Stopwatch.StartNew();
                var sdfMaterialFactory = target as EdgeTextureBuilder;

                var duplicate = Instantiate(sdfMaterialFactory);
                duplicate.GenerateTexture();
                DestroyImmediate(duplicate.gameObject);
                
                Debug.Log($"Time: {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
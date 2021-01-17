using UnityEditor;
using UnityEngine;

namespace Fruits.Builder.Editor
{
    [CustomEditor(typeof(SlicedTextureBuilder))]
    public class SlicedTextureBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                var slicedTextureBuilder = target as SlicedTextureBuilder;
                slicedTextureBuilder.Generate();
            }
        }
    }
}
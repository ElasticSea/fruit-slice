using UnityEditor;
using UnityEngine;

namespace Fruits.Sdf.Editor
{
    [CustomEditor(typeof(TextureScaler))]
    public class TextureScalerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Scale"))
            {
                var sdfMaterialFactory = target as TextureScaler;
                sdfMaterialFactory.Scale();
            }
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace Fruits.Sdf.Editor
{
    [CustomEditor(typeof(SdfMaterialFactory))]
    public class SdfMaterialFactoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                var sdfMaterialFactory = target as SdfMaterialFactory;

                var duplicate = Instantiate(sdfMaterialFactory);
                sdfMaterialFactory.GeneratedTextures = duplicate.Generate().ToArray();
                DestroyImmediate(duplicate);
            }
            
            if (GUILayout.Button("Bake"))
            {
                var sdfMaterialFactory = target as SdfMaterialFactory;
                sdfMaterialFactory.Bake();
            }
        }
    }
}
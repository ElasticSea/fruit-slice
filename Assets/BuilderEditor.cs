using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Builders))]
public class BuilderEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Convert"))
        {
            var builders = (Builders) target;
            builders.Build();
        }
    }
}
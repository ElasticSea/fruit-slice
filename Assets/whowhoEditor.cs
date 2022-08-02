//C# Example (LookAtPointEditor.cs)

using Fruits;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(whowhowho))]
[CanEditMultipleObjects]
public class whowhowhoEditor : Editor 
{
    SerializedProperty lookAtPoint;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var dd = (whowhowho) target;
        var fruits = serializedObject.FindProperty("volumes");
        for (var i = 0; i < fruits.arraySize; i++)
        {
            var ddd = fruits.GetArrayElementAtIndex(i);
            var fff = ddd.objectReferenceValue as VolumeMesh;
            if (GUILayout.Button(fff.name))
            {
                dd.SetVolume(fff);
            }
        }
    }
}
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour))]
public class TestInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MonoBehaviour monoBehaviour = (MonoBehaviour)target;

        var content = new GUIContent("Test Inspector", "This is a test inspector for MonoBehaviour.");

        DrawDefaultInspector();
    }
}

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class DescriptionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var type = target.GetType();

        var attr = type.GetCustomAttributes(
            typeof(ComponentDescriptionAttribute),
            true
            );

        if (attr.Length > 0)
        {
            var desc = ((ComponentDescriptionAttribute)attr[0]).Description;

            var content = new GUIContent("コンポーネント説明", desc);
            EditorGUILayout.LabelField(content, EditorStyles.miniLabel);
        }

        DrawDefaultInspector();
    }
}
#endif
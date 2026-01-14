#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MissingScriptChecker
{
    static MissingScriptChecker()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null) return;

        if (HasMissingScripts(gameObject))
        {
            var color = new Color(0.8f, 0.2f, 0.2f, 0.35f);
            EditorGUI.DrawRect(selectionRect, color);
        }
    }

    private static bool HasMissingScripts(GameObject go)
    {
        return GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0;
    }
}
#endif
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 選択されたスクリプトがアタッチされている五部ジェクトを探すツール
/// </summary>
public class ScriptReverseDetection : EditorWindow
{
    private const float TILE_WIDTH = 90f;
    private const float ICON_SIZE = 64f;

    private List<MonoScript> scripts = new List<MonoScript>();
    private Vector2 scroll;

    [MenuItem("Tools/Script Reverse Detection")]
    static void Open()
    {
        GetWindow<ScriptReverseDetection>("Script Reverse Detection");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scriptを検索"))
        {
            EditorGUIUtility.ShowObjectPicker<MonoScript>(
            null,
            false,
            "",
            0
            );

        }

        // Object Picker で選択が変わった瞬間
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            var script = EditorGUIUtility.GetObjectPickerObject() as MonoScript;
            if (script == null) return;

            System.Type type = script.GetClass();
            if (type == null) return;

            if (!type.IsSubclassOf(typeof(MonoBehaviour)))return;

            if (type.IsAbstract) return;

            // シーン内から逆探知（非アクティブ含む）
            var found = Object.FindObjectsOfType(type, true);

            if (found.Length == 0)
            {
                Debug.Log("このスクリプトはシーン内で使われていません");
                return;
            }

            GameObject[] gos = new GameObject[found.Length];
            for (int i = 0; i < found.Length; i++)
            {
                gos[i] = ((Component)found[i]).gameObject;
            }

            // Hierarchyでハイライト
            Selection.objects = gos;

            // Inspector を前面に
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }
    }
}
#endif
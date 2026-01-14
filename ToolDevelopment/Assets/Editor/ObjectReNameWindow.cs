using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ObjectReNameWindow : EditorWindow
{
    [MenuItem("Tools/ObjectReName")]
    public static void ShowWindow()
    {
        GetWindow<ObjectReNameWindow>("Object ReName");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("オブジェクト名の末尾に付く (1)、(2)… を自動で削除します", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Hierarchyから選択してください(複数選択可)", EditorStyles.boldLabel);

        if (GUILayout.Button("削除"))
        {
            ObjectRenaming();
        }
    }

    private void ObjectRenaming()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "オブジェクト未選択",
                "Hierarchy でオブジェクトを選択してください。",
                "OK"
            );
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            Undo.RecordObject(obj, "Remove Object Name Number");

            // 末尾の (数字) を削除
            obj.name = Regex.Replace(obj.name, @"\s*\(\d+\)$", "");

            EditorUtility.SetDirty(obj);
        }
    }
}

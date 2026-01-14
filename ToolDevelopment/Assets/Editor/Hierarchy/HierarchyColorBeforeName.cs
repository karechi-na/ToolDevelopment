#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class HierarchyColorBeforeName
{
    static HierarchyColorBeforeName()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject != null)
        {
            int depth = CalculateDepth(gameObject);

            // 階層ごとのインデントの幅をピクセル単位で計算
            int indentWidth = 2; // 通常のインデント幅を14ピクセルとする
            int totalIndent = indentWidth * depth;

            // 階層の深さに応じたオフセットを適用（微調整用）
            int offset = depth * 2; // 階層の深さに2ピクセルずつオフセットを追加

            // オブジェクト名の前に色を描画するための矩形を定義
            Rect colorRect = new Rect(selectionRect.x + totalIndent - offset, selectionRect.y, 3, selectionRect.height);

            // 色を決定
            Color color = Color.HSVToRGB((depth * 0.1f) % 1.0f, 0.6f, 0.8f);

            // オブジェクト名の前に色を描画
            EditorGUI.DrawRect(colorRect, color);
        }
    }

    private static int CalculateDepth(GameObject obj)
    {
        int depth = 0;
        while (obj.transform.parent != null)
        {
            depth++;
            obj = obj.transform.parent.gameObject;
        }
        return depth;
    }
}
#endif
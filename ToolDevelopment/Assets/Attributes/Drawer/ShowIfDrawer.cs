#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector上でプロパティをトグル表示する属性のDrawer
/// </summary>
[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // トグル部分
        Rect toggleRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        property.isExpanded =
            EditorGUI.ToggleLeft(toggleRect, label, property.isExpanded);

        // 展開時のみ中身を描画
        if (property.isExpanded)
        {
            Rect fieldRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2f,
                position.width,
                EditorGUI.GetPropertyHeight(property, true)
            );

            EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
        }

        EditorGUI.EndProperty();

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return
                EditorGUIUtility.singleLineHeight +
                EditorGUI.GetPropertyHeight(property, true) +
                2f;
        }

        return EditorGUIUtility.singleLineHeight;

    }
}
#endif
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IntButtonAttribute))]
public class IntButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "IntButtonÇÕintêÍópÇ≈Ç∑ÅB");
            return;
        }

        IntButtonAttribute intButtonAttribute = (IntButtonAttribute)attribute;

        EditorGUI.BeginProperty(position, label, property);

        // ÉâÉxÉãïîï™
        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        EditorGUI.LabelField(labelRect, label);

        Rect fieldRect = new Rect(
            position.x + EditorGUIUtility.labelWidth,
            position.y,
            position.width - EditorGUIUtility.labelWidth,
            position.height
        );

        float buttonWidth = 30f;

        Rect minusRect = new Rect(
            fieldRect.x,
            fieldRect.y,
            buttonWidth,
            fieldRect.height
        );

        Rect valueRect = new Rect(
            fieldRect.x + buttonWidth,
            fieldRect.y,
            fieldRect.width - buttonWidth * 2,
            fieldRect.height
        );

        Rect plusRect = new Rect(
            fieldRect.x + fieldRect.width - buttonWidth,
            fieldRect.y,
            buttonWidth,
            fieldRect.height
        );

        if (GUI.Button(minusRect, "-"))
        {
            property.intValue = Mathf.Max(intButtonAttribute.min, property.intValue - 1);
        }
        property.intValue = EditorGUI.IntField(valueRect, property.intValue);

        if (GUI.Button(plusRect, "+"))
        {
            property.intValue = Mathf.Min(intButtonAttribute.max, property.intValue + 1);
        }

        EditorGUI.EndProperty();
    }
}
#endif
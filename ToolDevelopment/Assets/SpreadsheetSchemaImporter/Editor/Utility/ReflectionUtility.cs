using System;
using System.Reflection;
using UnityEngine;

public static class ReflectionUtility
{
    /// <summary>
    /// ScriptableObjectのフィールドに値をセット
    /// </summary>
    public static void SetFieldValue(ScriptableObject target, string fieldName, object value)
    {
        // 実際のScriptableObjectの型を取得
        Type type = target.GetType();

        // 指定された名前のフィールド情報を取得
        FieldInfo field = type.GetField(
            fieldName,
            // publicなインスタンスフィールドのみ検索
            BindingFlags.Public | BindingFlags.Instance
        );

        if (field == null)
            throw new Exception($"{type.Name} に {fieldName} フィールドが見つからないよ");

        // フィールドへ値を設定
        field.SetValue(target, value);
    }
}

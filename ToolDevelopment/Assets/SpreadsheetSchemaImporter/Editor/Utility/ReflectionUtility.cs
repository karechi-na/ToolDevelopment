using System;
using System.Reflection;
using UnityEngine;

public static class ReflectionUtility
{
    public static void SetFieldValue(ScriptableObject target, string fieldName, object value)
    {
        Type type = target.GetType();

        FieldInfo field = type.GetField(
            fieldName,
            BindingFlags.Public | BindingFlags.Instance
        );

        if (field == null)
            throw new Exception($"{type.Name} に {fieldName} フィールドが見つからないよ");

        field.SetValue(target, value);
    }
}

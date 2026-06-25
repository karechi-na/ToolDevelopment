using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetGenerator
{
    public static void Generate(SchemaData schema, string outputFolder)
    {
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));

        Type assetType = FindType(schema.ClassName);

        if (assetType == null)
            throw new Exception($"{schema.ClassName}型が見つからないよ。先にスクリプト生成してね。");

        if (!typeof(ScriptableObject).IsAssignableFrom(assetType))
            throw new Exception($"{schema.ClassName}はScriptableObjectじゃないよ。");

        Directory.CreateDirectory(outputFolder);

        foreach (RowData row in schema.Rows)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(assetType);

            foreach (FieldData field in schema.Fields)
            {
                if (!row.Values.TryGetValue(field.Name, out string rawValue))
                    throw new Exception($"{field.Name}の値が見つからないよ。");

                object convertValue = TypeUtility.ConvertValue(rawValue, field.Type);

                ReflectionUtility.SetFieldValue(asset, field.Name, convertValue);
            }

            string assetName = GetAssetName(row, schema);
            string assetPath = $"{outputFolder}/{SanitizeFileName(assetName)}.asset";

            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"{schema.ClassName}のAsset生成完了！");
    }

    private static Type FindType(string className)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(className);

            if(type != null) return type;
        }
        return null;
    }

    private static string GetAssetName(RowData row, SchemaData schema)
    {
        FieldData firstField = schema.Fields[0];

        if (row.Values.TryGetValue(firstField.Name, out string value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return Guid.NewGuid().ToString();
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c.ToString(), "");
        }

        return fileName;
    }
}

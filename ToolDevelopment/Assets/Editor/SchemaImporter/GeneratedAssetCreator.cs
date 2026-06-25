using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class GeneratedAssetCreator
{
    public static void CreateAssets(string csvText, string className, string outputFolder)
    {
        Type type = FindType(className);

        if (type == null)
        {
            Debug.LogError($"{className}型が見つからないよ。先にScriptableObjectスクリプトを生成してコンパイルしてね");
            return;
        }

        Directory.CreateDirectory(outputFolder);

        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        string[] fieldNames = SplitCsvLine(lines[0]);
        string[] fieldTypes = SplitCsvLine(lines[1]);

        for (int row = 2; row < lines.Length; row++)
        {
            string[] values = SplitCsvLine(lines[row]);

            ScriptableObject asset = ScriptableObject.CreateInstance(type);

            for (int i = 0; i < fieldNames.Length; i++)
            {
                string fieldName = fieldNames[i].Trim().Trim('"');
                string rawValue = values[i].Trim().Trim('"');

                FieldInfo field = type.GetField(fieldName);

                if (field == null)
                {
                    Debug.LogWarning($"{className} に {fieldName}が見つからないよ");
                    continue;
                }

                object convertValue = ConvertValue(rawValue, field.FieldType);
                field.SetValue(asset, convertValue);
            }

            string assetName = values[0].Trim().Trim('"');
            string safeName = SanitizeFileName(assetName);
            string assetPath = $"{outputFolder}/{safeName}.asset";

            AssetDatabase.CreateAsset(asset, assetPath);
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"{className}のasset生成完了！");
    }

    private static Type FindType(string className)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(className);
            if(type != null) return type;
        }
        return null;
    }

    private static object ConvertValue(string raw, Type type)
    {
        if(type == typeof(string))return raw;
        if(type == typeof(int)) return int.Parse(raw);
        if(type == typeof(float)) return float.Parse(raw);
        if (type == typeof(bool)) return ParseBool(raw);

        throw new NotSupportedException($"未対応の型だよ : {type.Name}");
    }

    private static bool ParseBool(string raw)
    {
        raw = raw.Trim().ToLower();

        return raw switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            "〇" => true,
            "○" => true,
            "×" => false,
            _ => throw new Exception($"bool変換できない値だよ：{raw}")
        };
    }

    private static string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }
}

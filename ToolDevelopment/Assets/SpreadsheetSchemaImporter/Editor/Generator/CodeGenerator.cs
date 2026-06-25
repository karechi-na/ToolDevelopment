using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CodeGenerator
{
    public static void Generate(SchemaData schema, string outputFolder)
    {
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));

        if (string.IsNullOrWhiteSpace(schema.ClassName))
            throw new Exception("ƒNƒ‰ƒX–¼‚ª‹ó‚¾‚æ");

        Directory.CreateDirectory(outputFolder);

        string code = BuildCode(schema);
        string path = Path.Combine(outputFolder, $"{schema.ClassName}.cs");

        File.WriteAllText(path, code, Encoding.UTF8);

        AssetDatabase.Refresh();

        Debug.Log($"{schema.ClassName}.cs ‚ð¶¬‚µ‚½‚æI path: {path}");
    }

    private static string BuildCode(SchemaData schema)
    {
        StringBuilder builder = new();

        builder.AppendLine("using UnityEngine;");
        builder.AppendLine();
        builder.AppendLine($"[CreateAssetMenu(menuName = \"Generated/{schema.ClassName}\")]");
        builder.AppendLine($"public class {schema.ClassName} : ScriptableObject");
        builder.AppendLine("{");

        foreach (FieldData field in schema.Fields)
        {
            builder.AppendLine($"   public {GetTypeName(field.Type)} {field.Name};");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string GetTypeName(FieldType type)
    {
        return type switch
        {
            FieldType.String => "string",
            FieldType.Int => "int",
            FieldType.Float => "float",
            FieldType.Bool => "bool",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}

using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectCodeGenerator
{
    public static void Generate(string csvText, string className, string outputFolder)
    {
        string[] lines = csvText.Split('\n');

        if (lines.Length < 2)
        {
            Debug.LogError("CSVは最低でも 1行目：変数名、2行目：型が必要だよ");
            return;
        }

        string[] fieldNames = SplitCsvLine(lines[0]);
        string[] fieldTypes = SplitCsvLine(lines[1]);
        Debug.Log($"fieldNames: {string.Join(" | ", fieldNames)}");
        Debug.Log($"fieldTypes: {string.Join(" | ", fieldTypes)}");

        if (fieldNames.Length != fieldTypes.Length)
        {
            Debug.LogError("変数名の数と型の数が合ってないよ");
            return;
        }

        Directory.CreateDirectory(outputFolder);

        string code = BuildCode(className, fieldNames, fieldTypes);
        string path = Path.Combine(outputFolder, $"{className}.cs");

        File.WriteAllText(path, code, Encoding.UTF8);

        AssetDatabase.Refresh();

        Debug.Log($"{className}.cs を生成したよ！ path: {path}");
    }

    private static string BuildCode(string className, string[] fieldNames, string[] fieldTypes)
    {
        StringBuilder builder = new();

        builder.AppendLine("using UnityEngine;");
        builder.AppendLine();
        builder.AppendLine($"[CreateAssetMenu(menuName = \"Generated/{className}\")]");
        builder.AppendLine($"public class {className} : ScriptableObject");
        builder.AppendLine("{");

        for (int i = 0; i < fieldNames.Length; i++)
        {
            string type = NormalizeType(fieldTypes[i].Trim());
            string name = fieldNames[i].Trim();

            builder.AppendLine($"   public {type} {name};");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string NormalizeType(string type)
    {
        type = type.Trim().Trim('"').ToLower();

        return type switch
        {
            "string" => "string",
            "int" => "int",
            "float" => "float",
            "bool" => "bool",
            _ => "string"
        };
    }

    private static string[] SplitCsvLine(string line)
    {
        return line.Trim().Split(',');
    }
}

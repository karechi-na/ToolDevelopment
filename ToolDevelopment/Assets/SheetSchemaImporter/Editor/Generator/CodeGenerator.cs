using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace Karechina.SchemaImporter
{
    /// <summary>
    /// SchemaDataを基にScriptableObjectクラスを生成するクラス
    /// </summary>
    public static class CodeGenerator
    {
        /// <summary>
        /// ScriptableObjectクラスを生成する
        /// </summary>
        /// <param name="schema">CSV/GSSから解析したスキーマ情報</param>
        /// <param name="outputFolder">生成したスクリプトの保存先フォルダ</param>
        public static void Generate(SchemaData schema, string outputFolder)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (string.IsNullOrWhiteSpace(schema.ClassName))
                throw new Exception("クラス名が空だよ");

            // 保存先フォルダが存在しない場合は作成
            Directory.CreateDirectory(outputFolder);

            // ScriptableObjectクラスのコードを生成
            string code = BuildCode(schema);
            // 保存先パスを作成
            string path = Path.Combine(outputFolder, $"{schema.ClassName}.cs");

            // UTF-8でC#ファイルを書き出す
            File.WriteAllText(path, code, Encoding.UTF8);

            // Unityへ新しく生成したスクリプトを認識させる
            AssetDatabase.Refresh();

            Debug.Log($"{schema.ClassName}.cs を生成したよ！ path: {path}");
        }

        /// <summary>
        /// ScriptableObjectクラスのソースコードを生成する
        /// </summary>
        private static string BuildCode(SchemaData schema)
        {
            StringBuilder builder = new();

            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();

            // CreateAssetMenu属性を付与
            builder.AppendLine($"[CreateAssetMenu(menuName = \"Generated/{schema.ClassName}\")]");

            // ScriptableObjectクラス宣言
            builder.AppendLine($"public class {schema.ClassName} : ScriptableObject");
            builder.AppendLine("{");

            // スキーマ情報をもとにフィールドを生成
            foreach (FieldData field in schema.Fields)
            {
                builder.AppendLine($"   public {GetTypeName(field.Type)} {field.Name};");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        /// <summary>
        /// FieldTypeをC#の型名へ変換する
        /// </summary>
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
}
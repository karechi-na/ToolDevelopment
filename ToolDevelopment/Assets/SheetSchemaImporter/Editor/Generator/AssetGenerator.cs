using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Karechina.SchemaImporter
{
    /// <summary>
    /// 解析済みのSchemaDataをもとに、ScriptableObjectアセットを生成するクラス
    /// </summary>
    public static class AssetGenerator
    {
        /// <summary>
        /// SchemaDataの行データからScriptableObjectアセットを生成する
        /// </summary>
        /// <param name="schema">CSV/GSSから解析したスキーマ情報</param>
        /// <param name="outputFolder">生成したAssetを保存するフォルダパス</param>
        public static void Generate(SchemaData schema, string outputFolder)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            // クラス名と一致するScriptableObject型を検索
            Type assetType = FindType(schema.ClassName);

            if (assetType == null)
                throw new Exception($"{schema.ClassName}型が見つからないよ。先にスクリプト生成してね。");

            // 見つかった型がScriptableObjectを継承しているか確認
            if (!typeof(ScriptableObject).IsAssignableFrom(assetType))
                throw new Exception($"{schema.ClassName}はScriptableObjectじゃないよ。");

            // 保存先フォルダが存在しない場合は、outputFolderのパスでフォルダを作成
            Directory.CreateDirectory(outputFolder);

            foreach (RowData row in schema.Rows)
            {
                // 検索した型からScriptableObjectインスタンスを生成
                ScriptableObject asset = ScriptableObject.CreateInstance(assetType);

                foreach (FieldData field in schema.Fields)
                {
                    // 行データからフィールドに対応する値を取得
                    if (!row.Values.TryGetValue(field.Name, out string rawValue))
                        throw new Exception($"{field.Name}の値が見つからないよ。");

                    // CSV上では文字列なので、スキーマで指定された型へ変換
                    object convertedValue = TypeUtility.ConvertValue(rawValue, field.Type);

                    // Reflectionを使って、生成したScriptableObjectのフィールドに値を設定
                    ReflectionUtility.SetFieldValue(asset, field.Name, convertedValue);
                }

                // 基本的に1列目の値をAsset名として使用
                string assetName = GetAssetName(row, schema);

                // ファイル名として使えない文字を除去して保存パスを作成
                string assetPath = $"{outputFolder}/{SanitizeFileName(assetName)}.asset";

                // Unityプロジェクト内にAssetとして保存
                AssetDatabase.CreateAsset(asset, assetPath);
                // 変更済みとしてマークし、保存対象にする
                EditorUtility.SetDirty(asset);
            }

            // 生成したAssetを保存し、Projectビューに反映
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{schema.ClassName}のAsset生成完了！");
        }

        /// <summary>
        /// 現在読み込まれているアセンブリから、指定されたクラスの型を探す
        /// </summary>
        private static Type FindType(string className)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(className);

                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// Asset名として使用する値を取得する
        /// </summary>
        private static string GetAssetName(RowData row, SchemaData schema)
        {
            // Ver1.0では1列目の値をAsset名として扱う
            FieldData firstField = schema.Fields[0];

            if (row.Values.TryGetValue(firstField.Name, out string value) &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            // 1列目が空の場合は一意な名前を生成
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// ファイル名として使えない文字を除去する
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            return fileName;
        }
    }

}
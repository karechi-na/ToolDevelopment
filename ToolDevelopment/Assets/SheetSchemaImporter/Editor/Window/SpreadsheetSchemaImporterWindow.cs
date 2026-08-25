using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Karechina.SchemaImporter
{
    /// <summary>
    /// Spreadsheet Schema ImporterのEditorWindow
    /// CSVまたはGoogle Spreadsheetを入力元として、
    /// ScriptableObjectクラスとアセットを生成する
    /// </summary>
    public class SpreadsheetSchemaImporterWindow : EditorWindow
    {
        // CSV入力時に使用するCSVファイル
        private TextAsset csvFile;

        private string sheetUrl;

        // Google Spreadsheet入力時に使用するスプレッドシートID
        private string sheetId;

        // Google Spreadsheet入力時に使用するシートGID
        private string sheetGid = "0";

        // 生成するScriptableObjectクラス名
        private string className = "Hoge";

        // 生成したC#スクリプトを保存するフォルダ
        private DefaultAsset scriptOutputFolder;

        // 生成したScriptableObjectアセットを保存するフォルダ
        private DefaultAsset assetOutputFolder;

        // CSV / Google Spreadsheet のどちらから読み込むか
        private DataSourceType dataSourceType = DataSourceType.Csv;

        [MenuItem("Tools/SpreadSheet Schema Importer")]
        private static void Open()
        {
            GetWindow<SpreadsheetSchemaImporterWindow>("Schema Importer");
        }

        private void OnGUI()
        {
            DrawHeader();

            DrawInputSetting();

            DrawOutputSetting();
        }

        /// <summary>
        /// ウィンドウ上部のタイトルを描画する
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Spreadsheet Schema Importer", EditorStyles.boldLabel);
            DrawSeparator();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 入力元・クラス名・スクリプト保存先・スクリプト生成ボタンを描画する
        /// </summary>
        private void DrawInputSetting()
        {
            EditorGUILayout.LabelField("クラス生成", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("入力設定", EditorStyles.boldLabel);

            // 入力元を選択
            dataSourceType = (DataSourceType)EditorGUILayout.EnumPopup(
                "入力元",
                dataSourceType
            );

            // 選択された入力元に応じて設定項目を切り替える
            switch (dataSourceType)
            {
                case DataSourceType.Csv:
                    DrawCsvSettings();
                    break;

                case DataSourceType.GoogleSpreadsheet:
                    DrawGoogleSpreadsheetSettings();
                    break;
            }

            EditorGUILayout.Space();

            // 生成するScriptableObjectクラス名
            EditorGUILayout.LabelField("ScriptableObjectクラス名");
            className = EditorGUILayout.TextField(className);

            // 生成するC#スクリプトの保存先
            EditorGUILayout.LabelField("生成するクラスを保存するフォルダ");
            scriptOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                scriptOutputFolder,
                typeof(DefaultAsset),
                false
            );

            EditorGUILayout.Space();

            // 開発確認用：CSV/GSSを正しくSchemaDataへ変換できるか確認する
            if (GUILayout.Button("Parse Test"))
                ParseTest();

            // SchemaDataをもとにScriptableObjectクラスを生成する
            if (GUILayout.Button("Generate Script"))
                GenerateScript();
        }

        /// <summary>
        /// アセット保存先・アセット生成ボタンを描画する
        /// </summary>
        private void DrawOutputSetting()
        {
            DrawSeparator();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("アセット生成", EditorStyles.boldLabel);

            // 生成するScriptableObjectアセットの保存先
            EditorGUILayout.LabelField("生成するアセットを保存するフォルダ");
            assetOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                assetOutputFolder,
                typeof(DefaultAsset),
                false
            );

            // 既に生成されているScriptableObjectクラスからアセットを生成する
            if (GUILayout.Button("Generate Assets"))
                GenerateAssets();
        }

        /// <summary>
        /// CSV入力用の設定欄を描画する
        /// </summary>
        private void DrawCsvSettings()
        {
            EditorGUILayout.LabelField("変換するCSVファイル");
            csvFile = (TextAsset)EditorGUILayout.ObjectField(
                csvFile,
                typeof(TextAsset),
                false
            );
        }

        /// <summary>
        /// Google Spreadsheet入力用の設定欄を描画する
        /// </summary>
        private void DrawGoogleSpreadsheetSettings()
        {
            EditorGUILayout.LabelField("Google Spreadsheet URL");
            sheetUrl = EditorGUILayout.TextField(sheetUrl);
            //EditorGUILayout.LabelField("Google Spreadsheet ID");
            //sheetId = EditorGUILayout.TextField(sheetId);

            //EditorGUILayout.LabelField("Sheet GID");
            //sheetGid = EditorGUILayout.TextField(sheetGid);
        }

        /// <summary>
        /// 入力データをSchemaDataへ変換できるか確認する
        /// </summary>
        private void ParseTest()
        {
            try
            {
                SchemaData schema = CreateSchema();

                Debug.Log($"ClassName: {schema.ClassName}");
                Debug.Log($"Fields: {string.Join(", ", schema.Fields.Select(f => $"{f.Type} {f.Name}"))}");
                Debug.Log($"Rows: {schema.Rows.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 入力データからScriptableObjectクラスを生成する
        /// </summary>
        private void GenerateScript()
        {
            try
            {
                SchemaData schema = CreateSchema();

                string outputPath = GetFolderPath(scriptOutputFolder);

                CodeGenerator.Generate(schema, outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 入力データからScriptableObjectアセットを生成する
        /// </summary>
        private void GenerateAssets()
        {
            try
            {
                SchemaData schema = CreateSchema();

                string outputPath = GetFolderPath(assetOutputFolder);

                AssetGenerator.Generate(schema, outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 選択された入力元からCSV文字列を取得し、SchemaDataへ変換する
        /// </summary>
        private SchemaData CreateSchema()
        {
            IDataSource dataSource = CreateDataSource();

            string csvText = dataSource.GetCsvText();

            CsvParser parser = new();

            return parser.Parse(csvText, className);
        }

        /// <summary>
        /// 選択された入力元に対応したDataSourceを作成する
        /// </summary>
        private IDataSource CreateDataSource()
        {
            return dataSourceType switch
            {
                DataSourceType.Csv
                    => new CsvFileDataSource(csvFile),

                DataSourceType.GoogleSpreadsheet
                    => CreateGoogleSpreadsheetDataSource(),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IDataSource CreateGoogleSpreadsheetDataSource()
        {
            var (sheetId, sheetGid) = ParseSpreadSheetUrl(sheetUrl);

            return new GssDataSource(sheetId, sheetGid);
        }

        private (string sheetId, string sheetGid) ParseSpreadSheetUrl(string url)
        {
            Match idMatch = Regex.Match(url, @"/d/([^/]+)");
            Match gidMatch = Regex.Match(url, @"gid=(\d+)");

            if (!idMatch.Success)
                throw new Exception("SpreadSheet IDを取得できませんでした。");

            string sheetId = idMatch.Groups[1].Value;
            string sheetGid = gidMatch.Success ? gidMatch.Groups[1].Value : "0";

            return (sheetId, sheetGid);
        }

        /// <summary>
        /// DefaultAssetからUnityプロジェクト内のフォルダパスを取得する
        /// </summary>
        private string GetFolderPath(DefaultAsset folder)
        {
            if (folder == null)
                throw new Exception("出力先フォルダが指定されてないよ。");

            string path = AssetDatabase.GetAssetPath(folder);

            if (!AssetDatabase.IsValidFolder(path))
                throw new Exception("指定された出力先がフォルダじゃないよ。");

            return path;
        }

        /// <summary>
        /// ウィンドウ幅に合わせた区切り線を描画する
        /// </summary>
        private static void DrawSeparator(int thickness = 1, int padding = 8)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, padding + thickness);

            rect.height = thickness;
            rect.y += padding * 0.5f;

            EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
        }
    }
}
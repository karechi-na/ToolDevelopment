using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpreadsheetSchemaImporterWindow : EditorWindow
{
    private TextAsset csvFile;

    private string sheetId;
    private string sheetGid;

    private string className = "Hoge";
    private DefaultAsset scriptOutputFolder;
    private DefaultAsset assetOutputFolder;
    private DataSourceType dataSourceType = DataSourceType.Csv;

    [MenuItem("Tools/SpreadSheet Schema Importer")]
    private static void Open()
    {
        GetWindow<SpreadsheetSchemaImporterWindow>("Schema Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Spreadsheet Schema Importer", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("______________________________________________________________");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("クラス生成", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("入力設定", EditorStyles.boldLabel);
        dataSourceType = (DataSourceType)EditorGUILayout.EnumPopup(
            "入力元",
            dataSourceType
        );

        switch(dataSourceType)
        {
            case DataSourceType.Csv:
                DrawCsvSettings();
                break;

            case DataSourceType.GoogleSpreadsheet:
                DrawGoogleSpreadsheetSettings();
                break;
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("ScriptableObjectクラス名");
        className = EditorGUILayout.TextField(className);

        EditorGUILayout.LabelField("生成するクラスを保存するフォルダ");
        scriptOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            scriptOutputFolder,
            typeof(DefaultAsset),
            false
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Parse Test"))
        {
            ParseTest();
        }

        if (GUILayout.Button("Generate Script"))
        {
            GenerateScript();
        }
        EditorGUILayout.LabelField("______________________________________________________________");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("アセット生成", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("生成するAssetを保存するフォルダ");
        assetOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            assetOutputFolder,
            typeof(DefaultAsset),
            false
        );

        if (GUILayout.Button("Generate Assets"))
        {
            GenerateAssets();
        }
    }

    private void DrawCsvSettings()
    {
        EditorGUILayout.LabelField("変換するCSVファイル");
        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            csvFile,
            typeof(TextAsset),
            false
        );
    }

    private void DrawGoogleSpreadsheetSettings()
    {
        EditorGUILayout.LabelField("Google Spreadsheet ID");
        sheetId = EditorGUILayout.TextField(sheetId);

        EditorGUILayout.LabelField("Sheet GID");
        sheetGid = EditorGUILayout.TextField(sheetGid);
    }

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

    private SchemaData CreateSchema()
    {
        IDataSource dataSource = CreateDataSource();

        string csvText = dataSource.GetCsvText();

        CsvParser parser = new();

        return parser.Parse(csvText, className);
    }

    private IDataSource CreateDataSource()
    {
        return dataSourceType switch
        {
            DataSourceType.Csv => new CsvFileDataSource(csvFile),
            DataSourceType.GoogleSpreadsheet => new GssDataSource(sheetId, sheetGid),
        };
    }

    private string GetFolderPath(DefaultAsset folder)
    {
        if (folder == null)
            throw new Exception("出力先フォルダが指定されてないよ。");

        string path = AssetDatabase.GetAssetPath(folder);

        if (!AssetDatabase.IsValidFolder(path))
            throw new Exception("指定された出力先がフォルダじゃないよ。");

        return path;
    }
}

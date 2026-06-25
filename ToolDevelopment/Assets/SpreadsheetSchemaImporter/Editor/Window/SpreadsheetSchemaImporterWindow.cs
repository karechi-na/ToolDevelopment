using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpreadsheetSchemaImporterWindow : EditorWindow
{
    private TextAsset csvFile;
    private string className = "Hoge";
    private DefaultAsset scriptOutputFolder;
    private DefaultAsset assetOutputFolder;

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

        EditorGUILayout.LabelField("変換するCSVファイル");
        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            csvFile,
            typeof(TextAsset),
            false
        );

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
        IDataSource dataSource = new CsvFileDataSource(csvFile);

        string csvText = dataSource.GetCsvText();

        CsvParser parser = new();

        return parser.Parse(csvText, className);
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

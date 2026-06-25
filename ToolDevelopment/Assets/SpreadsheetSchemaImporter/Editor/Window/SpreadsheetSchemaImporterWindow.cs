using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpreadsheetSchemaImporterWindow : EditorWindow
{
    private TextAsset csvFile;
    private string className = "Hoge";
    private DefaultAsset scriptOutputFolder;

    [MenuItem("Tools/SpreadSheet Schema Importer")]
    private static void Open()
    {
        GetWindow<SpreadsheetSchemaImporterWindow>("Schema Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Spreadsheet Schema Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "変換するCSVファイル",
            csvFile,
            typeof(TextAsset),
            false
        );

        className = EditorGUILayout.TextField("生成するクラス名", className);

        scriptOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "保存するフォルダ",
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

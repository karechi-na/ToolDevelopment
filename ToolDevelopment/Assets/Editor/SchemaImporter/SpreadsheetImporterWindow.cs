using UnityEditor;
using UnityEngine;

public class SpreadsheetImporterWindow : EditorWindow
{
    private TextAsset csvFile;
    private string className = "EnemyData";
    private string outputFolder = "Assets/Generated/Scripts";

    private string assetOutputFolder = "Assets/Generated/ScriptableObjects";

    [MenuItem("Tools/SpreadSheet Schema Importer")]
    private static void Open()
    {
        GetWindow<SpreadsheetImporterWindow>("Schema Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("CSVからScriptableObjectクラスを生成", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("使うCSVファイルをセット");
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile,typeof(TextAsset));
        EditorGUILayout.LabelField("生成するScriptableObjectスクリプトの名前を指定");
        className = EditorGUILayout.TextField("ClassName", className);
        EditorGUILayout.LabelField("保存フォルダを指定");
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate SO Script"))
        {
            if (csvFile == null)
            {
                Debug.LogError("CSVファイルを指定してね");
                return;
            }

            ScriptableObjectCodeGenerator.Generate(csvFile.text, className, outputFolder);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("ScriptableObjectを生成するフォルダ名を指定してね");

        assetOutputFolder = EditorGUILayout.TextField("Asset Output Folder", assetOutputFolder);

        if (GUILayout.Button("Generate Assets"))
        {
            if (csvFile == null)
            {
                Debug.LogError("CSVファイルを指定してね");
                return;
            }

            GeneratedAssetCreator.CreateAssets(csvFile.text, className, assetOutputFolder);
        }
    }
}

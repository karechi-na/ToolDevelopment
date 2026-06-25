using System.Linq;
using UnityEditor;
using UnityEngine;

public class TestWidow : EditorWindow
{
    private TextAsset csvFile;
    private string className = "PlayerData";

    [MenuItem("Tools/TEEEEEEEST!")]
    public static void Open()
    {
        GetWindow<TestWidow>("TEEEEEEEEEEEEST!!!!!!!!");
    }

    private void OnGUI()
    {
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset));
        className = EditorGUILayout.TextField("ClassName", className);

        if (GUILayout.Button("Parse Test"))
        {
            try
            {
                IDataSource dataSource = new CsvFileDataSource(csvFile);
                string csvText = dataSource.GetCsvText();

                CsvParser parser = new();
                SchemaData schema = parser.Parse(csvText, className);

                Debug.Log($"ClassName: {schema.ClassName}");
                Debug.Log($"Fields: {string.Join(",", schema.Fields.Select(f => $"{f.Type} {f.Name}"))}");
                Debug.Log($"Rows: {schema.Rows.Count}");
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}

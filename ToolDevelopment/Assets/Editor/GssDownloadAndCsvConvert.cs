#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;                        // CSVファイルの保存(Directory / File)に使用
using System.Linq;
using Unity.EditorCoroutines.Editor;    // Editor上でCoroutineを使うために必要
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;           // UnityWebRequestを使ってHTTP通信を行うために必要
using UnityEngine.Rendering;

public enum CsvValueType
{
    Int,
    Float,
    Bool,
    String
}

[Serializable]
public class CsvConvertRule
{
    [Tooltip("CSVの列番号(0始まり)")]
    public int csvColumnIndex;

    [Tooltip("SOに入れるキー")]
    public string key;

    [Tooltip("変換型")]
    public CsvValueType type;
}

public class GssDownloadAndCsvConvert : EditorWindow
{
    #region GSSダウンロードに使う変数
    private string sheetId;
    private string sheetName;
    #endregion

    private string lastDownloadedCsvPath;
    private bool csvDownloaded;

    #region CSVをSOに変換する際に使う変数
    private TextAsset csvFile;

    [Header("変換ルール")]
    [SerializeField] private int ruleCount = 0;
    [SerializeField] private List<CsvConvertRule> rules = new();

    private Vector2 scrollPos = Vector2.zero;

    private const string SAVE_FOLDER = "Assets/MasterData/SO";

    private const float RULE_HEIGHT = 90.0f;
    private const float MAX_RULE_AREA_HEIGHT = 350.0f;
    #endregion


    [MenuItem("Tools/GSS → SO Converter")]
    private static void Open()
    {
        GetWindow<GssDownloadAndCsvConvert>("GSS → SO");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("GSS → CSV → ScriptableObject", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        DrawGssReader();
        DrawCsvConverter();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawGssReader()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));

        // テキストラベルの表示
        // EditorStyles.boldLabel 太字スタイルを適用
        // ※GUILayoutはレイアウト自動調整付きのGUI要素を作成するためのクラス
        EditorGUILayout.LabelField("入力するデータ", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("https://docs.google.com/spreadsheets/d/【GSSのID】/edit?gid=0#gid=0", EditorStyles.boldLabel);
        // Editor上で文字列を入力
        sheetId = EditorGUILayout.TextField("Sheet ID", sheetId);
        EditorGUILayout.Space();
        GUILayout.Label("GSS左下のシートの名前", EditorStyles.boldLabel);
        sheetName = EditorGUILayout.TextField("Sheet Name", sheetName);
        // EditorGUILayoutはEditor専用のGUI要素を作成するためのクラス
        // 入力値を直接変数に格納できる
        // TextFieldは文字列入力フィールドを作成するメソッド
        // 第一引数はラベル、第二引数は初期値、戻り値は入力された文字列

        // 押されたフレームでtrueを返し処理を実行
        if (GUILayout.Button("Download CSV"))
        {
            csvDownloaded = false;

            // Editor上でCoroutineを開始
            EditorCoroutineUtility.StartCoroutine(
                DownloadCsv(), this
            );
        }

        if (csvDownloaded)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("CSV Downloaded", MessageType.Info);

            if (GUILayout.Button("このCSVファイルを使ってScriptableObjectに変換"))
            {
                csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(lastDownloadedCsvPath);
            }
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// GSSをCSV形式でダウンロードするCoroutine
    /// </summary>
    private IEnumerator DownloadCsv()
    {
        string url =
            $"https://docs.google.com/spreadsheets/d/{sheetId}/gviz/tq?tqx=out:csv&sheet={sheetName}";
        // GSSのCSV取得URL
        // tqx=out:csv　→　CSV形式で出力する指定
        // sheet=シート名　→　取得するシート名を指定

        // 指定したURLにHTTP GETリクエストを送信
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // リクエストの送信と完了まで待機
            yield return request.SendWebRequest();

            // エラーハンドリング
            // 通信エラー時はログを出して処理を中断
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                yield break;
            }

            // ダウンロードしたCSV文字列を保存処理に渡す
            SaveCsv(request.downloadHandler.text);
        }
    }

    /// <summary>
    /// CSVをAssets配下に保存する
    /// </summary>
    private void SaveCsv(string csvText)
    {
        // 保存先フォルダパス
        string folderPath = "Assets/MasterData/CSV";

        // フォルダが存在しない場合は作成
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // シート名をファイル名にして保存
        string filePath = $"{folderPath}/{sheetName}.csv";
        File.WriteAllText(filePath, csvText);

        // Unityエディタに変更を反映
        AssetDatabase.Refresh();

        lastDownloadedCsvPath = filePath;
        csvDownloaded = true;

        Debug.Log($"CSV saved : {filePath}");
    }

    private void DrawCsvConverter()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));

        EditorGUILayout.LabelField("CSV → ScriptableObject", EditorStyles.boldLabel);

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "CSV File",
            csvFile,
            typeof(TextAsset),
            false
        );

        EditorGUILayout.Space();
        DrawRuleSettings();

        GUI.enabled = csvFile != null && ruleCount > 0;

        if (GUILayout.Button("Convert"))
        {
            Convert();
        }
        GUI.enabled = true;
        EditorGUILayout.Space();
        GUILayout.EndVertical();
    }

    private void DrawRuleSettings()
    {
        EditorGUILayout.LabelField("変換ルール設定", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("ScriptableObjectに登録する要素数を入力", EditorStyles.boldLabel);
        int newCount = EditorGUILayout.IntField("Rule Count", ruleCount);
        if (newCount != ruleCount)
        {
            ruleCount = newCount;
            AdjustRuleList();
        }

        // 設定する各要素の説明
        if (ruleCount != 0)
        {
            EditorGUILayout.LabelField("CSV Column Index → Ruleと同じ数字", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Key → 要素の名前(ID → id等)", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type → 扱う型", EditorStyles.boldLabel);
        }


        // ScriptableObjectの要素設定
        float ruleAreaHeight =
            Mathf.Min(rules.Count * RULE_HEIGHT, MAX_RULE_AREA_HEIGHT);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(ruleAreaHeight));
        for (int i = 0; i < rules.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Rule {i}");

            rules[i].csvColumnIndex =
                EditorGUILayout.IntField("CSV Column Index", rules[i].csvColumnIndex);

            rules[i].key =
                EditorGUILayout.TextField("Key", rules[i].key);

            rules[i].type =
                (CsvValueType)EditorGUILayout.EnumPopup("Type", rules[i].type);

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    private void AdjustRuleList()
    {
        while (rules.Count < ruleCount)
            rules.Add(new CsvConvertRule());

        while (rules.Count > ruleCount)
            rules.RemoveAt(rules.Count - 1);
    }

    private void Convert()
    {
        if (!AssetDatabase.IsValidFolder(SAVE_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets/MasterData", "SO");
        }

        string[] lines =
            csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int row = 1; row < lines.Length; row++)
        {
            string[] columns = SplitCsvLine(lines[row]);

            try
            {
                // IDをint型にパース
                string rawId = columns[0].Trim().Trim('"').Trim('\uFEFF');
                int id = int.Parse(rawId);

                // 名前をトリミング
                string rawName = columns[1].Trim().Trim('"');
                string assetName = SanitizeFileName(rawName);

                // 名前が空だったときのフォールバック
                if (string.IsNullOrEmpty(assetName))
                {
                    assetName = rawId;
                }
                string assetPath = $"{SAVE_FOLDER}/{assetName}.asset";

                SampleMasterData data =
                    AssetDatabase.LoadAssetAtPath<SampleMasterData>(assetPath);
                if (data == null)
                {
                    data = CreateInstance<SampleMasterData>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                // 初期化(全上書き)
                //data.name = assetFileName;
                data.id = id;
                data.intValues.Clear();
                data.floatValues.Clear();
                data.boolValues.Clear();
                data.stringValues.Clear();

                foreach (var rule in rules)
                {
                    if (rule.csvColumnIndex >= columns.Length)
                    {
                        Debug.LogWarning($"列不足 行：{row + 1}");
                        continue;
                    }

                    string raw = columns[rule.csvColumnIndex];
                    ApplyRule(data, rule, raw);
                }

                EditorUtility.SetDirty(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV変換エラー 行：{row + 1}\n{e}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("変換完了");
    }

    private string[] SplitCsvLine(string line)
    {
        return line.Split(',')
                   .Select(v => v.Trim())
                   .ToArray();
    }

    private string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }

    private void ApplyRule(SampleMasterData data, CsvConvertRule rule, string raw)
    {
        raw = raw.Trim().Trim('"');

        switch (rule.type)
        {
            case CsvValueType.Int:
                {
                    int v = int.Parse(raw);

                    data.intValues.Add(new IntEntry
                    {
                        key = rule.key,
                        value = v
                    });
                    break;
                }

            case CsvValueType.Float:
                {
                    float v = float.Parse(raw);

                    data.floatValues.Add(new FloatEntry
                    {
                        key = rule.key,
                        value = v
                    });
                    break;
                }

            case CsvValueType.Bool:
                {
                    bool v = bool.Parse(raw.ToLower());

                    data.boolValues.Add(new BoolEntry
                    {
                        key = rule.key,
                        value = v
                    });
                    break;
                }

            case CsvValueType.String:
                {
                    string v = raw;

                    data.stringValues.Add(new StringEntry
                    {
                        key = rule.key,
                        value = v
                    });
                    break;
                }
        }

    }
}

#endif
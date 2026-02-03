#if UNITY_EDITOR
//=============================================================
// CSVファイルをScriptableObjectに変換するエディタ拡張
//=============================================================

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

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

public class CsvToSoConverter : EditorWindow
{
    private TextAsset csvFile;

    [Header("変換ルール")]
    [SerializeField] private int ruleCount = 0;
    [SerializeField] private List<CsvConvertRule> rules = new();

    private const string SAVE_FOLDER = "Assets/MasterData/SO";

    [MenuItem("Tools/ CSV → SO Converter")]
    private static void Open()
    {
        GetWindow<CsvToSoConverter>("CSV → SO");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV → ScriptableObject", EditorStyles.boldLabel);

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
    }

    private void DrawRuleSettings()
    {
        EditorGUILayout.LabelField("変換ルール設定", EditorStyles.boldLabel);

        int newCount = EditorGUILayout.IntField("Rule Count", ruleCount);
        if (newCount != ruleCount)
        {
            ruleCount = newCount;
            AdjustRuleList();
        }

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
                string rawId = columns[0].Trim().Trim('"').Trim('\uFEFF');
                int id = int.Parse(rawId);
                string assetPath = $"{SAVE_FOLDER}/{id}.asset";

                SampleMasterData data =
                    AssetDatabase.LoadAssetAtPath<SampleMasterData>(assetPath);
                if (data == null)
                {
                    data = CreateInstance<SampleMasterData>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                // 初期化(全上書き)
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
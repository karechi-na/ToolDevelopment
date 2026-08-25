using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;

public class PlayerPrefsViewerWindow : EditorWindow
{
    // 最小ウィンドウサイズ
    private const float MinWidth = 420f;
    private const float MinHeight = 360f;

    // 除外するキーのリスト
    private static readonly HashSet<string> ExcludeKeys = new() {
            "UnityGraphicsQuality",
            "unity.cloud_userid",
            "unity.player_session_count",
            "unity.player_sessionid"
    };

    // PlayerPrefsのデータを保持するリスト
    private readonly List<PlayerPrefsData> prefsListData = new();

    [MenuItem("Tools/PlayerPrefs Viewer")]
    public static void ShowWindow()
    {
        GetWindow<PlayerPrefsViewerWindow>("PlayerPrefs Viewer");
    }

    private void OnEnable()
    {
        // ウィンドウの最小サイズを設定
        minSize = new Vector2(MinWidth, MinHeight);
    }

    /// <summary>
    /// GUIの作成
    /// </summary>
    public void CreateGUI()
    {
        // GUIのルート要素を取得
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Editor/PlayerPrefsViewer/PlayerPrefsViewer.uxml"
        );

        // GUIのルート要素にUXMLを適用
        visualTree.CloneTree(rootVisualElement);

        // スタイルシートをロードして適用
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/Editor/PlayerPrefsViewer/PlayerPrefsViewer.uss"
        );

        // スタイルシートをルート要素に追加
        rootVisualElement.styleSheets.Add(styleSheet);

        // ---- UI要素の取得 ----
        // Refreshボタン
        var refreshButton = rootVisualElement.Q<Button>("refresh-button");

        // Key入力フィールド
        var keyField = rootVisualElement.Q<TextField>("key-field");
        // Type選択フィールド
        var typeField = rootVisualElement.Q<DropdownField>("type-field");
        // Value入力フィールド
        var valueField = rootVisualElement.Q<TextField>("value-field");

        // Newボタン
        var newButton = rootVisualElement.Q<Button>("new-button");
        // Saveボタン
        var saveButton = rootVisualElement.Q<Button>("save-button");
        // Deleteボタン
        var deleteButton = rootVisualElement.Q<Button>("delete-button");

        // PlayerPrefsのリスト表示用ListView
        var prefsList = rootVisualElement.Q<ListView>("prefs-list");

        // ---- イベントハンドラの設定 ----
        // Refreshボタンのクリックイベント
        refreshButton.clicked += () =>
        {
            RefreshList(prefsList);
        };

        // Newボタンのクリックイベント
        newButton.clicked += () =>
        {
            prefsList.ClearSelection();

            keyField.value = string.Empty;
            typeField.value = "String";
            valueField.value = string.Empty;

            keyField.Focus();
        };

        // Saveボタンのクリックイベント
        saveButton.clicked += () =>
        {
            if (SavePlayerPrefs(
                keyField.value,
                typeField.value,
                valueField.value))
            {
                RefreshList(prefsList);
            }
        };

        // Deleteボタンのクリックイベント
        deleteButton.clicked += () =>
        {
            if (DeletePlayerPrefs(
                keyField.value,
                valueField))
            {
                RefreshList(prefsList);
            }
        };

        // ---- ListViewの設定 ----

        LoadPlayerPrefsData();

        prefsList.itemsSource = prefsListData;
        prefsList.makeItem = () =>
        {
            var row = new VisualElement();

            row.AddToClassList("prefs-row");

            var keyLabel = new Label();
            keyLabel.name = "key-label";
            keyLabel.AddToClassList("key-column");

            var typeLabel = new Label();
            typeLabel.name = "type-label";
            typeLabel.AddToClassList("type-column");

            var valueLabel = new Label();
            valueLabel.name = "value-label";
            valueLabel.AddToClassList("value-column");

            row.Add(keyLabel);
            row.Add(typeLabel);
            row.Add(valueLabel);

            return row;
        };

        prefsList.bindItem = (element, index) =>
        {
            var keyLabel = element.Q<Label>("key-label");
            var typeLabel = element.Q<Label>("type-label");
            var valueLabel = element.Q<Label>("value-label");

            PlayerPrefsData data = prefsListData[index];

            keyLabel.text = data.Key;
            typeLabel.text = data.Type;
            valueLabel.text = data.Value;
        };

        prefsList.selectionType = SelectionType.Single;
        prefsList.selectionChanged += selectedItems =>
        {
            foreach (var item in selectedItems)
            {
                if (item is not PlayerPrefsData data) return;

                keyField.value = data.Key;
                typeField.value = data.Type;
                valueField.value = data.Value;

                break;
            }
        };
    }


    private void RefreshList(ListView prefsList)
    {
        LoadPlayerPrefsData();
        prefsList.RefreshItems();
    }

    /// <summary>
    /// フィールドで設定した値をPlayerPrefsに保存
    /// </summary>
    private bool SavePlayerPrefs(string key, string type, string value)
    {
        // Keyが空白の場合は警告を表示して保存しない
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning("Keyを入力して下さい");
            return false;
        }

        // 各Typeに応じてPlayerPrefsに保存する
        switch (type)
        {
            case "String":
                PlayerPrefs.SetString(key, value);
                break;
            case "Int":
                if (!int.TryParse(value, out int intValue))
                {
                    Debug.LogWarning("Int型の値を入力して下さい");
                    return false;
                }
                PlayerPrefs.SetInt(key, intValue);
                break;
            case "Float":
                if (!float.TryParse(value, out float floatValue))
                {
                    Debug.LogWarning("Float型の値を入力して下さい");
                    return false;
                }

                PlayerPrefs.SetFloat(key, floatValue);
                break;
        }

        // PlayerPrefsに保存
        PlayerPrefs.Save();
        Debug.Log($"PlayerPrefsに保存しました: Key={key}, Type={type}, Value={value}");
        return true;
    }

    private bool DeletePlayerPrefs(string key, TextField valueField)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning("Keyを入力して下さい");
            return false;
        }

        if (!PlayerPrefs.HasKey(key))
        {
            Debug.LogWarning($"指定されたKeyは存在しません: {key}");
            return false;
        }

        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        valueField.value = string.Empty;
        Debug.Log($"PlayerPrefsから削除しました: Key={key}");

        return true;
    }

    private void LoadPlayerPrefsData()
    {
        prefsListData.Clear();

        string registryPath =
            $@"Software\Unity\UnityEditor\{Application.companyName}\{Application.productName}";

        using RegistryKey registryKey =
            Registry.CurrentUser.OpenSubKey(registryPath);

        if (registryKey == null) return;

        foreach (string valueName in registryKey.GetValueNames())
        {
            string key = RemoveHash(valueName);

            if (ShouldExclude(key)) continue;

            object value = registryKey.GetValue(valueName);

            string type = GetPlayerPrefsType(value);

            prefsListData.Add(new PlayerPrefsData()
            {
                Key = key,
                Type = type,
                Value = GetPlayerPrefsValue(key, type)
            });
        }
    }

    private bool ShouldExclude(string key)
    {
        return ExcludeKeys.Contains(key);
    }

    private string GetPlayerPrefsType(object value)
    {
        return value switch
        {
            string => "String",
            int => "Int",
            long => "Float",
            _ => "Unknown"
        };
    }

    private string GetPlayerPrefsValue(string key, string type)
    {
        return type switch
        {
            "String" => PlayerPrefs.GetString(key),
            "Int" => PlayerPrefs.GetInt(key).ToString(),
            "Float" => PlayerPrefs.GetFloat(key).ToString(),
            _ => "_"
        };
    }

    private string RemoveHash(string key)
    {
        // ハッシュを削除
        return Regex.Replace(
            key,
            @"_h\d+$",
            string.Empty);
    }


    private class PlayerPrefsData
    {
        public string Key;
        public string Type;
        public string Value;
    }
}

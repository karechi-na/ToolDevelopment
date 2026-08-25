using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


// ============================================================
// Folder Color Settings
// ============================================================

[Serializable]
public sealed class FolderColorSettings
{
    private const string SavePath =
        "ProjectSettings/FolderColors.json";


    [Serializable]
    public class FolderColorData
    {
        public string guid;
        public Color color;
    }


    [SerializeField]
    private List<FolderColorData> folders = new();


    // ========================================================
    // Singleton
    // ========================================================

    [NonSerialized]
    private static FolderColorSettings _instance;


    public static FolderColorSettings instance
    {
        get
        {
            if (_instance == null)
            {
                Load();
            }

            return _instance;
        }
    }


    // ========================================================
    // Load
    // ========================================================

    public static void Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                _instance =
                    new FolderColorSettings();

                return;
            }


            string json = File.ReadAllText(SavePath);


            if (string.IsNullOrWhiteSpace(json))
            {
                _instance = new FolderColorSettings();

                return;
            }


            _instance = JsonUtility.FromJson<FolderColorSettings>(json);


            if (_instance == null)
            {
                _instance = new FolderColorSettings();
            }


            if (_instance.folders == null)
            {
                _instance.folders = new List<FolderColorData>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[FolderColor] Failed to load settings.\n"
                + e);

            _instance = new FolderColorSettings();
        }
    }


    // ========================================================
    // Reload
    // Unity起動時などに強制再読み込み
    // ========================================================

    public static void Reload()
    {
        _instance = null;

        Load();
    }


    // ========================================================
    // Save
    // ========================================================

    private void Save()
    {
        try
        {
            string directory = Path.GetDirectoryName(SavePath);


            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(this, true);


            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[FolderColor] Failed to save settings.\n"
                + e);
        }
    }


    // ========================================================
    // Get
    // ========================================================

    public bool TryGetColor(string guid, out Color color)
    {
        if (folders == null)
        {
            folders = new List<FolderColorData>();
        }

        foreach (FolderColorData data in folders)
        {
            if (data.guid == guid)
            {
                color = data.color;

                return true;
            }
        }

        color = Color.white;

        return false;
    }


    // ========================================================
    // Set
    // ========================================================

    public void SetColor(string guid, Color color)
    {
        if (folders == null)
        {
            folders = new List<FolderColorData>();
        }

        foreach (FolderColorData data in folders)
        {
            if (data.guid == guid)
            {
                data.color = color;

                Save();

                return;
            }
        }

        folders.Add(
            new FolderColorData
            {
                guid = guid,
                color = color
            });

        Save();
    }


    // ========================================================
    // Remove
    // ========================================================

    public void RemoveColor(string guid)
    {
        if (folders == null) return;

        folders.RemoveAll(x => x.guid == guid);

        Save();
    }


    // ========================================================
    // Clear
    // ========================================================

    public void Clear()
    {
        if (folders == null)
        {
            folders = new List<FolderColorData>();
        }
        else
        {
            folders.Clear();
        }

        Save();
    }
}


// ============================================================
// Folder Color Editor
// ============================================================

[InitializeOnLoad]
public static class FolderColorEditor
{
    private static Texture2D folderIcon;

    private static GUIStyle listLabelStyle;
    private static GUIStyle gridLabelStyle;


    // ========================================================
    // 微調整は基本ここだけ
    // ========================================================

    private const float ListIconSize = 16f;

    // Grid表示時のアイコン余白
    private const float GridPadding = 2f;

    // Grid表示時のラベル高さ
    private const float GridLabelHeight = 18f;


    static FolderColorEditor()
    {
        FolderColorSettings.Reload();

        EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;

        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

        EditorApplication.delayCall += () =>
        {
            CacheResources();

            EditorApplication.RepaintProjectWindow();
        };
    }


    private static void CacheResources()
    {
        folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;

        // ProjectWindow を確実に再描画
        EditorApplication.RepaintProjectWindow();
    }


    // ============================================================
    // Main
    // ============================================================

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        if (Event.current.type != EventType.Repaint) return;

        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (string.IsNullOrEmpty(path)) return;
        if (!AssetDatabase.IsValidFolder(path)) return;
        if (!FolderColorSettings.instance.TryGetColor(guid, out Color color)) return;

        if (folderIcon == null)
        {
            folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;

            if (folderIcon == null) return;
        }

        string folderName = Path.GetFileName(path);

        // Project Window の表示判定
        bool isGridView = selectionRect.height > 20f;

        bool isSelected = IsSelected(guid);

        bool isHovered = selectionRect.Contains(Event.current.mousePosition);

        bool isRenaming = EditorGUIUtility.editingTextField && isSelected;


        if (isGridView)
        {
            DrawGridView(
                selectionRect,
                folderName,
                color,
                isSelected,
                isHovered,
                isRenaming);
        }
        else
        {
            DrawListView(
                selectionRect,
                folderName,
                color,
                isSelected,
                isHovered,
                isRenaming);
        }
    }


    // ============================================================
    // Selection
    // ============================================================

    private static bool IsSelected(string guid)
    {
        string[] selectedGuids = Selection.assetGUIDs;

        if (selectedGuids == null) return false;

        for (int i = 0; i < selectedGuids.Length; i++)
        {
            if (selectedGuids[i] == guid) return true;
        }

        return false;
    }


    // ============================================================
    // Background
    // ============================================================

    private static Color GetNormalBackground()
    {
        if (EditorGUIUtility.isProSkin)
        {
            return new Color(0.219f, 0.219f, 0.219f, 1f);
        }

        return new Color(0.76f, 0.76f, 0.76f, 1f);
    }


    private static Color GetHoverBackground()
    {
        if (EditorGUIUtility.isProSkin)
        {
            return new Color(0.27f, 0.27f, 0.27f, 1f);
        }

        return new Color(0.82f, 0.82f, 0.82f, 1f);
    }


    private static Color GetSelectionBackground()
    {
        // UnityのDark Themeに近い選択色
        if (EditorGUIUtility.isProSkin)
        {
            return new Color(0.17f, 0.36f, 0.55f, 1f);
        }

        return new Color(0.24f, 0.49f, 0.75f, 1f);
    }


    private static void DrawBackground(Rect rect, bool selected, bool hovered)
    {
        Color background;

        if (selected)
        {
            background = GetSelectionBackground();
        }
        else if (hovered)
        {
            background = GetHoverBackground();
        }
        else
        {
            background = GetNormalBackground();
        }

        EditorGUI.DrawRect(rect, background);
    }


    // ============================================================
    // List View
    // ============================================================

    private static void DrawListView(Rect rect, string folderName, Color color, bool selected, bool hovered, bool isRenaming)
    {
        // ========================================================
        // アイコン領域
        // ========================================================

        Rect iconArea =
            new Rect(
                rect.x,
                rect.y,
                20f,
                rect.height);


        // ========================================================
        // ラベル領域
        // ========================================================

        Rect labelRect =
            new Rect(
                rect.x + 20f,
                rect.y,
                Mathf.Max(0f, rect.width - 20f),
                rect.height);


        // ========================================================
        // 背景
        // ========================================================

        // アイコン部分は常に自前背景
        DrawBackground(iconArea, selected, hovered);


        // 通常時のみ文字部分も塗る
        if (!isRenaming)
        {
            DrawBackground(labelRect, selected, hovered);
        }


        // ========================================================
        // Icon
        // ========================================================

        Rect iconRect =
            new Rect(
                rect.x + 1f,
                rect.y +
                (rect.height - ListIconSize) * 0.5f,
                ListIconSize,
                ListIconSize);


        DrawTintedIcon(iconRect, color);


        // ========================================================
        // Label
        // ========================================================

        if (isRenaming) return;


        GUIStyle style = GetListLabelStyle(color, selected);

        GUI.Label(labelRect, folderName, style);
    }


    // ============================================================
    // Grid View
    // ============================================================

    private static void DrawGridView(Rect rect, string folderName, Color color, bool selected, bool hovered, bool isRenaming)
    {
        // ========================================================
        // アイコン領域
        // ========================================================

        float iconAreaHeight = Mathf.Max(16f, rect.height - GridLabelHeight);

        Rect iconArea =
            new Rect(
                rect.x,
                rect.y,
                rect.width,
                iconAreaHeight);


        // ========================================================
        // ラベル領域
        // ========================================================

        Rect labelRect =
            new Rect(
                rect.x,
                rect.yMax - GridLabelHeight,
                rect.width,
                GridLabelHeight);


        // ========================================================
        // 背景
        // ========================================================

        // アイコン領域は常に塗る
        // → Unity標準フォルダアイコンを隠す
        DrawBackground(iconArea, false, hovered);

        // 通常時だけラベル領域も塗る
        // Rename中はUnity標準の入力欄を見せる
        if (!isRenaming)
        {
            DrawBackground(labelRect, selected, hovered);
        }


        // ========================================================
        // Icon
        // ========================================================

        float iconSize = Mathf.Min(iconArea.width, iconArea.height);

        iconSize -= GridPadding * 2f;

        iconSize = Mathf.Max(16f, iconSize);

        Rect iconRect =
            new Rect(
                iconArea.center.x - iconSize * 0.5f,
                iconArea.center.y - iconSize * 0.5f,
                iconSize,
                iconSize);


        // Rename中でも色付きアイコンは維持
        DrawTintedIcon(iconRect, color);


        // ========================================================
        // Folder Name
        // ========================================================

        // Rename中はUnity標準描画に任せる
        if (isRenaming) return;

        GUIStyle style = GetGridLabelStyle(color, selected);

        GUI.Label(labelRect, folderName, style);
    }


    // ============================================================
    // Icon
    // ============================================================

    private static void DrawTintedIcon(Rect rect, Color color)
    {
        Color oldColor = GUI.color;

        GUI.color = color;

        GUI.DrawTexture(rect, folderIcon, ScaleMode.ScaleToFit, true);

        GUI.color = oldColor;
    }


    // ============================================================
    // Label Style
    // ============================================================

    private static GUIStyle GetListLabelStyle(Color color, bool selected)
    {
        if (listLabelStyle == null)
        {
            listLabelStyle =
                new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,

                    clipping = TextClipping.Clip
                };
        }


        ApplyTextColor(listLabelStyle, color, selected);

        return listLabelStyle;
    }


    private static GUIStyle GetGridLabelStyle(Color color, bool selected)
    {
        if (gridLabelStyle == null)
        {
            gridLabelStyle =
                new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,

                    clipping = TextClipping.Clip,

                    wordWrap = false
                };
        }

        ApplyTextColor(gridLabelStyle, color, selected);

        return gridLabelStyle;
    }


    private static void ApplyTextColor(GUIStyle style, Color folderColor, bool selected)
    {
        /*
         * 選択中でもフォルダ色を維持。
         *
         * 選択中だけ白文字にしたい場合は
         *
         * Color textColor =
         *     selected ? Color.white : folderColor;
         *
         * に変更すればOK。
         */

        Color textColor = folderColor;


        style.normal.textColor = textColor;

        style.hover.textColor = textColor;

        style.active.textColor = textColor;

        style.focused.textColor = textColor;

        style.onNormal.textColor = textColor;

        style.onHover.textColor = textColor;

        style.onActive.textColor = textColor;

        style.onFocused.textColor = textColor;
    }


    // ============================================================
    // Preset
    // ============================================================

    [MenuItem("Assets/Folder Color/Red", false, 2000)]
    private static void Red()
    {
        SetColor(new Color(0.95f, 0.30f, 0.30f));
    }

    [MenuItem("Assets/Folder Color/Orange", false, 2001)]
    private static void Orange()
    {
        SetColor(new Color(1.00f, 0.55f, 0.20f));
    }

    [MenuItem("Assets/Folder Color/Yellow", false, 2002)]
    private static void Yellow()
    {
        SetColor(new Color(1.00f, 0.82f, 0.25f));
    }

    [MenuItem("Assets/Folder Color/Green", false, 2003)]
    private static void Green()
    {
        SetColor(new Color(0.35f, 0.85f, 0.40f));
    }


    [MenuItem("Assets/Folder Color/Cyan", false, 2004)]
    private static void Cyan()
    {
        SetColor(new Color(0.25f, 0.85f, 0.90f));
    }

    [MenuItem("Assets/Folder Color/Blue", false, 2005)]
    private static void Blue()
    {
        SetColor(new Color(0.35f, 0.60f, 1.00f));
    }


    [MenuItem("Assets/Folder Color/Purple", false, 2006)]
    private static void Purple()
    {
        SetColor(new Color(0.70f, 0.45f, 1.00f));
    }


    [MenuItem("Assets/Folder Color/Pink", false, 2007)]
    private static void Pink()
    {
        SetColor(new Color(1.00f, 0.40f, 0.70f));
    }


    [MenuItem("Assets/Folder Color/Gray", false, 2008)]
    private static void Gray()
    {
        SetColor(new Color(0.65f, 0.65f, 0.65f));
    }


    // ============================================================
    // Custom
    // ============================================================

    [MenuItem("Assets/Folder Color/Custom...", false, 2050)]
    private static void Custom()
    {
        FolderColorPickerWindow.Open();
    }


    // ============================================================
    // Reset
    // ============================================================

    [MenuItem("Assets/Folder Color/Reset", false, 2100)]
    private static void Reset()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!AssetDatabase.IsValidFolder(path)) continue;

            string guid = AssetDatabase.AssetPathToGUID(path);

            FolderColorSettings.instance.RemoveColor(guid);
        }

        EditorApplication.RepaintProjectWindow();
    }


    // ============================================================
    // Menu Validation
    // ============================================================

    [MenuItem("Assets/Folder Color/Red", true)]
    [MenuItem("Assets/Folder Color/Orange", true)]
    [MenuItem("Assets/Folder Color/Yellow", true)]
    [MenuItem("Assets/Folder Color/Green", true)]
    [MenuItem("Assets/Folder Color/Cyan", true)]
    [MenuItem("Assets/Folder Color/Blue", true)]
    [MenuItem("Assets/Folder Color/Purple", true)]
    [MenuItem("Assets/Folder Color/Pink", true)]
    [MenuItem("Assets/Folder Color/Gray", true)]
    [MenuItem("Assets/Folder Color/Custom...", true)]
    [MenuItem("Assets/Folder Color/Reset", true)]
    private static bool ValidateMenu()
    {
        if (Selection.objects == null ||
            Selection.objects.Length == 0)
        {
            return false;
        }


        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (AssetDatabase.IsValidFolder(path)) return true;
        }

        return false;
    }


    // ============================================================
    // Set Color
    // ============================================================

    public static void SetColor(Color color)
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!AssetDatabase.IsValidFolder(path)) continue;

            string guid = AssetDatabase.AssetPathToGUID(path);

            FolderColorSettings.instance.SetColor(guid, color);
        }


        EditorApplication.RepaintProjectWindow();
    }
}


// ============================================================
// Custom Color Picker
// ============================================================

public class FolderColorPickerWindow : EditorWindow
{
    private Color selectedColor = new Color( 0.35f, 0.60f, 1.00f);


    public static void Open()
    {
        FolderColorPickerWindow window = CreateInstance<FolderColorPickerWindow>();

        window.titleContent = new GUIContent("Folder Color");

        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (AssetDatabase.IsValidFolder(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);

                if (FolderColorSettings.instance.TryGetColor(guid, out Color current))
                {
                    window.selectedColor = current;
                }
            }
        }

        Vector2 size = new Vector2(300f, 110f);

        window.minSize = size;

        window.maxSize = size;
        
        window.ShowUtility();
    }


    private void OnGUI()
    {
        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Folder Color", EditorStyles.boldLabel);

        GUILayout.Space(5f);

        selectedColor = EditorGUILayout.ColorField("Color", selectedColor);

        GUILayout.FlexibleSpace();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Cancel", GUILayout.Height(25f)))
            {
                Close();
            }

            if (GUILayout.Button("Apply", GUILayout.Height(25f)))
            {
                FolderColorEditor.SetColor(selectedColor);

                Close();
            }
        }

        GUILayout.Space(8f);
    }
}
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImageSplitterWindow : EditorWindow
{
    private Texture2D sourceTexture;
    private int columns = 3;
    private int rows = 3;
    private float zoom = 1f;
    private Vector2 pan = Vector2.zero;
    private bool panning;
    private Vector2 lastMouse;
    [SerializeField] private float sidebarWidth = 270f;
    private bool resizingSidebar;
    private const float MinSidebarWidth = 220f;
    private const float MinPreviewWidth = 180f;
    private const float DividerWidth = 5f;
    private const float PreviewPadding = 20f;

    [MenuItem("Tools/Image Splitter")]
    public static void Open()
    {
        var window = GetWindow<ImageSplitterWindow>("Image Splitter");
        window.minSize = new Vector2(420, 360);
    }

    private void OnGUI()
    {
        Rect all = new Rect(0, 0, position.width, position.height);
        sidebarWidth = Mathf.Clamp(sidebarWidth, MinSidebarWidth,
            Mathf.Max(MinSidebarWidth, all.width - MinPreviewWidth));
        Rect sidebar = new Rect(0, 0, sidebarWidth, all.height);
        Rect divider = new Rect(sidebarWidth, 0, DividerWidth, all.height);
        Rect preview = new Rect(divider.xMax, 0,
            Mathf.Max(1, all.width - divider.xMax), all.height);
        HandleSidebarResize(divider);

        EditorGUI.DrawRect(sidebar, new Color(0.20f, 0.20f, 0.20f, 1f));
        EditorGUI.DrawRect(preview, new Color(0.13f, 0.13f, 0.13f, 1f));
        EditorGUI.DrawRect(divider, new Color(0.32f, 0.32f, 0.32f, 1f));
        EditorGUIUtility.AddCursorRect(divider, MouseCursor.ResizeHorizontal);

        DrawSidebar(sidebar);
        HandleDrop(preview);
        HandlePreviewInput(preview);
        DrawPreview(preview);
    }

    private void HandleSidebarResize(Rect divider)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 &&
            divider.Contains(e.mousePosition))
        {
            resizingSidebar = true;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && resizingSidebar)
        {
            sidebarWidth = Mathf.Clamp(e.mousePosition.x, MinSidebarWidth,
                Mathf.Max(MinSidebarWidth, position.width - MinPreviewWidth));
            e.Use();
            Repaint();
        }
        else if (e.type == EventType.MouseUp && resizingSidebar)
        {
            resizingSidebar = false;
            e.Use();
        }
    }

    private void DrawSidebar(Rect area)
    {
        GUILayout.BeginArea(new Rect(area.x + 12, area.y + 12, area.width - 24, area.height - 24));
        GUILayout.Label("Image Splitter", EditorStyles.boldLabel);
        GUILayout.Space(14);

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        var next = (Texture2D)EditorGUILayout.ObjectField("画像", sourceTexture, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
        {
            sourceTexture = next;
            ResetView();
        }

        EditorGUI.BeginDisabledGroup(sourceTexture == null);

        if (GUILayout.Button("x", GUILayout.Width(25)))
        {
            ClearSourceTexture();
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
        columns = Mathf.Max(1, EditorGUILayout.IntField("横の分割数", columns));
        rows = Mathf.Max(1, EditorGUILayout.IntField("縦の分割数", rows));

        if (sourceTexture != null)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("画像サイズ");
            EditorGUILayout.SelectableLabel(
                $"{sourceTexture.width} × {sourceTexture.height}",
                EditorStyles.label, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            long count = (long)columns * rows;
            EditorGUILayout.LabelField("出力枚数", count.ToString());
            if (columns > sourceTexture.width || rows > sourceTexture.height)
                EditorGUILayout.HelpBox("分割数が画像のピクセル数を超えています。", MessageType.Warning);
        }

        GUILayout.Space(12);
        if (GUILayout.Button("表示をリセット"))
            ResetView();

        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(sourceTexture == null ||
            columns > (sourceTexture != null ? sourceTexture.width : 0) ||
            rows > (sourceTexture != null ? sourceTexture.height : 0) ||
            (long)columns * rows > 10000);
        if (GUILayout.Button("分割して保存", GUILayout.Height(38)))
            SaveTiles();
        EditorGUI.EndDisabledGroup();
        GUILayout.EndArea();
    }

    private void ResetView()
    {
        zoom = 1f;
        pan = Vector2.zero;
        Repaint();
    }

    private Rect GetImageRect(Rect preview)
    {
        float w = Mathf.Max(1f, preview.width - PreviewPadding * 2);
        float h = Mathf.Max(1f, preview.height - PreviewPadding * 2);
        float fit = Mathf.Min(w / sourceTexture.width, h / sourceTexture.height);
        Vector2 size = new Vector2(sourceTexture.width, sourceTexture.height) * fit * zoom;
        Vector2 center = preview.center + pan;
        return new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
    }

    private void HandlePreviewInput(Rect preview)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseUp && e.button == 0)
        {
            panning = false;
            return;
        }

        if (!preview.Contains(e.mousePosition))
            return;

        if (sourceTexture != null && e.type == EventType.ScrollWheel)
        {
            Rect before = GetImageRect(preview);
            Vector2 mouse = e.mousePosition;
            Vector2 relative = new Vector2(
                (mouse.x - before.x) / before.width,
                (mouse.y - before.y) / before.height);
            zoom = Mathf.Clamp(zoom * Mathf.Pow(1.1f, -e.delta.y), 0.1f, 16f);
            Rect after = GetImageRect(preview);
            pan += mouse - new Vector2(
                after.x + relative.x * after.width,
                after.y + relative.y * after.height);
            e.Use();
            Repaint();
        }
        else if (sourceTexture != null && e.type == EventType.MouseDown && e.button == 0)
        {
            panning = true;
            lastMouse = e.mousePosition;
            e.Use();
        }
        else if (panning && e.type == EventType.MouseDrag && e.button == 0)
        {
            pan += e.mousePosition - lastMouse;
            lastMouse = e.mousePosition;
            e.Use();
            Repaint();
        }
    }

    private void ClearSourceTexture()
    {
        sourceTexture = null;

        zoom = 1.0f;

        pan = Vector2.zero;

        panning = false;

        Repaint();
    }

    private void HandleDrop(Rect preview)
    {
        Event e = Event.current;
        if (!preview.Contains(e.mousePosition))
            return;
        if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform)
            return;

        Texture2D dropped = null;
        foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
        {
            if (obj is Texture2D tex)
            {
                dropped = tex;
                break;
            }
        }
        if (dropped == null)
            return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (e.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            sourceTexture = dropped;
            ResetView();
        }
        e.Use();
    }

    private void DrawPreview(Rect preview)
    {
        if (sourceTexture == null)
        {
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            };
            GUI.Label(preview, "ここに画像をドラッグ＆ドロップ", style);
            return;
        }

        Rect imageRect = GetImageRect(preview);
        GUI.BeginGroup(preview);
        Rect local = new Rect(imageRect.x - preview.x, imageRect.y - preview.y,
            imageRect.width, imageRect.height);
        GUI.DrawTexture(local, sourceTexture, ScaleMode.StretchToFill, true);

        Handles.BeginGUI();
        Handles.color = new Color(1, 1, 1, 0.9f);
        for (int x = 1; x < columns && x < 10000; x++)
        {
            float xx = local.x + local.width * x / columns;
            Handles.DrawLine(new Vector3(xx, local.y), new Vector3(xx, local.yMax));
        }
        for (int y = 1; y < rows && y < 10000; y++)
        {
            float yy = local.y + local.height * y / rows;
            Handles.DrawLine(new Vector3(local.x, yy), new Vector3(local.xMax, yy));
        }
        Handles.EndGUI();
        GUI.EndGroup();
    }

    private void SaveTiles()
    {
        string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
        if (string.IsNullOrEmpty(assetPath))
        {
            EditorUtility.DisplayDialog("エラー", "Project内のPNG/JPG画像を選択してください。", "OK");
            return;
        }

        string extension = Path.GetExtension(assetPath).ToLowerInvariant();
        if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
        {
            EditorUtility.DisplayDialog("非対応形式", "この版の保存処理はPNG/JPGに対応しています。", "OK");
            return;
        }

        string folder = EditorUtility.OpenFolderPanel("保存先フォルダを選択", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder))
            return;

        Texture2D readable = null;
        try
        {
            byte[] input = File.ReadAllBytes(Path.GetFullPath(assetPath));
            readable = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(readable, input))
                throw new Exception("画像の読み込みに失敗しました。");

            int width = readable.width;
            int height = readable.height;
            if (columns > width || rows > height)
                throw new Exception("分割数が画像サイズを超えています。");

            string baseName = Path.GetFileNameWithoutExtension(assetPath);
            for (int y = 0; y < rows; y++)
            {
                int top = (int)((long)height * y / rows);
                int bottom = (int)((long)height * (y + 1) / rows);
                int tileH = bottom - top;
                for (int x = 0; x < columns; x++)
                {
                    int left = (int)((long)width * x / columns);
                    int right = (int)((long)width * (x + 1) / columns);
                    int tileW = right - left;
                    Texture2D tile = null;
                    try
                    {
                        tile = new Texture2D(tileW, tileH, TextureFormat.RGBA32, false);
                        tile.SetPixels(readable.GetPixels(left, height - bottom, tileW, tileH));
                        tile.Apply();
                        string filename = $"{baseName}_r{y + 1:D2}_c{x + 1:D2}.png";
                        File.WriteAllBytes(Path.Combine(folder, filename), tile.EncodeToPNG());
                    }
                    finally
                    {
                        if (tile != null) DestroyImmediate(tile);
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("保存完了", $"{(long)columns * rows}枚を保存しました。\n{folder}", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("保存エラー", ex.Message, "OK");
        }
        finally
        {
            if (readable != null) DestroyImmediate(readable);
        }
    }
}

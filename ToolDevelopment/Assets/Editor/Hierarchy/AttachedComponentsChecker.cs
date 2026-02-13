#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class AttachedComponentsChecker
{
    private static Color red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    private static Color orange = new Color(1.0f, 0.5f, 0.0f, 1.0f);        // AudioSource
    private static Color yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);        // Light
    private static Color yellowGreen = new Color(0.5f, 1.0f, 0.2f, 1.0f);   // Physics
    private static Color green = new Color(0.2f, 1.0f, 0.3f, 1.0f);         
    private static Color skyBlue = new Color(0.2f, 0.8f, 1.0f, 1.0f);       // Canvas
    private static Color royalBlue = new Color(0.3f, 0.5f, 0.9f, 1.0f);     // Camera
    private static Color purple = new Color(0.7f, 0.3f, 0.9f, 1.0f);        // ParticleSystem
    private static Color pink = new Color(1.0f, 0.4f, 0.7f, 1.0f);          // Animator
    private static Color gray = new Color(0.6f, 0.6f, 0.6f, 1.0f);          // DontDestroyOnLoad

    static AttachedComponentsChecker()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null) return;

        var markers = GetMarkers(gameObject);
        DrawMarkers(selectionRect, markers);
    }

    /// <summary>
    /// コンポーネント毎の色を設定
    /// </summary>
    private static List<HierarchyMarker> GetMarkers(GameObject go)
    {
        var list = new List<HierarchyMarker>();

        // Canvas
        if (go.GetComponent<Canvas>())
        {
            list.Add(new HierarchyMarker
            {
                color = skyBlue,
                isActive = true
            });
        }

        // Camera
        if (go.GetComponent<Camera>())
        {
            list.Add(new HierarchyMarker
            {
                color = royalBlue,
                isActive = true
            });
        }

        // Light
        if (go.GetComponent<Light>())
        {
            list.Add(new HierarchyMarker
            {
                color = yellow,
                isActive = true
            });
        }

        // AudioSource
        if (go.GetComponent<AudioSource>())
        {
            list.Add(new HierarchyMarker
            {
                color = orange,
                isActive = true
            });
        }

        // Rigidbody or Collider
        if (go.GetComponent<Rigidbody>() || go.GetComponent<Collider>())
        {
            list.Add(new HierarchyMarker
            {
                color = yellowGreen,
                isActive = true
            });
        }

        // ParticleSystem
        if (go.GetComponent<ParticleSystem>())
        {
            list.Add(new HierarchyMarker
            {
                color = purple,
                isActive = true
            });
        }

        // Animator
        if (go.GetComponent<Animator>() || go.GetComponent<Animation>())
        {
            list.Add(new HierarchyMarker
            {
                color = pink,
                isActive = true
            });
        }

        // Don't Destroy On Load
        if (go.scene.name == "DontDestroyOnLoad")
        {
            list.Add(new HierarchyMarker
            {
                color = gray,
                isActive = true
            });
        }

        return list;
    }

    /// <summary>
    /// 描画処理
    /// </summary>
    private static void DrawMarkers(Rect selectionRect, List<HierarchyMarker> markers)
    {
        if (markers.Count == 0) return;

        const float markerWidth = 3f;
        const float spacing = 2f;

        float x = selectionRect.xMax - spacing;

        foreach (var marker in markers)
        {
            x -= markerWidth;

            var rect = new Rect(
                x,
                selectionRect.y + 1,
                markerWidth,
                selectionRect.height - 2
            );

            EditorGUI.DrawRect(rect, marker.color);

            x -= spacing;
        }
    }
}

struct HierarchyMarker
{
    public Color color;
    public bool isActive;
}


#endif
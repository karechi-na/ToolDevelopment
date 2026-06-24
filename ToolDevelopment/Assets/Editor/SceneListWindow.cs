#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class SceneListWindow : EditorWindow
{
    private Vector2 scroll;
    private List<string> scenePaths = new List<string>();

    private List<EditorBuildSettingsScene> buildScenes
        = new List<EditorBuildSettingsScene>();

    private string selectedScenePath = "";

    [MenuItem("Tools/ Scene List", false, 1)]
    private static void Open()
    {
        GetWindow<SceneListWindow>("Scene List");
    }

    private void OnEnable()
    {
        RefreshSceneList();
    }

    private void RefreshSceneList()
    {
        buildScenes.Clear();
        buildScenes.AddRange(EditorBuildSettings.scenes);

        //scenePaths.Clear();

        //foreach (var scene in EditorBuildSettings.scenes) 
        //{
        //    scenePaths.Add(scene.path);
        //}
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            RefreshSceneList();
        }

        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var scene in buildScenes)
        {
            string path = scene.path;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

            Rect rect = GUILayoutUtility.GetRect(
                new GUIContent(sceneName),
                EditorStyles.label
            );

            // --- 背景ハイライト ---
            if (path == selectedScenePath)
            {
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.90f, 0.35f));
            }

            // --- 無効シーンはグレーアウト ---
            Color prevColor = GUI.color;
            if (!scene.enabled)
            {
                GUI.color = Color.gray;
            }
            EditorGUI.LabelField(rect, sceneName);

            GUI.color = prevColor;

            // --- クリック判定 ---
            if (Event.current.type == EventType.MouseDown &&
                rect.Contains(Event.current.mousePosition))
            {
                selectedScenePath = path;

                if (Event.current.clickCount == 1)
                {
                    PingScene(path);
                }
                else if (Event.current.clickCount == 2)
                {
                    OpenScene(path);
                }

                Event.current.Use();
            }

        }

        EditorGUILayout.EndScrollView();
    }

    private void PingScene(string path)
    {
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        if (scene != null)
        {
            EditorGUIUtility.PingObject(scene);
            Selection.activeObject = scene;
        }
    }

    private void OpenScene(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
        }
    }
}
#endif
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class SceneLauncherWindow : EditorWindow
{
    private Vector2 scrollPosition;

    private List<SceneInfo> sceneInfos = new();

    private SceneLauncherSettings settings;
    private SerializedObject serializedSettings;

    private bool needsRefresh;

    [MenuItem("Tools/Scene Launcher")]
    private static void Open()
    {
        GetWindow<SceneLauncherWindow>("Scene Launcher");
    }

    private void OnEnable()
    {
        settings = SceneLauncherSettings.GetOrCreate();
        serializedSettings = new SerializedObject(settings);

        RefreshScenes();
    }

    private void OnGUI()
    {
        DrawSettings();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Refresh"))
        {
            RefreshScenes();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (SceneInfo sceneInfo in sceneInfos)
        {
            DrawScene(sceneInfo);
        }

        EditorGUILayout.EndScrollView();

        if (needsRefresh)
        {
            needsRefresh = false;
            RefreshScenes();
            Repaint();
        }
    }

    private void DrawSettings()
    {
        serializedSettings.Update();

        EditorGUILayout.LabelField("PlaySettings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            serializedSettings.FindProperty("bootScene"),
            new GUIContent("ゲーム開始のシーン")
        );

        EditorGUILayout.PropertyField(
            serializedSettings.FindProperty("playFromBootScene"),
            new GUIContent("BootSceneから始めるか")
        );

        serializedSettings.ApplyModifiedProperties();
    }

    private void DrawScene(SceneInfo sceneInfo)
    {
        Color originalColor = GUI.backgroundColor;

        GUI.backgroundColor = sceneInfo.IsInBuildSettings
            ? new Color(0.5f, 1.0f, 0.5f)
            : new Color(1.0f, 0.5f, 0.5f);

        EditorGUILayout.BeginVertical("box");

        GUI.backgroundColor = originalColor;

        EditorGUILayout.BeginHorizontal();

        string status = sceneInfo.IsInBuildSettings ? "[OK]" : "[NG]";

        EditorGUILayout.LabelField($"{status} {sceneInfo.SceneName}");

        if (GUILayout.Button("Open", GUILayout.Width(80)))
        {
            OpenScene(sceneInfo.ScenePath);
        }

        if (sceneInfo.IsInBuildSettings)
        {
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                RemoveSceneFromBuildSettings(sceneInfo.ScenePath);
                needsRefresh = true;
            }
        }
        else
        {
            if (GUILayout.Button("Add", GUILayout.Width(80)))
            {
                AddSceneToBuildSettings(sceneInfo.ScenePath);
                needsRefresh = true;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(sceneInfo.ScenePath, EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(2);
    }

    private void AddSceneToBuildSettings(string scenePath)
    {
        bool add = EditorUtility.DisplayDialog(
            "Build Settingsに追加",
            $"{scenePath}\n\nこのシーンをBuild Settingsに追加しますか？",
            "追加する",
            "キャンセル"
        );

        if (!add) return;

        List<EditorBuildSettingsScene> scenes = 
            EditorBuildSettings.scenes.ToList();

        if (scenes.Any(scene => scene.path == scenePath)) return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
    }

    private void RemoveSceneFromBuildSettings(string scenePath)
    {
        bool remove = EditorUtility.DisplayDialog(
            "Build Settingsから削除",
            $"{scenePath}\n\nこのシーンをBuild Settingsから削除しますか？",
            "削除する",
            "キャンセル"
        );

        if (!remove) return;

        List<EditorBuildSettingsScene> scenes = 
            EditorBuildSettings.scenes.ToList();

        scenes.RemoveAll(scene => scene.path == scenePath);

        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
    }

    private void RefreshScenes()
    {
        sceneInfos.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

        HashSet<string> buildScenePaths = EditorBuildSettings.scenes
            .Select(scene => scene.path)
            .ToHashSet();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            sceneInfos.Add(new SceneInfo
            {
                SceneName = System.IO.Path.GetFileNameWithoutExtension(path),
                ScenePath = path,
                IsInBuildSettings = buildScenePaths.Contains(path)
            });
        }

        sceneInfos = sceneInfos
            .OrderBy(info => info.SceneName)
            .ToList();
    }

    private void OpenScene(string path)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.OpenScene(path);
    }

    private class SceneInfo
    {
        public string SceneName;
        public string ScenePath;
        public bool IsInBuildSettings;
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneLauncherPlayHandler
{
    static SceneLauncherPlayHandler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;

        SceneLauncherSettings settings = SceneLauncherSettings.GetOrCreate();

        if(!settings.PlayFromBootScene) return;

        if(string.IsNullOrEmpty(settings.BootScenePath)) return;

        Scene currentScene = SceneManager.GetActiveScene();

        if(currentScene.path == settings.BootScenePath) return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorSceneManager.OpenScene(settings.BootScenePath);
    }
}

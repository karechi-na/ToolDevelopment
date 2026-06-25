using UnityEditor;
using UnityEngine;

public class SceneLauncherSettings : ScriptableObject
{
    public const string ASSET_PATH =
        "Assets/Editor/ProjectTools/SceneLauncherSettings.asset";

    [SerializeField] private SceneAsset bootScene;
    
    [SerializeField] private bool playFromBootScene = true;
    public SceneAsset BootScene => bootScene;
    public bool PlayFromBootScene => playFromBootScene;

    public string BootScenePath
    {
        get
        {
            if(bootScene == null) return string.Empty;

            return AssetDatabase.GetAssetPath(bootScene);
        }
    }

    public static SceneLauncherSettings GetOrCreate()
    {
        SceneLauncherSettings settings =
            AssetDatabase.LoadAssetAtPath<SceneLauncherSettings>(ASSET_PATH);

        if(settings != null) return settings;

        settings = CreateInstance<SceneLauncherSettings>();

        string folderPath = System.IO.Path.GetDirectoryName(ASSET_PATH);

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateAsset(settings, ASSET_PATH);
        AssetDatabase.SaveAssets();

        return settings;
    }
}

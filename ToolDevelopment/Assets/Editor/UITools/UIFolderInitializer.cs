using UnityEditor;

/// <summary>
/// UI用フォルダがない場合に生成するクラス
/// </summary>
[InitializeOnLoad]
public static class UIFolderInitializer
{
    private const string ROOT_FOLDER = "Assets";

    private const string UI_FOLDER_NAME = "UI";

    private const string ICON_FOLDER_NAME = "Icon";
    private const string BUTTON_FOLDER_NAME = "Button";
    private const string BACKGROUND_FOLDER_NAME = "BackGround";

    //フォルダの有無を確認、ない場合作成
    static UIFolderInitializer()
    {
        CreateFolderIfNotExists(ROOT_FOLDER, UI_FOLDER_NAME);

        CreateFolderIfNotExists($"{ROOT_FOLDER}/{UI_FOLDER_NAME}", ICON_FOLDER_NAME);
        CreateFolderIfNotExists($"{ROOT_FOLDER}/{UI_FOLDER_NAME}", BUTTON_FOLDER_NAME);
        CreateFolderIfNotExists($"{ROOT_FOLDER}/{UI_FOLDER_NAME}", BACKGROUND_FOLDER_NAME);
    }

    /// <summary>
    /// フォルダ生成処理を行うメソッド
    /// </summary>
    private static void CreateFolderIfNotExists(string parent, string folderName)
    {
        string path = $"{parent}/{folderName}";

        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}

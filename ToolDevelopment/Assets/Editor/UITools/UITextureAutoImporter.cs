using UnityEditor;

/* 
AssetPostprocessor <- アセットがUnityにインポートされるときに割り込むためのクラス
画像等がプロジェクトに入った時にインポート前に設定を変える、インポート後に処理する等できる
 */
/// <summary>
/// Assets/UIフォルダに画像をインポートしたときに自動でUI用の設定に変更するツール
/// </summary>
public class UITextureAutoImporter : AssetPostprocessor
{
    // 対象のフォルダ
    private const string UI_FOLDER = "Assets/UI";

    private const string ICON_FOLDER = "Assets/UI/Icon/";
    private const string BUTTON_FOLDER = "Assets/UI/Button/";
    private const string BACKGROUND_FOLDER = "Assets/UI/BackGround/";

    /*
    Texture画像をインポートする直前に自動で呼ばれるUnity公式の用意しているメソッド
     */
    /// <summary>
    /// ここで画像の設定を切り替える
    /// </summary>
    private void OnPreprocessTexture()
    {
        /* 
        assetPathはAssetPostprocessorが持ってる変数
        インポートされようとしているアセットのパスが入っている
         */
        // ここで画像のパスが対象フォルダと一緒かを見る
        if (!assetPath.StartsWith(UI_FOLDER)) return;

        /*
        assetImporterもAssetPostprocessorが持つ変数
        今インポートしているアセットのImporterが入っている
        今回はOnPreprocessTexture()内なので対象はTexture
         */
        // assetImporterをTextureImporter型にキャスト
        TextureImporter importer = (TextureImporter)assetImporter;

        ApplyBaseUISettings(importer);

        // 各フォルダに合わせて画像サイズを変更
        if (assetPath.StartsWith(ICON_FOLDER))
        {
            importer.maxTextureSize = 512;
        }
        else if (assetPath.StartsWith(BUTTON_FOLDER))
        {
            importer.maxTextureSize = 1024;
        }
        else if (assetPath.StartsWith(BACKGROUND_FOLDER))
        {
            importer.maxTextureSize = 2048;
        }
        else
        {
            importer.maxTextureSize = 2048;
        }
    }

    private void ApplyBaseUISettings(TextureImporter importer)
    {
        // Texture Type : Sprite (2D and UI)に変換
        importer.textureType = TextureImporterType.Sprite;
        // SpriteModeをSingleに設定
        importer.spriteImportMode = SpriteImportMode.Single;
        // UIでは不要なためMipMapを無効化
        importer.mipmapEnabled = false;
        // PNGの透明部分を適切に扱うようにする
        importer.alphaIsTransparency = true;
        // 拡大縮小時に滑らかに補間する
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
        // 圧縮
        importer.textureCompression = TextureImporterCompression.Compressed;
    }
}

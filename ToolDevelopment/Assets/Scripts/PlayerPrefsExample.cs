using UnityEngine;

public class PlayerPrefsExample : MonoBehaviour
{
    // 保存できるデータの型　int, float, string
    // 1行で保存、読み込みできる
    // 各プラットフォームに適した場所に自動保存
    // ユーザーが保存場所を見つけられてしまう


    // 基本的な使い方
    private void Start()
    {
        // int を保存
        PlayerPrefs.SetInt("HighScore", 1000);

        int score = PlayerPrefs.GetInt("HighScore");
        Debug.Log(score);
    }
}

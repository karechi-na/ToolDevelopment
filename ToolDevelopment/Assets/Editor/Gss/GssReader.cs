#if UNITY_EDITOR
//==============================================================================
// Google Spreadsheet(GSS)をCSV形式で取得し、
// UnityプロジェクトのAssets配下にCSVファイルとして保存するエディタツール
//==============================================================================

using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;           // UnityWebRequestを使ってHTTP通信を行うために必要
using System.IO;                        // CSVファイルの保存(Directory / File)に使用
using Unity.EditorCoroutines.Editor;    // Editor上でCoroutineを使うために必要
using System.Collections;

public class GssReader : EditorWindow
{
    // ユーザーが入力するGoogle SpreadsheetのIDとシート名
    private string sheetId;
    private string sheetName;

    //[MenuItem("Tools/GSS Reader")]
    private static void Open()
    {
        GetWindow<GssReader>("GSS reader");
    }

    private void OnGUI()
    {
        // テキストラベルの表示
        // EditorStyles.boldLabel 太字スタイルを適用
        GUILayout.Label("Google Spreadsheet Reader", EditorStyles.boldLabel);
        // ※GUILayoutはレイアウト自動調整付きのGUI要素を作成するためのクラス

        GUILayout.Label("入力するデータ", EditorStyles.boldLabel);
        GUILayout.Label("https://docs.google.com/spreadsheets/d/【GSSのID】/edit?gid=0#gid=0", EditorStyles.boldLabel);
        // Editor上で文字列を入力
        sheetId = EditorGUILayout.TextField("Sheet ID", sheetId);
        GUILayout.Label("GSS左下のシートの名前", EditorStyles.boldLabel);
        sheetName = EditorGUILayout.TextField("Sheet Name", sheetName);
        // EditorGUILayoutはEditor専用のGUI要素を作成するためのクラス
        // 入力値を直接変数に格納できる
        // TextFieldは文字列入力フィールドを作成するメソッド
        // 第一引数はラベル、第二引数は初期値、戻り値は入力された文字列

        // 押されたフレームでtrueを返し処理を実行
        if (GUILayout.Button("Download CSV"))
        {
            // Editor上でCoroutineを開始
            EditorCoroutineUtility.StartCoroutine(
                DownloadCsv(), this
            );
        }
    }

    /// <summary>
    /// GSSをCSV形式でダウンロードするCoroutine
    /// </summary>
    private IEnumerator DownloadCsv()
    {
        string url =
            $"https://docs.google.com/spreadsheets/d/{sheetId}/gviz/tq?tqx=out:csv&sheet={sheetName}";
        // GSSのCSV取得URL
        // tqx=out:csv　→　CSV形式で出力する指定
        // sheet=シート名　→　取得するシート名を指定

        // 指定したURLにHTTP GETリクエストを送信
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // リクエストの送信と完了まで待機
            yield return request.SendWebRequest();

            // エラーハンドリング
            // 通信エラー時はログを出して処理を中断
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                yield break;
            }

            // ダウンロードしたCSV文字列を保存処理に渡す
            SaveCsv(request.downloadHandler.text);
        }
    }

    /// <summary>
    /// CSVをAssets配下に保存する
    /// </summary>
    private void SaveCsv(string csvText)
    {
        // 保存先フォルダパス
        string folderPath = "Assets/MasterData/CSV";

        // フォルダが存在しない場合は作成
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // シート名をファイル名にして保存
        string filePath = $"{folderPath}/{sheetName}.csv";
        File.WriteAllText(filePath, csvText);

        // Unityエディタに変更を反映
        AssetDatabase.Refresh();
        Debug.Log($"CSV saved : {filePath}");
    }
}
#endif
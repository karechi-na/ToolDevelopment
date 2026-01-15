#if UNITY_EDITOR

using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace App.ReadCsv
{
    /// <summary>
    /// Google SpreadSheetを読み込み、CSVを生成するエディタツール
    /// </summary>
    public class GssReaderTool : EditorWindow
    {
        // CSVの保存先フォルダ
        private const string CSV_FOLDER_PATH = "Assets/MasterData/CSV/";

        // GSSをCSV形式で取得するための指定
        private const string GSS_FORMAT = "tqx=out:csv";

        // 入力用変数
        private string gssSheetId;      // スプレッドシートID
        private string gssSheetName;    // シート名
        private string csvfileName;     // 出力するCSV名

        // ログ表示用
        private string log;
        private bool isLoadingGss;

        /// <summary>
        /// メニューに表示
        /// </summary>
        [MenuItem("Tools/GSS Reader Tool")]
        public static void OpenWindow() 
        {
            GetWindow<GssReaderTool>("GSS読み込みツール");
        }

        /// <summary>
        /// ウィンドウUI描画
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("[GSSの読み込み,CSVファイルの作成]");

            // 入力欄
            gssSheetId = EditorGUILayout.TextField("・ GSSのシートID :", gssSheetId);
            gssSheetName = EditorGUILayout.TextField("・ GSSのシート名 :", gssSheetName);
            csvfileName = EditorGUILayout.TextField("・ 作成するファイル名 :", csvfileName);

            // 実行ボタン
            if (GUILayout.Button("実行"))
            {
                log = string.Empty;
                StartLoadGss(gssSheetId, gssSheetName);
            }
            DrawLogGUI();
        }

        /// <summary>
        /// GSSの読み込み開始
        /// </summary>
        public void StartLoadGss(string id, string name)
        {
            // 二重実行防止
            if (isLoadingGss)
            {
                AddLog("Error : GSSを読み込み中...");
                return;
            }
            // 入力チェック
            else if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(csvfileName))
            {
                AddLog("Error : 設定項目を全て記入してください");
                return;
            }

            // GSSのCSV取得URL
            var url = "https://docs.google.com/spreadsheets/d/" + id + "/gviz/tq?" + GSS_FORMAT + "&sheet=" + name;
            
            // EditorCoroutineで非同期処理
            EditorCoroutineUtility.StartCoroutine(LoadGss(url), this);
        }

        /// <summary>
        /// ログ追加
        /// </summary>
        private void AddLog(string message)
        {
            log += $" - {message}\n";
        }

        /// <summary>
        /// GSSデータ取得処理
        /// </summary>
        private IEnumerator LoadGss(string url)
        {
            // GSSデータの取得
            using (var request = UnityWebRequest.Get(url))
            {
                AddLog("GSSの読み込み中");

                isLoadingGss = true;
                yield return request.SendWebRequest();
                isLoadingGss = false;

                // エラーチェック
                if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    AddLog($"Error Load GSS : {request.error} (ネットワークに接続されているか確認して下さい)");
                    yield break;
                }
                AddLog("GSSの読み込み完了");

                // CSV作成
                CreateCsv(csvfileName, request.downloadHandler.text);
            }
        }

        /// <summary>
        /// CSVファイルの作成
        /// </summary>
        private void CreateCsv(string fileName, string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                AddLog("Error Create CSV : データが空であるため、ファイルを作成できません");
                return;
            }

            AddLog("CSVファイルの作成開始");

            // フォルダがなければ作成
            if (!Directory.Exists(CSV_FOLDER_PATH))
            {
                Directory.CreateDirectory(CSV_FOLDER_PATH);
            }

            var path = CSV_FOLDER_PATH + fileName + ".csv";
            using (var sw = new StreamWriter(path, false))
            {
                try
                {
                    sw.Write(data);

                    // Unityに反映
                    AssetDatabase.Refresh();
                    AddLog($"CSVファイルの作成完了 : {path}");
                }
                catch (System.Exception e)
                {
                    AddLog(e.ToString());
                }
            }
        }

        /// <summary>
        /// ログを出力
        /// </summary>
        private void DrawLogGUI()
        {
            if (string.IsNullOrEmpty(log)) return;

            GUILayout.FlexibleSpace();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));

            GUILayout.Label(" [Log] ");
            GUILayout.Label(log, new GUIStyle() { wordWrap = true, normal = new GUIStyleState() { textColor = new Color(1, 1, 1, 1) } });

            if (GUILayout.Button("Clear Log"))
            {
                log = string.Empty;
            }
            GUILayout.Space(7);
        }
    }

}
#endif
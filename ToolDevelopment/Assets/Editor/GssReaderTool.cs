#if UNITY_EDITOR

using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace App.ReadCsv
{
    public class GssReaderTool : EditorWindow
    {
        private const string CSV_FOLDER_PATH = "Assets/MasterData/CSV/";
        private const string GSS_FORMAT = "tqx=out:csv";

        private string gssSheetId;
        private string gssSheetName;
        private string csvfileName;
        private string log;
        private bool isLoadingGss;

        [MenuItem("Tools/GSS Reader Tool")]
        public static void OpenWindow() 
        {
            GetWindow<GssReaderTool>("GSS読み込みツール");
        }

        private void OnGUI()
        {
            GUILayout.Label("[GSSの読み込み,CSVファイルの作成]");

            gssSheetId = EditorGUILayout.TextField("・ GSSのシートID :", gssSheetId);
            gssSheetName = EditorGUILayout.TextField("・ GSSのシート名 :", gssSheetName);
            csvfileName = EditorGUILayout.TextField("・ 作成するファイル名 :", csvfileName);

            if (GUILayout.Button("実行"))
            {
                log = string.Empty;
                StartLoadGss(gssSheetId, gssSheetName);
            }
            DrawLogGUI();
        }

        /// <summary>
        /// GSSの読み込み
        /// </summary>
        public void StartLoadGss(string id, string name)
        {
            if (isLoadingGss)
            {
                AddLog("Error : GSSを読み込み中...");
                return;
            }
            else if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(csvfileName))
            {
                AddLog("Error : 設定項目を全て記入してください");
                return;
            }

            var url = "https://docs.google.com/spreadsheets/d/" + id + "/gviz/tq?" + GSS_FORMAT + "&sheet=" + name;
            EditorCoroutineUtility.StartCoroutine(LoadGss(url), this);
        }

        private void AddLog(string message)
        {
            log += $" - {message}\n";
        }

        private IEnumerator LoadGss(string url)
        {
            // GSSデータの取得
            using (var request = UnityWebRequest.Get(url))
            {
                AddLog("GSSの読み込み中");

                isLoadingGss = true;
                yield return request.SendWebRequest();
                isLoadingGss = false;

                if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    AddLog($"Error Load GSS : {request.error} (ネットワークに接続されているか確認して下さい)");
                    yield break;
                }
                AddLog("GSSの読み込み完了");

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
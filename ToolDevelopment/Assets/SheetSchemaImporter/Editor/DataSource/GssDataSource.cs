using System;
using UnityEngine.Networking;

namespace Karechina.SchemaImporter
{

    /// <summary>
    /// GoogleSpreadsheetのデータを読み取るクラス
    /// </summary>
    public class GssDataSource : IDataSource
    {
        // シートのID 
        public readonly string sheetId;
        // シートのGID
        public readonly string sheetGid;

        // IDとGIDを受け取る
        public GssDataSource(string sheetId, string sheetGid)
        {
            this.sheetId = sheetId;
            this.sheetGid = sheetGid;
        }

        /// <summary>
        /// GSSのデータをUnityWebRequestで受け取り
        /// </summary>
        public string GetCsvText()
        {
            if (string.IsNullOrWhiteSpace(sheetId))
                throw new Exception("Sheet ID が入力されていないよ");

            if (string.IsNullOrWhiteSpace(sheetGid))
                throw new Exception("Sheet GID が入力されていないよ");

            string url =
                $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={sheetGid}";

            // GSSにGET通信を始める
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SendWebRequest();

            // 通信完了まで待機
            while (!request.isDone) { }

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"GSS取得に失敗したよ：{request.error}");

            // 取得したデータを返す
            return request.downloadHandler.text;
        }
    }
}
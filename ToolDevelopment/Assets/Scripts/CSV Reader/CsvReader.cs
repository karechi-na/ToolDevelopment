using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace App.ReadCsv
{
    /// <summary>
    /// CSVファイルを読み込み、ID検索できるクラス
    /// </summary>
    public class CsvReader : MonoBehaviour
    {
        // Inspectorで指定するCSV(TextAsset)
        [SerializeField] private TextAsset csv;

        // 読み込んだCSVデータ
        // 1行 = string[]
        public List<string[]> ReadDatas { get; private set; } = new List<string[]>();

        /// <summary>
        /// ID(1列目)を用いて、データを取得する
        /// </summary>
        public string[] FindDataWithId(string id)
        {
            // IDが一致する行を取得
            var lineData = ReadDatas.FirstOrDefault(e => e[0] == id);

            // 見つからなければ null 相当
            if (lineData == default) return lineData;

            // ID列を除いたデータを返す
            var lineDataExceptId = new string[lineData.Length - 1];
            for (var i = 0; i < lineData.Length - 1; i++)
            {
                lineDataExceptId[i] = lineData[i + 1];
            }
            return lineDataExceptId;
        }
#if UNITY_EDITOR
        /// <summary>
        /// Inspectorの値が変更されたときに呼ばれる
        /// </summary>
        private void OnValidate()
        {
            if (!csv) return;

            // CSVかどうか確認
            var path = AssetDatabase.GetAssetPath(csv);
            if (!path.Contains(".csv"))
            {
                csv = null;
                Debug.LogError("CSVファイルを指定してください");
                return;
            }

            // CSV読み込み
            ReadDatas = Read(csv.text);
        }

        /// <summary>
        /// データの読み込み(CSVテキストをList<string[]>に変換)
        /// </summary>
        private static List<string[]> Read(string text)
        {
            var readDatas = new List<string[]>();
            if(string.IsNullOrEmpty(text))return readDatas;

            var reader = new StringReader(text);

            // 1行ずつ読み込み
            while (reader.Peek() != -1)
            {
                var line = reader.ReadLine();
                readDatas.Add(line.Split(","));
            }

            // 読み込み確認用ログ
            Debug.Log($"Read CSV :\n {string.Join("\n", readDatas.Select(e => string.Join(",", e)))}");

            return readDatas;
        }
#endif
    }
#if UNITY_EDITOR
    /// <summary>
    /// CsvReader専用Inspector拡張
    /// </summary>
    [CustomEditor(typeof(CsvReader))]
    public class CsvReaderEditor : Editor
    {
        private string[] data;
        private string id;
        private bool isFoldout;

        public override void OnInspectorGUI()
        {
            // 通常のInspector
            base.OnInspectorGUI();

            //折りたたみUI
            isFoldout = EditorGUILayout.Foldout(isFoldout, "データ確認用");
            if (isFoldout)
            {
                id = EditorGUILayout.TextField("ID:", id);
                if (GUILayout.Button("データの取得"))
                {
                    var creator = (CsvReader)target;
                    data = creator.FindDataWithId(id);
                }

                if (data != default)
                {
                    EditorGUILayout.LabelField("取得したデータ:", string.Join(",", data));
                }
            }
        }
    }
#endif
}


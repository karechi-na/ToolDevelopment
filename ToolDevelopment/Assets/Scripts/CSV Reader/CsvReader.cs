using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace App.ReadCsv
{
    public class CsvReader : MonoBehaviour
    {
        [SerializeField] private TextAsset csv;

        public List<string[]> ReadDatas { get; private set; } = new List<string[]>();

        /// <summary>
        /// IDを用いて、データを取得する
        /// </summary>
        public string[] FindDataWithId(string id)
        {
            var lineData = ReadDatas.FirstOrDefault(e => e[0] == id);
            if (lineData == default) return lineData;

            var lineDataExceptId = new string[lineData.Length - 1];
            for (var i = 0; i < lineData.Length - 1; i++)
            {
                lineDataExceptId[i] = lineData[i + 1];
            }
            return lineDataExceptId;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!csv) return;

            var path = AssetDatabase.GetAssetPath(csv);
            if (!path.Contains(".csv"))
            {
                csv = null;
                Debug.LogError("CSVファイルを指定してください");
                return;
            }

            ReadDatas = Read(csv.text);
        }

        /// <summary>
        /// データの読み込み
        /// </summary>
        private static List<string[]> Read(string text)
        {
            var readDatas = new List<string[]>();
            if(string.IsNullOrEmpty(text))return readDatas;

            var reader = new StringReader(text);

            while (reader.Peek() != -1)
            {
                var line = reader.ReadLine();
                readDatas.Add(line.Split(","));
            }
            Debug.Log($"Read CSV :\n {string.Join("\n", readDatas.Select(e => string.Join(",", e)))}");

            return readDatas;
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CsvReader))]
    public class CsvReaderEditor : Editor
    {
        private string[] data;
        private string id;
        private bool isFoldout;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

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


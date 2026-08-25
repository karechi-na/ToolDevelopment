using UnityEngine;

namespace Karechina.SchemaImporter
{
    /// <summary>
    /// CSVファイルを変換する際のデータを取り出すためのクラス
    /// </summary>
    public class CsvFileDataSource : IDataSource
    {
        // CSVファイル
        private readonly TextAsset csvFile;

        // CSVファイルを受け取る
        public CsvFileDataSource(TextAsset csvFile)
        {
            this.csvFile = csvFile;
        }

        /// <summary>
        /// CSVファイル内の文字列を取得
        /// </summary>
        public string GetCsvText()
        {
            if (csvFile == null)
                throw new System.Exception("CSVファイルが指定されていないよ。");

            return csvFile.text;
        }
    }
}
using UnityEngine;

public class CsvFileDataSource : IDataSource
{
    private readonly TextAsset csvFile;

    public CsvFileDataSource(TextAsset csvFile)
    {
        this.csvFile = csvFile;
    }

    public string GetCsvText()
    {
        if (csvFile == null)
            throw new System.Exception("CSVファイルが指定されていないよ。");

        return csvFile.text;
    }
}

using UnityEngine;

public class CsvFileDataSource : IDataSource
{
    private readonly TextAsset csv;

    public CsvFileDataSource(TextAsset csv)
    {
        this.csv = csv;
    }

    public string GetCsvText()
    {
        return csv.text;
    }
}

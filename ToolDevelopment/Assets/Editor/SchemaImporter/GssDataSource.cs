using UnityEngine;

public class GssDataSource : IDataSource
{
    public string SheetId;
    public string Gid;

    public string GetCsvText()
    {
        return string.Empty;
    }
}

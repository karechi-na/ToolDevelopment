using System;
using UnityEngine.Networking;

public class GssDataSource : IDataSource
{
    public readonly string sheetId;
    public readonly string sheetGid;

    public GssDataSource(string sheetId, string sheetGid)
    {
        this.sheetId = sheetId;
        this.sheetGid = sheetGid;
    }

    public string GetCsvText()
    {
        if (string.IsNullOrWhiteSpace(sheetId))
            throw new Exception("Sheet ID ‚ª“ü—Í‚³‚ê‚Ä‚¢‚È‚¢‚æ");

        if (string.IsNullOrWhiteSpace(sheetGid))
            throw new Exception("Sheet GID ‚ª“ü—Í‚³‚ê‚Ä‚¢‚È‚¢‚æ");

        string url = 
            $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={sheetGid}";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.SendWebRequest();

        while (!request.isDone)
        {
        }

        if (request.result != UnityWebRequest.Result.Success)
            throw new Exception($"GSSæ“¾‚É¸”s‚µ‚½‚æF{request.error}");

        return request.downloadHandler.text;
    }
}

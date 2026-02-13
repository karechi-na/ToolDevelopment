using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int level;
    public int hp;
    public bool isPoisoned;
}

public class SaveManager : MonoBehaviour
{
    private string savePath = "";

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/saveData.json";
        Debug.Log(savePath);
    }

    private void Start()
    {
        SaveGame();
    }

    private void SaveGame()
    {
        // データを作成
        PlayerData data = new PlayerData();
        data.playerName = "勇者";
        data.level = 10;
        data.hp = 100;
        data.isPoisoned = true;

        // JSON文字列に変換
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        LoadGame();
    }

    private void LoadGame()
    {
        // JSON文字列

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);


            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log(data.playerName);
            Debug.Log(data.level);
            Debug.Log(data.hp);
            Debug.Log(data.isPoisoned);
        }
        else
        {
            Debug.LogError("セーブデータが見つかりませんでした。");
        }

    }
}

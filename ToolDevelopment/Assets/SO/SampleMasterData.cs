//======================================================================================
// CSVファイルをScriptableObjectに変換するときの雛型SO
// 外部で値を受け取るときは、このSOの持つ各型を返すメソッドを呼んで受け取る
// 受け取りたい値のKeyを引数として渡すことで受け取ることができる。
//======================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IntEntry
{
    public string key;
    public int value;
}

[Serializable]
public class FloatEntry
{
    public string key;
    public float value;
}

[Serializable]
public class BoolEntry
{
    public string key;
    public bool value;
}

[Serializable]
public class StringEntry
{
    public string key;
    public string value;
}

[CreateAssetMenu(menuName = "MasterData/Sample")]
public class SampleMasterData : ScriptableObject
{
    public int id;

    public List<IntEntry> intValues = new();
    public List<FloatEntry> floatValues = new();
    public List<BoolEntry> boolValues = new();
    public List<StringEntry> stringValues = new();
    public int GetInt(string key, int defaultValue = 0)
    {
        var entry = intValues.Find(x => x.key == key);
        return entry != null ? entry.value : defaultValue;
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        var entry = floatValues.Find(x => x.key == key);
        return entry != null ? entry.value : defaultValue;
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        var entry = boolValues.Find(x => x.key == key);
        return entry != null ? entry.value : defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        var entry = stringValues.Find(x => x.key == key);
        return entry != null ? entry.value : defaultValue;
    }
}

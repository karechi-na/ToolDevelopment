using System;

public static class TypeUtility
{
    public static object ConvertValue(string raw, FieldType type)
    {
        return type switch
        {
            FieldType.String => raw,
            FieldType.Int => int.Parse(raw),
            FieldType.Float => float.Parse(raw),
            FieldType.Bool => ParseBool(raw),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static bool ParseBool(string raw)
    {
        raw = raw.Trim().ToLower();

        return raw switch
        {
            "true" => true,
            "false" => false,
            "1" => true,
            "0" => false,
            "yes" => true,
            "no" => false,
            "Z" => true,
            "›" => true,
            "~" => false,
            _ => throw new Exception($"bool‚É•ÏŠ·‚Å‚«‚È‚¢’l‚¾‚æ: {raw}")
        };
    }
}

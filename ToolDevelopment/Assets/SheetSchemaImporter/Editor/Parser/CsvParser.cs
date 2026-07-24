using System;

/// <summary>
/// CSV文字列をSchemaDataへ変換するクラス
/// </summary>
public class CsvParser
{
    /// <summary>
    /// CSV文字列を解析し、クラス生成・Asset生成で使うSchemaDataを作成する
    /// </summary>
    /// <param name="csvText">解析するCSV文字列</param>
    /// <param name="className">生成するScriptableObjectクラス名</param>
    /// <returns>解析結果のSchemaData</returns>
    public SchemaData Parse(string csvText, string className)
    {
        // 改行コードの違いに対応しつつ、空行を除外して行ごとに分割
        string[] lines = csvText.Split(
            new[] { "\r\n", "\n", "\r" },
            StringSplitOptions.RemoveEmptyEntries
        );

        if (lines.Length < 2)
            throw new Exception("CSVは最低でも 1行目：変数名、2行目：型 が必要だよ。");

        //1行目をフィールド名、2行目を型情報として扱う
        string[] names = SplitCsvLine(lines[0]);
        string[] types = SplitCsvLine(lines[1]);

        if (names.Length != types.Length)
            throw new Exception("変数名の数と型の数が一致してないよ。");

        SchemaData schema = new()
        {
            ClassName = className,
        };

        // フィールド名と型情報をSchemaDataに登録
        for (int i = 0; i < names.Length; i++)
        {
            schema.Fields.Add(new FieldData
            {
                Name = Clean(names[i]),
                Type = ParseFieldType(Clean(types[i]))
            });
        }

        // 3行目以降を実データとして登録
        for (int row = 2; row < lines.Length; row++)
        {
            string[] values = SplitCsvLine(lines[row]);

            RowData rowData = new();

            // フィールド名をキーにして、各セルの値を保存
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                string fieldName = schema.Fields[i].Name;
                string value = i < values.Length ? Clean(values[i]) : "";

                rowData.Values[fieldName] = value;
            }

            schema.Rows.Add(rowData);
        }
        return schema;
    }

    /// <summary>
    /// CSVの1行をカンマ区切りで分割する
    /// </summary>
    private string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }

    /// <summary>
    /// CSVの値から空白、ダブルクォーテーション、BOMを除去する
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string Clean(string value)
    {
        return value.Trim().Trim('"').Trim('\uFEFF');
    }

    /// <summary>
    /// CSV上の型文字列をFieldTypeへ変換する
    /// </summary>
    private FieldType ParseFieldType(string value)
    {
        return value.ToLower() switch
        {
            "string" => FieldType.String,
            "int" => FieldType.Int,
            "float" => FieldType.Float,
            "bool" => FieldType.Bool,
            _ => throw new Exception($"未対応の型だよ：{value}")
        };
    }
}

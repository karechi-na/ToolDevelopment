using System;

public class CsvParser
{
    public SchemaData Parse(string csvText, string className)
    {
        string[] lines = csvText.Split(
            new[] { "\r\n", "\n", "\r" },
            StringSplitOptions.RemoveEmptyEntries
        );

        if (lines.Length < 2)
            throw new Exception("CSVは最低でも 1行目：変数名、2行目：型 が必要だよ。");

        string[] names = SplitCsvLine(lines[0]);
        string[] types = SplitCsvLine(lines[1]);

        if (names.Length != types.Length)
            throw new Exception("変数名の数と型の数が一致してないよ。");

        SchemaData schema = new() 
        {
            ClassName = className,
        };

        for (int i = 0; i < names.Length; i++)
        {
            schema.Fields.Add(new FieldData
            {
                Name = Clean(names[i]),
                Type = ParseFieldType(Clean(types[i]))
            });
        }

        for (int row = 2; row < lines.Length; row++)
        {
            string[] values = SplitCsvLine(lines[row]);

            RowData rowData = new();

            foreach (string value in values)
            {
                rowData.Values.Add(Clean(value));
            }

            schema.Rows.Add(rowData);
        }
        return schema;
    }

    private string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }

    private string Clean(string value)
    {
        return value.Trim().Trim('"').Trim('\uFEFF');
    }

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

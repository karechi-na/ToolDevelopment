using System;

namespace Karechina.SchemaImporter
{
    public static class TypeUtility
    {
        /// <summary>
        /// CSVから取得した文字列を指定された型へ変換する
        /// </summary>
        /// <param name="raw">変換する文字列</param>
        /// <param name="type">変換先の型</param>
        /// <returns>変換後の型</returns>
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

        /// <summary>
        /// 文字列をbool値へ変換する
        /// </summary>
        /// <param name="raw">変換する文字列</param>
        /// <returns>変換後のbool値</returns>
        private static bool ParseBool(string raw)
        {
            // 空白を除去し、小文字へ統一
            raw = raw.Trim().ToLower();

            return raw switch
            {
                "true" => true,
                "false" => false,
                "1" => true,
                "0" => false,
                "yes" => true,
                "no" => false,
                "〇" => true,
                "○" => true,
                "×" => false,
                _ => throw new Exception($"boolに変換できない値だよ: {raw}")
            };
        }
    }
}
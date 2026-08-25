using System.Collections.Generic;

namespace Karechina.SchemaImporter
{
    public class SchemaData
    {
        // クラス名
        public string ClassName;
        //各フィールドに変換する情報を持ったクラスのリスト
        public List<FieldData> Fields = new();
        // 各フィールドのリスト
        public List<RowData> Rows = new();
    }
}
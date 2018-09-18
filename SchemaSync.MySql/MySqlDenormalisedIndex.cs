using SchemaSync.Library.Models;

namespace SchemaSync.MySql
{
    class MySqlDenormalisedIndex
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public IndexType IndexType { get; set; }
        public string ColumnName { get; set; }
        public int ColumnPosition { get; set; }
    }
}

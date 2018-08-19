using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public enum IndexType
	{
		Index,
		PrimaryKey,
		UniqueKey
	}

	public class Index
	{
		public Table Table { get; set; }
		public string Name { get; set; }
		public IndexType Type { get; set; }
		public IEnumerable<string> ColumnNames { get; set; }
	}
}
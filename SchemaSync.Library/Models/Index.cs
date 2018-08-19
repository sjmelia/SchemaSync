using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public enum IndexType
	{
		Index,
		PrimaryKey,
		UniqueKey
	}

	public enum SortDirection
	{
		Ascending,
		Descending
	}

	public class Index
	{
		public Table Table { get; set; }
		public string Name { get; set; }
		public IndexType Type { get; set; }
		public IEnumerable<IndexColumn> Columns { get; set; }
	}

	public class IndexColumn
	{
		public string Name { get; set; }
		public SortDirection SortDirection { get; set; }
		public int Position { get; set; }
	}
}
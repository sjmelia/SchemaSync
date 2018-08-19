using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class Table
	{
		public string Schema { get; set; }
		public string Name { get; set; }
		public string IdentityColumn { get; set; }
		public string ClusteredIndex { get; set; }

		/// <summary>
		/// If empty, then it's okay to drop and rebuild table
		/// </summary>
		public bool IsEmpty { get; set; }

		public IEnumerable<Column> Columns { get; set; }
		public IEnumerable<Index> Indexes { get; set; }
	}
}
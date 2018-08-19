using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class Schema
	{
		public IEnumerable<Table> Tables { get; set; }
		public IEnumerable<ForeignKey> ForeignKeys { get; set; }
	}
}
using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class Database
	{
		public IEnumerable<Table> Tables { get; set; }
		public IEnumerable<ForeignKey> ForeignKeys { get; set; }
		public IEnumerable<Procedure> Procedures { get; set; }
		public IEnumerable<View> Views { get; set; }
	}
}
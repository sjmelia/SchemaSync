using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class Procedure
	{
		public string Name { get; set; }
		public IEnumerable<Parameter> Parameters { get; set; }
		public string Body { get; set; }
	}
}
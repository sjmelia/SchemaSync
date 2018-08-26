using SchemaSync.Library.Models;
using System.Collections.Generic;

namespace SchemaSync.Library
{
	public class ObjectComparison
	{
		public IEnumerable<DbObject> Create { get; set; }
		public IEnumerable<DbObject> Alter { get; set; }
		public IEnumerable<DbObject> Drop { get; set; }
	}
}
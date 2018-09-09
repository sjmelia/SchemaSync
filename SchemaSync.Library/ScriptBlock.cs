using SchemaSync.Library.Models;
using System.Collections.Generic;

namespace SchemaSync.Library
{
	public enum ActionType
	{
		Create,
		Alter,
		Drop
	}

	public class ScriptBlock
	{
		public ActionType ActionType { get; set; }
		public IEnumerable<DbObject> Objects { get; set; }
		public IEnumerable<string> Commands { get; set; }
	}
}
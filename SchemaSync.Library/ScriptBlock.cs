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

	/// <summary>
	/// Relates a set of DbObjects with a set of SQL commands
	/// For example, two column additions can relate to a single table rebuild, which is one script block
	/// </summary>
	public class ScriptBlock
	{
		public ActionType ActionType { get; set; }
		public IEnumerable<DbObject> Objects { get; set; }
		public IEnumerable<string> Commands { get; set; }

		/// <summary>
		/// Returns all commands for the block each followed by the SQL syntax batch separator
		/// </summary>		
		public IEnumerable<string> GetCommands(SqlSyntax syntax)
		{
			foreach (string cmd in Commands)
			{
				yield return cmd;
				yield return syntax.BatchSeparator;
			}			
		}
	}
}
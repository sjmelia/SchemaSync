using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class Column : DbObject
	{
		public Table Table { get; set; }
		public string Name { get; set; }

		/// <summary>
		/// Combines the system type, length, precision, scale, and identity info
		/// </summary>
		public string DataType { get; set; }
		public bool IsNullable { get; set; }		
		public string Default { get; set; }
		public int Position { get; set; }		

		public override IEnumerable<string> AlterCommands()
		{
			yield return $"ALTER TABLE <{Table}> ALTER {Syntax()}";
		}

		public override IEnumerable<string> CreateCommands()
		{
			yield return $"ALTER TABLE <{Table}> ADD {Syntax()}";
		}

		public override IEnumerable<string> DropCommands()
		{
			yield return $"ALTER TABLE <{Table}> DROP COLUMN <{Name}>";
		}

		public override bool Equals(object obj)
		{
			Column col = obj as Column;
			if (col != null)
			{
				return Table.Equals(col.Table) && (Name ?? string.Empty).ToLower().Equals(col.Name.ToLower());
			}
			return false;
		}

		internal string Syntax()
		{
			string result = $"<{Name}> {DataType} {((IsNullable) ? "NULL" : "NOT NULL")}";
			if (!string.IsNullOrEmpty(Default)) result += $" DEFAULT ({Default})";
			return result;
		}

		public override int GetHashCode()
		{
			return Table.GetHashCode() + (Name ?? string.Empty).ToLower().GetHashCode();
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<DbObject> GetDependencies(Database database)
		{
			return Enumerable.Empty<DbObject>();
		}
	}
}
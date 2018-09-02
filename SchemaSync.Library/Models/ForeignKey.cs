using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class ForeignKey : DbObject
	{
		public string Name { get; set; }
		public Table ReferencedTable { get; set; }
		public Table ReferencingTable { get; set; }
		public IEnumerable<Column> Columns { get; set; }
		public bool CascadeDelete { get; set; }
		public bool CascadeUpdate { get; set; }
		
		public class Column
		{
			public string ReferencedName { get; set; }
			public string ReferencingName { get; set; }

			public override bool Equals(object obj)
			{
				Column col = obj as Column;
				if (col != null)
				{
					return col.ReferencedName.ToLower().Equals(ReferencedName.ToLower()) && col.ReferencingName.ToLower().Equals(ReferencingName.ToLower());
				}
				return false;
			}

			public override int GetHashCode()
			{
				return ReferencedName.ToLower().GetHashCode() + ReferencingName.ToLower().GetHashCode();
			}
		}

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			string referencingColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencingName}>"));
			string referencedColumns = string.Join(", ", Columns.Select(col => $"<{col.ReferencedName}>"));

			string cmd = $"ALTER TABLE <{ReferencingTable}> ADD CONSTRAINT <{Name}> FOREIGN KEY (\r\n\t{referencingColumns}\r\n) REFERENCES <{ReferencedTable}> (\r\n\t{referencedColumns}\r\n)";
			if (CascadeUpdate) cmd += " ON UPDATE CASCADE";
			if (CascadeDelete) cmd += " ON DELETE CASCADE";
			yield return cmd;
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
		{
			yield return $"ALTER TABLE <{ReferencedTable}> DROP CONSTRAINT <{Name}>";
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			ForeignKey fk = obj as ForeignKey;
			if (fk != null)
			{
				return
					fk.ReferencedTable.Equals(ReferencedTable) &&
					fk.ReferencingTable.Equals(ReferencingTable) &&
					fk.Columns.SequenceEqual(Columns);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}
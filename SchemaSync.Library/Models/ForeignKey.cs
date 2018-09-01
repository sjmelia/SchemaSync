using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class ForeignKey : DbObject
	{
		public string Name { get; set; }
		public Table ReferencedTable { get; set; }
		public Column ReferencingColumn { get; set; }
		public bool CascadeDelete { get; set; }
		public bool CascadeUpdate { get; set; }

		public override IEnumerable<string> AlterCommands(SqlSyntax syntax)
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			string cmd = $"ALTER TABLE <{ReferencingColumn.Table}> ADD CONSTRAINT <{Name}> FOREIGN KEY (<{ReferencingColumn.Name}>) REFERENCES <{ReferencedTable}> (<{ReferencedTable.IdentityColumn}>)";
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
				return fk.ReferencedTable.Equals(ReferencedTable) && fk.ReferencingColumn.Equals(ReferencingColumn);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return ReferencedTable.GetHashCode() + ReferencingColumn.GetHashCode();
		}
	}
}
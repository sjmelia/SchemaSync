using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public enum IndexType
	{
		PrimaryKey = 1,
		UniqueIndex = 2,
		UniqueConstraint = 3,
		NonUnique = 4		
	}

	public enum SortDirection
	{
		Ascending,
		Descending
	}

	public class Index : DbObject
	{
		public Table Table { get; set; }
		public string Name { get; set; }
		public IndexType Type { get; set; }
		public bool IsClustered { get; set; }
		public IEnumerable<IndexColumn> Columns { get; set; }

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			string columnList = string.Join(", ", Columns.OrderBy(col => col.Position).Select(col => $"<{col.Name}> {((col.SortDirection == SortDirection.Ascending) ? "ASC" : "DESC")}"));

			string clustered = (IsClustered) ? "CLUSTERED" : "NONCLUSTERED";

			switch (Type)
			{
				case IndexType.UniqueIndex:
					yield return $"CREATE {clustered} INDEX <{Name}> ON <{Table}> ({columnList})";
					break;

				case IndexType.UniqueConstraint:
					yield return $"ALTER TABLE <{Table}> ADD CONSTRAINT <{Name}> UNIQUE {clustered} ({columnList})";
					break;

				case IndexType.PrimaryKey:
					yield return $"ALTER TABLE <{Table}> ADD CONSTRAINT <{Name}> PRIMARY KEY {clustered} ({columnList})";
					break;
			}			
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
		{
			yield return $"DROP INDEX <{Name}> ON <{Table}>";
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			var test = obj as Index;
			if (test != null)
			{
				return test.Table.Equals(Table) && test.Name.ToLower().Equals(Name.ToLower());
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Table.GetHashCode() + Name.ToLower().GetHashCode();
		}
	}

	public class IndexColumn
	{
		public string Name { get; set; }
		public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
		public int Position { get; set; }
	}
}
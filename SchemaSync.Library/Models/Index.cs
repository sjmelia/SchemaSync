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
		public bool IsClustered { get { return Name.Equals(Table.ClusteredIndex); } }
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
			switch (Type)
			{
				case IndexType.UniqueIndex:
				case IndexType.NonUnique:
					yield return $"DROP INDEX <{Name}> ON <{Table}>";
					break;

				case IndexType.UniqueConstraint:
				case IndexType.PrimaryKey:
					yield return $"ALTER TABLE <{Table}> DROP CONSTRAINT <{Name}>";
					break;
			}			
		}

		public override IEnumerable<string> AlterCommands(SqlSyntax syntax)
		{
			if (!string.IsNullOrEmpty(AlterDescription)) yield return $"{syntax.CommentStart} {AlterDescription}";
			foreach (var cmd in base.AlterCommands(syntax)) yield return cmd;
		}

		public override bool IsAltered(DbObject compare)
		{
			var test = compare as Index;
			if (test != null)
			{
				if (!test.Columns.SequenceEqual(Columns))
				{
					string srcColumns = string.Join(", ", Columns.Select(col => $"{col.Name}-{col.SortDirection}"));
					string destColumns = string.Join(", ", test.Columns.Select(col => $"{col.Name}-{col.SortDirection}"));
					AlterDescription = $"Index columns changed from {srcColumns} to {destColumns}";
					return true;
				}
			}
			return false;
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

		public override string ToString()
		{
			return $"{Table}.{Name}";
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

		public override bool Equals(object obj)
		{
			IndexColumn test = obj as IndexColumn;
			if (test != null)
			{
				return test.Name.ToLower().Equals(Name.ToLower()) && test.Position == Position && test.SortDirection == SortDirection;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.ToLower().GetHashCode() + Position.GetHashCode() + SortDirection.GetHashCode();
		}
	}
}
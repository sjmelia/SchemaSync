using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public enum IndexType
	{
		Index,
		PrimaryKey,
		UniqueKey
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
		public IEnumerable<IndexColumn> Columns { get; set; }

		public override IEnumerable<string> Alter()
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> Create()
		{
			string columnList = string.Join(", ", Columns.OrderBy(col => col.Position).Select(col => $"<{col.Name}> {((col.SortDirection == SortDirection.Ascending) ? "ASC" : "DESC")}"));

			switch (Type)
			{
				case IndexType.Index:
					yield return $"CREATE INDEX <{Name}> ON <{Table}> ({columnList})";
					break;

				case IndexType.UniqueKey:
					yield return $"ALTER TABLE <{Table}> ADD CONSTRAINT <{Name}> UNIQUE ({columnList})";
					break;

				case IndexType.PrimaryKey:
					yield return $"ALTER TABLE <{Table}> ADD CONSTRAINT <{Name}> PRIMARY KEY ({columnList})";
					break;
			}			
		}

		public override IEnumerable<string> Drop()
		{
			yield return $"DROP INDEX <{Name}> ON <{Table}>";
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}
	}

	public class IndexColumn
	{
		public string Name { get; set; }
		public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
		public int Position { get; set; }
	}
}
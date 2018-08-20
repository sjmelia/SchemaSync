using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class Table : DbObject
	{
		public string Schema { get; set; }
		public string Name { get; set; }
		public string IdentityColumn { get; set; }
		public string ClusteredIndex { get; set; }

		/// <summary>
		/// If empty, then it's okay to drop and rebuild table
		/// </summary>
		public bool IsEmpty { get; set; }

		public IEnumerable<Column> Columns { get; set; } = Enumerable.Empty<Column>();
		public IEnumerable<Index> Indexes { get; set; } = Enumerable.Empty<Index>();

		public override IEnumerable<string> AlterCommands()
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> CreateCommands()
		{
			string columns = string.Join(",\r\n", Columns.OrderBy(col => col.Position).Select(col => $"\t{col.Syntax()}"));
			yield return $"CREATE TABLE <{ToString()}> (\r\n{columns})";
		}

		public override IEnumerable<string> DropCommands()
		{
			yield return $"DROP TABLE <{ToString()}>";
		}

		public override bool Equals(object obj)
		{
			Table tbl = obj as Table;
			if (tbl != null)
			{
				return (Schema ?? string.Empty).ToLower().Equals(tbl.Schema.ToLower()) && (Name ?? string.Empty).ToLower().Equals(tbl.Name.ToLower());
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Schema ?? string.Empty).ToLower().GetHashCode() + (Name ?? string.Empty).ToLower().GetHashCode();
		}

		public override string ToString()
		{
			return (!string.IsNullOrEmpty(Schema)) ? $"{Schema}.{Name}" : Name;
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<DbObject> GetDependencies(Database database)
		{
			return database.ForeignKeys.Where(fk => fk.ReferencedTable.Equals(this));
		}
	}
}
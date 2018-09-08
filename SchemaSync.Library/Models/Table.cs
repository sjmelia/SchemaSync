using System;
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
		public int RowCount { get; set; }

		/// <summary>
		/// If empty, then it's okay to drop and rebuild table
		/// </summary>
		public bool IsEmpty { get { return RowCount == 0; } }

		public IEnumerable<Column> Columns { get; set; } = Enumerable.Empty<Column>();
		public IEnumerable<Index> Indexes { get; set; } = Enumerable.Empty<Index>();

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			string columns = string.Join(",\r\n", Columns.OrderBy(col => col.InternalId).Select(col => $"\t{col.Definition(syntax)}"));
			yield return $"CREATE TABLE <{ToString()}> (\r\n{columns}\r\n)";

			foreach (var index in Indexes)
			{
				foreach (var cmd in index.CreateCommands(syntax)) yield return cmd;
			}
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
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

		public override bool IsAltered(DbObject compare)
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<DbObject> GetDependencies(Database database)
		{
			return database.ForeignKeys.Where(fk => fk.ReferencedTable.Equals(this));
		}
	}
}
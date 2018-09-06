using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class Column : DbObject
	{
		public Table Table { get; set; }
		public string Name { get; set; }
		public string DataType { get; set; }
		public bool IsNullable { get; set; }
		public string Default { get; set; }
		public string Collation { get; set; }
		public int MaxLength { get; set; }
		public int Scale { get; set; }
		public int Precision { get; set; }		

		public override IEnumerable<string> AlterCommands(SqlSyntax syntax)
		{
			yield return $"ALTER TABLE <{Table}> ALTER COLUMN {Definition(syntax)}";
		}

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			yield return $"ALTER TABLE <{Table}> ADD {Definition(syntax)}";
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
		{
			yield return $"ALTER TABLE <{Table}> DROP COLUMN <{Name}>";
		}

		public override string ToString()
		{
			return $"{Table}.{Name}";
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

		internal string Definition(SqlSyntax syntax)
		{
			string identity = (Table.IdentityColumn?.Equals(Name) ?? false) ? $" {syntax.IdentitySyntax}" : string.Empty;
			string result = $"<{Name}> {syntax.GetDataTypeDefinition(this)}{identity} {((IsNullable) ? "NULL" : "NOT NULL")}";
			if (!string.IsNullOrEmpty(Default)) result += $" DEFAULT ({Default})";
			return result;
		}

		public override int GetHashCode()
		{
			return Table.GetHashCode() + (Name ?? string.Empty).ToLower().GetHashCode();
		}

		public override bool IsAltered(DbObject compare)
		{
			Column test = compare as Column;
			if (test != null)
			{
				if (!test.DataType.Equals(DataType)) return true;
				if (test.MaxLength != MaxLength) return true;
				if (test.IsNullable != IsNullable) return true;
				if (test.Scale != Scale) return true;
				if (test.Precision != Precision) return true;
				if (!test.Collation.Equals(Collation)) return true;
				// not sure how to handle defaults as they don't really impact the column def
			}

			return false;
		}
	}
}
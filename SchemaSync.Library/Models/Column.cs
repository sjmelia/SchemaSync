using System;
using System.Collections.Generic;
using System.Linq;

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

		/// <summary>
		/// Expression used for computed column
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// Expression to use with UPDATE...SET when creating required column in table with data
		/// </summary>
		public string DefaultExpression { get; set; }

		public override IEnumerable<string> AlterCommands(SqlSyntax syntax, Database database)
		{
			if (!string.IsNullOrEmpty(AlterDescription)) yield return $"{syntax.CommentStart} {AlterDescription}";
			yield return $"ALTER TABLE <{Table}> ALTER COLUMN {Definition(syntax)}";
		}

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			if (!Table.IsEmpty && !IsNullable && !string.IsNullOrEmpty(DefaultExpression))
			{
				yield return $"ALTER TABLE <{Table}> ADD {Definition(syntax, true)}";
				yield return $"UPDATE <{Table}> SET <{Name}>={DefaultExpression}";
				yield return $"ALTER TABLE <{Table}> ALTER COLUMN {Definition(syntax)}";
			}
			else
			{
				yield return $"ALTER TABLE <{Table}> ADD {Definition(syntax)}";
			}			
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

		internal string Definition(SqlSyntax syntax, bool? nullableOverride = null)
		{
			string result = null;

			if (!string.IsNullOrEmpty(Expression))
			{
				result = $"<{Name}> AS {Expression}";
			}
			else
			{
				bool isNull = (!nullableOverride.HasValue) ? IsNullable : nullableOverride.Value;
				string identity = (Table.IdentityColumn?.Equals(Name) ?? false) ? $" {syntax.IdentitySyntax}" : string.Empty;
				result = $"<{Name}> {syntax.GetDataTypeDefinition(this)}{identity} {((isNull) ? "NULL" : "NOT NULL")}";
				if (!string.IsNullOrEmpty(Default)) result += $" DEFAULT ({Default})";
			}
			
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
				if (!(test?.Expression ?? string.Empty).Equals(Expression ?? string.Empty))
				{
					AlterDescription = $"Calculation expression changed from {test.Expression} to {Expression}";
					return true;
				}

				if (DataType.Equals("decimal") && string.IsNullOrEmpty(Expression))
				{
					if (test.Scale != Scale)
					{
						AlterDescription = $"Scale changed from {test.Scale} to {Scale}";
						return true;
					}

					if (test.Precision != Precision)
					{
						AlterDescription = $"Precision changed from {test.Precision} to {Precision}";
						return true;
					}
				}

				if (!test.DataType.Equals(DataType))
				{
					AlterDescription = $"Data type changed from {test.DataType} to {DataType}";
					return true;
				}

				if (DataType.StartsWith("var") || DataType.StartsWith("nvar"))
				{
					if (test.MaxLength != MaxLength)
					{
						AlterDescription = $"Max length changed from {test.MaxLength} to {MaxLength}";
						return true;
					}
				}

				if (test.IsNullable != IsNullable && string.IsNullOrEmpty(Expression))
				{
					AlterDescription = $"Nullable changed from {test.IsNullable} to {IsNullable}";
					return true;
				}

				if (!(test?.Collation ?? string.Empty).Equals(Collation ?? test?.Collation ?? string.Empty))
				{
					AlterDescription = $"Collation changed from {test.Collation} to {Collation}";
					return true;
				}
				
				// not sure how to handle defaults as they don't really impact the column def
			}
			
			return false;
		}

		public bool IsForeignKey(Database database, out ForeignKey foreignKey)
		{
			foreignKey = database.ForeignKeys.SingleOrDefault(fk => 
				fk.ReferencingTable.Equals(Table) &&
				fk.Columns.Any(col => col.ReferencingName.Equals(Name)));
			return (foreignKey != null);
		}
	}
}
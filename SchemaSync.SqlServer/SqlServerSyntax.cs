using SchemaSync.Library;
using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SchemaSync.SqlServer
{
	public class SqlServerSyntax : SqlSyntax
	{
		public override string BatchSeparator => "GO";
		public override char StartDelimiter => '[';
		public override char EndDelimiter => ']';
		public override string DefaultSchema => "dbo";
		public override string IdentitySyntax => "identity(1,1)";
		public override bool SupportsSchemas => true;
		public override string CommentStart => "--";

		public override string GetDataTypeDefinition(Column column)
		{
			string result = column.DataType;

			if (result.StartsWith("nvar") || result.StartsWith("var"))
			{
				result += (column.MaxLength > -1) ? $"({column.MaxLength})" : "(max)";
			}

			if (result.Equals("decimal"))
			{
				result += $"({column.Precision}, {column.Scale})";
			}

			return result;
		}

		public override IEnumerable<string> DatabaseCommands(Database database)
		{
			var schemas = database.Tables.Select(t => t.Schema).GroupBy(s => s).Select(grp => grp.Key).ToArray();

			foreach (string schema in schemas)
			{
				yield return $"-- uncomment this next line as needed\r\n-- CREATE SCHEMA [{schema}]";
			}
		}
	}
}
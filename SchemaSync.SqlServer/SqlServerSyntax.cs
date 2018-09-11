using SchemaSync.Library;
using SchemaSync.Library.Models;

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
	}
}
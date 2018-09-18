using SchemaSync.Library;
using SchemaSync.Library.Models;

namespace SchemaSync.MySql
{
	public class MySqlSyntax : SqlSyntax
	{
		public override string BatchSeparator => ";";
		public override char StartDelimiter => '`';
		public override char EndDelimiter => '`';
		public override string DefaultSchema => "dbo";
		public override string IdentitySyntax => "auto_increment";
		public override bool SupportsSchemas => true;
		public override string CommentStart => "#";

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
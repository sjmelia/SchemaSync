using Dapper;
using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SchemaSync.SqlServer
{
	public partial class SqlServerDbProvider
	{
		private IEnumerable<Schema> GetSchemas(IDbConnection connection)
		{
			return connection.Query<string>(
				@"SELECT [name] FROM sys.schemas
				WHERE [name] NOT IN ('guest', 'sys', 'information_schema') AND [name] NOT LIKE 'db[_]%'")
				.Select(s => new Schema() { Name = s });
		}
	}
}
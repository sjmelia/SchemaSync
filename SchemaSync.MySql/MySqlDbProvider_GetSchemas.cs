using Dapper;
using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SchemaSync.MySql
{
	public partial class MySqlDbProvider
	{
		private IEnumerable<Schema> GetSchemas(IDbConnection connection)
		{
            return connection.Query<string>(
                @"SELECT `schema_name` FROM `information_schema`.`schemata`")
                .Except(new string[] { "information_schema", "mysql", "performance_schema", "sys" })
                .Select(s => new Schema() { Name = s });
		}
	}
}
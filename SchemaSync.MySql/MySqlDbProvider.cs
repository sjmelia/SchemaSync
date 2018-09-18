using SchemaSync.Library;
using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SchemaSync.MySql
{
	public partial class MySqlDbProvider : IDbConnectionProvider
	{
		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys | ObjectTypeFlags.Schemas;

		public Func<string, IDbConnection> ConnectionMethod => (connectionString) => new MySqlConnection(connectionString);

		public Database GetDatabase(string connectionString)
		{
			using (var cn = new MySqlConnection(connectionString))
			{
				return GetDatabase(cn);
			}
		}

		public Database GetDatabase(IDbConnection connection)
		{
			Database db = new Database();
			db.Schemas = GetSchemas(connection);
			db.Tables = GetTables(connection);
			db.ForeignKeys = GetForeignKeys(connection);
			return db;
		}

		public SqlSyntax GetDefaultSyntax()
		{
			return new MySqlSyntax();
		}
	}
}
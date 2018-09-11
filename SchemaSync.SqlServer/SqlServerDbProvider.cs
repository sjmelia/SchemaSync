using SchemaSync.Library;
using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SchemaSync.SqlServer
{
	public partial class SqlServerDbProvider : IDbConnectionProvider
	{
		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		public Func<string, IDbConnection> ConnectionMethod => (connectionString) => new SqlConnection(connectionString);

		public Database GetDatabase(string connectionString)
		{
			using (var cn = new SqlConnection(connectionString))
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
			return new SqlServerSyntax();
		}
	}
}
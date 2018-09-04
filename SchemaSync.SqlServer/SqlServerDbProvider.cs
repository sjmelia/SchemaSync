using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System.Data;
using System.Data.SqlClient;

namespace SchemaSync.SqlServer
{
	public partial class SqlServerDbProvider : IDbProviderFromConnection
	{
		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

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
			db.Tables = GetTables(connection);
			db.ForeignKeys = GetForeignKeys(connection);
			return db;
		}
	}
}
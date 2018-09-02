using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System.Data;

namespace SchemaSync.SqlServer
{
	public partial class ConnectionProvider : IDbProviderFromConnection
	{
		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		public Database GetDatabase(IDbConnection connection)
		{
			Database db = new Database();
			db.Tables = GetTables(connection);
			db.ForeignKeys = GetForeignKeys(connection);
			return db;
		}
	}
}
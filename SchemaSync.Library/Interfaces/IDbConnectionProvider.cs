using SchemaSync.Library.Models;
using System.Data;

namespace SchemaSync.Library.Interfaces
{
	public interface IDbConnectionProvider
	{
		ObjectTypeFlags ObjectTypes { get; }

		Database GetDatabase(IDbConnection connection);
	}
}
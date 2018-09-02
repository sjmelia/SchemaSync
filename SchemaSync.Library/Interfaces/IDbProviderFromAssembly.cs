using SchemaSync.Library.Models;
using System.Reflection;

namespace SchemaSync.Library.Interfaces
{
	public interface IDbProviderFromAssembly
	{
		ObjectTypeFlags ObjectTypes { get; }
		Database GetDatabase(Assembly assembly);
	}
}
using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Reflection;

namespace SchemaSync.Library.Interfaces
{
	public interface IDbProviderFromAssembly
	{
		ObjectTypeFlags ObjectTypes { get; }
		Database GetDatabase(Assembly assembly);
		IEnumerable<IgnoredTypeInfo> IgnoredTypes { get; }
	}
}
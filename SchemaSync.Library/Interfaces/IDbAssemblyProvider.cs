using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Reflection;

namespace SchemaSync.Library.Interfaces
{
	public interface IDbAssemblyProvider
	{		
		ObjectTypeFlags ObjectTypes { get; }
		Database GetDatabase(Assembly assembly);
		IEnumerable<IgnoredTypeInfo> IgnoredTypes { get; }
	}
}
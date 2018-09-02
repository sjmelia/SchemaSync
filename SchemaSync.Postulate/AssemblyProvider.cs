using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System;
using System.Reflection;

namespace SchemaSync.Postulate
{
	public class AssemblyProvider : IDbProviderFromAssembly
	{
		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		public Database GetDatabase(Assembly assembly)
		{
			throw new NotImplementedException();
		}
	}
}
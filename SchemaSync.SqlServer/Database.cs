using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SchemaSync.SqlServer
{
	public class Database : Library.Models.Database
	{
		protected override DatabaseSourceFlags SupportedSources => DatabaseSourceFlags.Assembly | DatabaseSourceFlags.Connection;

		protected override ObjectTypeFlags SupportedObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		protected override IEnumerable<Type> GetModelTypes(Assembly assembly)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<ForeignKey> GetForeignKeys(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<Procedure> GetProcedures(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<Procedure> GetProcedures(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<Table> GetTables(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<Table> GetTables(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<View> GetViews(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<View> GetViews(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override Column ColumnFromProperty(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}
	}
}
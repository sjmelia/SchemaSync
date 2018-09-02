using Dapper;
using SchemaSync.Library.Models;
using SchemaSync.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SchemaSync.SqlServer
{
	public partial class SqlServerDatabase : Database
	{
		public SqlServerDatabase()
		{
		}

		public SqlServerDatabase(Assembly assembly, ObjectTypeFlags objectTypes = ObjectTypeFlags.TablesAndForeignKeys)
		{
			LoadFromAssembly(assembly, objectTypes);
		}

		public SqlServerDatabase(IDbConnection connection, ObjectTypeFlags objectTypes = ObjectTypeFlags.TablesAndForeignKeys)
		{
			LoadFromConnection(connection, objectTypes);
		}

		protected override IEnumerable<Type> GetModelTypes(IEnumerable<Type> assemblyTypes)
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

		protected override Column GetColumnFromProperty(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		protected override Table GetTableFromType(Type modelType)
		{
			throw new NotImplementedException();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SchemaSync.Library.Models
{
	[Flags]
	public enum DatabaseSourceFlags
	{
		NotSet = 0,
		Connection = 1,
		Assembly = 2
	}

	[Flags]
	public enum ObjectTypeFlags
	{
		Tables = 1,
		ForeignKeys = 2,
		Procedures = 4,
		Views = 8
	}

	public abstract class Database
	{
		public DatabaseSourceFlags Source { get; private set; }
		public IEnumerable<Table> Tables { get; set; }
		public IEnumerable<ForeignKey> ForeignKeys { get; set; }
		public IEnumerable<Procedure> Procedures { get; set; }
		public IEnumerable<View> Views { get; set; }

		public Database()
		{
			Source = DatabaseSourceFlags.NotSet;
		}

		public ObjectComparison Compare(Database database)
		{
			var result = new ObjectComparison();
			result.Create = CompareCreateObjects(database);
			result.Alter = CompareAlterObjects(database);
			result.Drop = CompareDropObjects(database);
			return result;
		}

		private IEnumerable<DbObject> CompareCreateObjects(Database database)
		{
			List<DbObject> results = new List<DbObject>();

			var newTables = Tables.Where(t => !database.Tables.Contains(t));
			results.AddRange(newTables);

			var matchingTables = Tables.Where(t => database.Tables.Contains(t));

			var newColumns = matchingTables.SelectMany(t => t.Columns).Where(c => !database.Tables.SelectMany(t => t.Columns).Contains(c));
			results.AddRange(newColumns);

			var newForeignKeys = ForeignKeys.Where(fk => !database.ForeignKeys.Contains(fk));
			results.AddRange(newForeignKeys);

			// indexes

			return results;
		}

		private IEnumerable<DbObject> CompareAlterObjects(Database database)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<DbObject> CompareDropObjects(Database database)
		{
			throw new NotImplementedException();
		}

		#region static initializer methods
		public static T FromConnection<T>(IDbConnection connection) where T : Database, new()
		{
			var db = new T();
			if ((db.SupportedSources & DatabaseSourceFlags.Connection) == DatabaseSourceFlags.Connection)
			{
				db.Source = DatabaseSourceFlags.Connection;
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Tables) == ObjectTypeFlags.Tables) db.Tables = db.GetTables(connection);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.ForeignKeys) == ObjectTypeFlags.ForeignKeys) db.ForeignKeys = db.GetForeignKeys(connection);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Procedures) == ObjectTypeFlags.Procedures) db.Procedures = db.GetProcedures(connection);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Views) == ObjectTypeFlags.Views) db.Views = db.GetViews(connection);
				return db;
			}
			else
			{
				throw new NotImplementedException("This Database doesn't implement an IDbConnection source.");
			}
		}

		public static T FromAssembly<T>(string assemblyFile) where T : Database, new()
		{
			var assembly = Assembly.LoadFile(assemblyFile);
			return FromAssembly<T>(assembly);
		}

		public static T FromAssembly<T>(Assembly assembly) where T : Database, new()
		{
			var db = new T();
			if ((db.SupportedSources & DatabaseSourceFlags.Assembly) == DatabaseSourceFlags.Assembly)
			{
				db.Source = DatabaseSourceFlags.Assembly;
				var modelTypes = db.GetModelTypes(assembly);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Tables) == ObjectTypeFlags.Tables) db.Tables = db.GetTables(modelTypes);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.ForeignKeys) == ObjectTypeFlags.ForeignKeys) db.ForeignKeys = db.GetForeignKeys(modelTypes);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Procedures) == ObjectTypeFlags.Procedures) db.Procedures = db.GetProcedures(modelTypes);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Views) == ObjectTypeFlags.Views) db.Views = db.GetViews(modelTypes);
				return db;
			}
			else
			{
				throw new NotImplementedException("This Database doesn't implement an Assembly source.");
			}
		}
		#endregion

		#region object discovery abstract methods

		protected abstract DatabaseSourceFlags SupportedSources { get; }
		protected abstract ObjectTypeFlags SupportedObjectTypes { get; }		

		// connection-source
		protected abstract IEnumerable<Table> GetTables(IDbConnection connection);
		protected abstract IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection);
		protected abstract IEnumerable<Procedure> GetProcedures(IDbConnection connection);
		protected abstract IEnumerable<View> GetViews(IDbConnection connection);

		// assembly source
		protected abstract IEnumerable<Type> GetModelTypes(Assembly assembly);
		protected abstract IEnumerable<Table> GetTables(IEnumerable<Type> modelTypes);
		protected abstract Column ColumnFromProperty(PropertyInfo propertyInfo);
		protected abstract IEnumerable<ForeignKey> GetForeignKeys(IEnumerable<Type> modelTypes);
		protected abstract IEnumerable<Procedure> GetProcedures(IEnumerable<Type> modelTypes);
		protected abstract IEnumerable<View> GetViews(IEnumerable<Type> modelTypes);
		
		#endregion
	}
}
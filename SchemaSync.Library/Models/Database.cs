using System;
using System.Collections.Generic;
using System.Data;
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
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Tables) == ObjectTypeFlags.Tables) db.Tables = db.GetTables(assembly);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.ForeignKeys) == ObjectTypeFlags.ForeignKeys) db.ForeignKeys = db.GetForeignKeys(assembly);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Procedures) == ObjectTypeFlags.Procedures) db.Procedures = db.GetProcedures(assembly);
				if ((db.SupportedObjectTypes & ObjectTypeFlags.Views) == ObjectTypeFlags.Views) db.Views = db.GetViews(assembly);
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
		protected abstract bool SupportsTableSchemas { get; }
		protected abstract char StartDelimiter { get; }
		protected abstract char EndDelimiter { get; }

		protected abstract IEnumerable<Table> GetTables(IDbConnection connection);
		protected abstract IEnumerable<Table> GetTables(Assembly assembly);

		protected abstract IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection);
		protected abstract IEnumerable<ForeignKey> GetForeignKeys(Assembly assembly);

		protected abstract IEnumerable<Procedure> GetProcedures(IDbConnection connection);
		protected abstract IEnumerable<Procedure> GetProcedures(Assembly assembly);

		protected abstract IEnumerable<View> GetViews(IDbConnection connection);
		protected abstract IEnumerable<View> GetViews(Assembly assembly);
		#endregion
	}
}
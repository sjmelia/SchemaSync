using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SchemaSync.Library.Models
{
	public enum DatabaseSource
	{
		Connection,
		Assembly
	}

	public abstract class Database
	{
		public DatabaseSource Source { get; private set; }
		public IEnumerable<Table> Tables { get; set; }
		public IEnumerable<ForeignKey> ForeignKeys { get; set; }
		public IEnumerable<Procedure> Procedures { get; set; }
		public IEnumerable<View> Views { get; set; }

		public static T FromConnection<T>(IDbConnection connection) where T : Database, new()
		{
			var db = new T();
			db.Source = DatabaseSource.Connection;
			db.Tables = db.GetTables(connection);
			db.ForeignKeys = db.GetForeignKeys(connection);
			db.Procedures = db.GetProcedures(connection);
			db.Views = db.GetViews(connection);
			return db;
		}

		public static T FromAssembly<T>(string assemblyFile) where T : Database, new()
		{
			var assembly = Assembly.LoadFile(assemblyFile);
			return FromAssembly<T>(assembly);
		}

		public static T FromAssembly<T>(Assembly assembly) where T : Database, new()
		{
			var db = new T();
			db.Source = DatabaseSource.Assembly;
			db.Tables = db.GetTables(assembly);
			db.ForeignKeys = db.GetForeignKeys(assembly);
			return db;
		}

		protected abstract IEnumerable<Table> GetTables(IDbConnection connection);
		protected abstract IEnumerable<Table> GetTables(Assembly assembly);
		protected abstract IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection);
		protected abstract IEnumerable<ForeignKey> GetForeignKeys(Assembly assembly);
		protected abstract IEnumerable<Procedure> GetProcedures(IDbConnection connection);
		protected abstract IEnumerable<View> GetViews(IDbConnection connection);
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace SchemaSync.Library.Models
{
	[Flags]
	public enum ObjectTypeFlags
	{
		Tables = 1,
		ForeignKeys = 2,
		Procedures = 4,
		Views = 8,
		TablesAndForeignKeys = Tables | ForeignKeys,
		All = Tables | ForeignKeys | Procedures | Views
	}

	public abstract class Database
	{
		public string Collation { get; set; }
		public IEnumerable<Table> Tables { get; set; }
		public IEnumerable<ForeignKey> ForeignKeys { get; set; }
		public IEnumerable<Procedure> Procedures { get; set; }
		public IEnumerable<View> Views { get; set; }

		public void LoadFromConnection(IDbConnection connection, ObjectTypeFlags objectTypes = ObjectTypeFlags.TablesAndForeignKeys)
		{
			if ((objectTypes & ObjectTypeFlags.Tables) == ObjectTypeFlags.Tables) Tables = GetTables(connection);
			if ((objectTypes & ObjectTypeFlags.ForeignKeys) == ObjectTypeFlags.ForeignKeys) ForeignKeys = GetForeignKeys(connection);
			if ((objectTypes & ObjectTypeFlags.Procedures) == ObjectTypeFlags.Procedures) Procedures = GetProcedures(connection);
			if ((objectTypes & ObjectTypeFlags.Views) == ObjectTypeFlags.Views) Views = GetViews(connection);
		}

		public void LoadFromAssembly(string path, ObjectTypeFlags objectTypes = ObjectTypeFlags.TablesAndForeignKeys)
		{
			var assembly = Assembly.LoadFile(path);
			LoadFromAssembly(assembly, objectTypes);
		}

		public void LoadFromAssembly(Assembly assembly, ObjectTypeFlags objectTypes = ObjectTypeFlags.TablesAndForeignKeys)
		{
			var modelTypes = GetModelTypes(assembly.GetTypes());
			if ((objectTypes & ObjectTypeFlags.Tables) == ObjectTypeFlags.Tables) Tables = GetTables(modelTypes);
			if ((objectTypes & ObjectTypeFlags.ForeignKeys) == ObjectTypeFlags.ForeignKeys) ForeignKeys = GetForeignKeys(modelTypes);
			if ((objectTypes & ObjectTypeFlags.Procedures) == ObjectTypeFlags.Procedures) Procedures = GetProcedures(modelTypes);
			if ((objectTypes & ObjectTypeFlags.Views) == ObjectTypeFlags.Views) Views = GetViews(modelTypes);
		}

		#region object discovery abstract methods

		// connection-source
		protected abstract IEnumerable<Table> GetTables(IDbConnection connection);

		protected abstract IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection);

		protected abstract IEnumerable<Procedure> GetProcedures(IDbConnection connection);

		protected abstract IEnumerable<View> GetViews(IDbConnection connection);

		// assembly source
		protected abstract IEnumerable<Type> GetModelTypes(IEnumerable<Type> assemblyTypes);

		protected abstract IEnumerable<Table> GetTables(IEnumerable<Type> modelTypes);

		protected abstract Column GetColumnFromProperty(PropertyInfo propertyInfo);

		protected abstract Table GetTableFromType(Type modelType);

		protected abstract IEnumerable<ForeignKey> GetForeignKeys(IEnumerable<Type> modelTypes);

		protected abstract IEnumerable<Procedure> GetProcedures(IEnumerable<Type> modelTypes);

		protected abstract IEnumerable<View> GetViews(IEnumerable<Type> modelTypes);

		#endregion object discovery abstract methods

		public void SaveScript(SqlSyntax syntax, string path)
		{
			using (var file = File.CreateText(path))
			{
				foreach (var cmd in syntax.DatabaseCommands(this))
				{
					WriteCommand(syntax, file, cmd);
					EndBatch(syntax, file);
				}

				foreach (var t in Tables) WriteCommands(syntax, file, t);

				foreach (var fk in ForeignKeys) WriteCommands(syntax, file, fk);
			}
		}

		private static void WriteCommands(SqlSyntax syntax, StreamWriter file, DbObject @object)
		{
			foreach (var cmd in @object.CreateCommands(syntax)) WriteCommand(syntax, file, cmd);
			EndBatch(syntax, file);
		}

		private static void EndBatch(SqlSyntax syntax, StreamWriter file)
		{
			file.WriteLine("\n" + syntax.BatchSeparator);
		}

		private static void WriteCommand(SqlSyntax syntax, StreamWriter file, string cmd)
		{
			file.Write("\n" + syntax.ApplyDelimiters(cmd));
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{	
	public abstract class DbObject
	{
		/// <summary>
		/// Set by IsAltered to indicate what was if an object was altered
		/// </summary>
		public string AlterDescription { get; protected set; }

		/// <summary>
		/// Used by SQL Server to identify tables and other top-level database objects
		/// </summary>
		public int ObjectId { get; set; }

		/// <summary>
		/// Used for things like sys.indexes.index_id or possibly sys.columns.column_id,
		/// a value that makes an enumerated thing unique within an object
		/// </summary>
		public int InternalId { get; set; }

		/// <summary>
		/// Returns true if an object was modified and needs to be altered or rebuilt
		/// </summary>
		public abstract bool IsAltered(DbObject compare);

		/// <summary>
		/// Generates the SQL CREATE statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> CreateCommands(SqlSyntax syntax);

		/// <summary>
		/// Generates the SQL DROP statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> DropCommands(SqlSyntax syntax);

		/// <summary>
		/// Generates the SQL ALTER statement(s) for an object.
		/// Default action is to delete and rebuild the object
		/// </summary>
		public virtual IEnumerable<string> AlterCommands(SqlSyntax syntax)
		{
			foreach (var cmd in DropCommands(syntax)) yield return cmd;
			foreach (var cmd in CreateCommands(syntax)) yield return cmd;
		}

		/// <summary>
		/// Override this to get the dependencies of an object that must be dropped and created whenever rebuilding this object
		/// </summary>
		public virtual IEnumerable<DbObject> GetDependencies(Database database)
		{
			return Enumerable.Empty<DbObject>();
		}

		public IEnumerable<string> Rebuild(Database database, SqlSyntax syntax)
		{
			var dependencies = GetDependencies(database);

			foreach (var @object in dependencies)
			{
				foreach (var cmd in @object.DropCommands(syntax)) yield return cmd;
			}

			foreach (var cmd in DropCommands(syntax)) yield return cmd;

			foreach (var cmd in CreateCommands(syntax)) yield return cmd;

			foreach (var @object in dependencies)
			{
				foreach (var cmd in @object.CreateCommands(syntax)) yield return cmd;
			}
		}

		public Type SourceType { get; set; }
	}
}
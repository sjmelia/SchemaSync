using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public abstract class DbObject
	{
		/// <summary>
		/// Used by SQL Server to identify objects
		/// </summary>
		public int ObjectId { get; set; }

		/// <summary>
		/// Returns true if an object was modified and needs to be altered or rebuilt
		/// </summary>
		public abstract bool IsAltered(object compare);

		/// <summary>
		/// Generates the SQL CREATE statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> CreateCommands(SqlSyntax syntax);

		/// <summary>
		/// Generates the SQL DROP statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> DropCommands(SqlSyntax syntax);

		/// <summary>
		/// Generates the SQL ALTER statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> AlterCommands(SqlSyntax syntax);

		/// <summary>
		/// Override this to get the dependencies of an object that must be dropped and created whenever rebuilding this object
		/// </summary>
		public virtual IEnumerable<DbObject> GetDependencies(Database database)
		{
			return Enumerable.Empty<DbObject>();
		}

		public IEnumerable<string> Rebuild(Database database, SqlSyntax syntax)
		{
			foreach (var @object in GetDependencies(database))
			{
				foreach (var cmd in @object.DropCommands(syntax)) yield return cmd;
			}

			foreach (var cmd in DropCommands(syntax)) yield return cmd;

			foreach (var cmd in CreateCommands(syntax)) yield return cmd;

			foreach (var @object in GetDependencies(database))
			{
				foreach (var cmd in @object.CreateCommands(syntax)) yield return cmd;
			}
		}
	}
}
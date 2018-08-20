using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public abstract class DbObject
	{
		/// <summary>
		/// Returns true if an object was modified and needs to be altered or rebuilt
		/// </summary>
		public abstract bool IsAltered(object compare);

		/// <summary>
		/// Generates the SQL CREATE statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> CreateCommands();

		/// <summary>
		/// Generates the SQL DROP statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> DropCommands();

		/// <summary>
		/// Generates the SQL ALTER statement(s) for an object
		/// </summary>
		public abstract IEnumerable<string> AlterCommands();

		public abstract IEnumerable<DbObject> GetDependencies(Database database);

		public IEnumerable<string> Rebuild(Database database)
		{
			foreach (var @object in GetDependencies(database))
			{
				foreach (var cmd in @object.DropCommands()) yield return cmd;
			}

			foreach (var cmd in DropCommands()) yield return cmd;

			foreach (var cmd in CreateCommands()) yield return cmd;

			foreach (var @object in GetDependencies(database))
			{
				foreach (var cmd in @object.CreateCommands()) yield return cmd;
			}
		}
	}
}
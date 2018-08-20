using System;
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
		public abstract IEnumerable<string> Create();

		/// <summary>
		/// Generates the SQL DROP statement(s) for an object
		/// </summary>		
		public abstract IEnumerable<string> Drop();

		/// <summary>
		/// Generates the SQL ALTER statement(s) for an object
		/// </summary>		
		public abstract IEnumerable<string> Alter();

		public IEnumerable<string> Rebuild()
		{
			foreach (var cmd in Drop()) yield return cmd;
			foreach (var cmd in Create()) yield return cmd;
		}
	}
}
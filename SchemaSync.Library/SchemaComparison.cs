using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library
{
	public class SchemaComparison
	{
		public SchemaComparison(Database source, Database destination)
		{
			Source = source;
			Destination = destination;
		}

		public static SchemaComparison Execute(Database source, Database destination)
		{
			var sc = new SchemaComparison(source, destination);
			sc.Execute();
			return sc;
		}

		public void Execute()
		{
			Create = CompareCreateObjects(Source, Destination);
		}

		public Database Source { get; private set; }
		public Database Destination { get; private set; }

		public IEnumerable<DbObject> Create { get; private set; }
		public IEnumerable<DbObject> Alter { get; private set; }
		public IEnumerable<DbObject> Drop { get; private set; }

		private static IEnumerable<DbObject> CompareCreateObjects(Database source, Database destination)
		{
			List<DbObject> results = new List<DbObject>();

			var newTables = source.Tables.Where(t => !destination.Tables.Contains(t));
			results.AddRange(newTables);

			var matchingTables = from s in source.Tables
								 join d in destination.Tables on s equals d
								 select s;

			var newColumns = matchingTables.SelectMany(t => t.Columns).Where(c => !destination.Tables.SelectMany(t => t.Columns).Contains(c));
			results.AddRange(newColumns);

			var newForeignKeys = source.ForeignKeys.Where(fk => !destination.ForeignKeys.Contains(fk));
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

	}
}
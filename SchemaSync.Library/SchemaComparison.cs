using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
			Alter = Enumerable.Empty<DbObject>();
			Drop = Enumerable.Empty<DbObject>();
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

			var newIndexes = matchingTables.SelectMany(t => t.Indexes).Where(x => !destination.Tables.SelectMany(t => t.Indexes).Contains(x));
			results.AddRange(newIndexes);

			return results;
		}

		private static IEnumerable<DbObject> CompareAlterObjects(Database source, Database destination)
		{
			List<DbObject> results = new List<DbObject>();

			var alteredColumns = from s in source.Tables.SelectMany(t => t.Columns)
								  join d in destination.Tables.SelectMany(t => t.Columns) on s equals d
								  where s.IsAltered(d)
								  select s;
			results.AddRange(alteredColumns);

			return results;
		}

		private IEnumerable<DbObject> CompareDropObjects(Database database)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> GetScriptCommands(SqlSyntax syntax)
		{
			var createColumns = Create.OfType<Column>();
			var createOther = Create.Where(obj => !obj.GetType().Equals(typeof(Column)));

			foreach (var tbl in createColumns.GroupBy(item => item.Table))
			{
				string columnList = string.Join(", ", tbl.Select(col => col.Name));

				if (tbl.Key.IsEmpty)
				{					
					yield return $"{syntax.CommentStart} rebuilding empty table {tbl.Key} to add column(s) {columnList}";

					var deps = tbl.Key.GetDependencies(Destination);
					foreach (var obj in deps)
					{
						foreach (var cmd in obj.DropCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
					}

					foreach (var cmd in tbl.Key.AlterCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);

					foreach (var obj in deps)
					{
						foreach (var cmd in obj.CreateCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
					}
				}
				else
				{
					yield return $"{syntax.CommentStart} table {tbl.Key} has {tbl.Key.RowCount:n0} rows, so new columns are added individually";

					foreach (var col in tbl)
					{
						foreach (var cmd in col.CreateCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
					}
				}
			}

			foreach (var create in createOther)
			{

				foreach (var cmd in create.CreateCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
			}

			foreach (var alter in Alter)
			{
				foreach (var cmd in alter.AlterCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
			}

			foreach (var drop in Drop)
			{
				var deps = drop.GetDependencies(Destination);
				foreach (var obj in deps)
				{
					foreach (var cmd in obj.DropCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
				}

				foreach (var cmd in drop.DropCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
			}
		}

		public void SaveScript(SqlSyntax syntax, string path)
		{
			using (var file = File.CreateText(path))
			{
				foreach (var cmd in GetScriptCommands(syntax))
				{
					file.Write(cmd);
				}
			}
		}
	}
}
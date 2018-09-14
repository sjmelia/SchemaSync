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
			Alter = CompareAlterObjects(Source, Destination);
			Drop = CompareDropObjects(Source, Destination);
		}

		public Database Source { get; private set; }
		public Database Destination { get; private set; }

		public IEnumerable<DbObject> Create { get; private set; }
		public IEnumerable<DbObject> Alter { get; private set; }
		public IEnumerable<DbObject> Drop { get; private set; }

		private static IEnumerable<DbObject> CompareCreateObjects(Database source, Database destination)
		{
			List<DbObject> results = new List<DbObject>();

			var newSchemas = source.Schemas.Where(s => !destination.Schemas.Contains(s));
			results.AddRange(newSchemas);

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

			var alteredKFs = from s in source.ForeignKeys
							 join d in destination.ForeignKeys on s equals d
							 where s.IsAltered(d)
							 select s;
			results.AddRange(alteredKFs);

			var alteredIndexes = from s in source.Tables.SelectMany(t => t.Indexes)
								 join d in destination.Tables.SelectMany(t => t.Indexes) on s equals d
								 where s.IsAltered(d)
								 select s;
			results.AddRange(alteredIndexes);

			return results;
		}

		private IEnumerable<DbObject> CompareDropObjects(Database source, Database destination)
		{
			List<DbObject> results = new List<DbObject>();

			var droppedSchemas = destination.Schemas.Where(s => !source.Schemas.Contains(s));
			results.AddRange(droppedSchemas);

			var droppedTables = destination.Tables.Where(t => !source.Tables.Contains(t));
			results.AddRange(droppedTables);

			var matchingTables = from s in source.Tables
								join d in source.Tables on s equals d
								select d;

			var droppedColumns = matchingTables.SelectMany(t => t.Columns).Where(c => !source.Tables.SelectMany(t => t.Columns).Contains(c));
			results.AddRange(droppedColumns);

			var droppedIndexes = matchingTables.SelectMany(t => t.Indexes).Where(x => !source.Tables.SelectMany(t => t.Indexes).Contains(x));
			results.AddRange(droppedIndexes);

			var droppedFKs = destination.ForeignKeys.Where(fk => !source.ForeignKeys.Contains(fk));
			results.AddRange(droppedFKs);

			return results;
		}

		public IEnumerable<ScriptBlock> GetScriptBlocks(SqlSyntax syntax)
		{
			var createColumns = Create.OfType<Column>();
			var createOther = Create.Where(obj => !obj.GetType().Equals(typeof(Column)));

			List<ScriptBlock> fkScript = new List<ScriptBlock>();

			foreach (var columnGrp in createColumns.GroupBy(item => item.Table))
			{
				string columnList = string.Join(", ", columnGrp.Select(col => col.Name));

				if (columnGrp.Key.IsEmpty)
				{
					yield return new ScriptBlock()
					{
						ActionType = ActionType.Create,
						Objects = columnGrp,
						Commands = RebuildCommands(syntax, columnGrp, columnList)
					};

					foreach (var fk in columnGrp.Key.GetForeignKeys(Source))
					{
						fkScript.Add(new ScriptBlock()
						{
							ActionType = ActionType.Create,
							Objects = columnGrp,
							Commands = CreateCommands(syntax, fk)
						});
					}
				}
				else
				{										
					foreach (var col in columnGrp)
					{
						yield return new ScriptBlock()
						{
							ActionType = ActionType.Create,
							Objects = new DbObject[] { col },
							Commands = CreateCommands(syntax, col)
						};

						if (col.IsForeignKey(Source, out ForeignKey fk))
						{
							fkScript.Add(new ScriptBlock()
							{
								ActionType = ActionType.Create,
								Objects = new DbObject[] { col },
								Commands = CreateCommands(syntax, fk)
							});
						}
					}
				}
			}

			foreach (var create in createOther)
			{
				yield return new ScriptBlock()
				{
					ActionType = ActionType.Create,
					Objects = new DbObject[] { create },
					Commands = CreateCommands(syntax, create)
				};				
			}

			foreach (var alter in Alter)
			{
				yield return new ScriptBlock()
				{
					ActionType = ActionType.Alter,
					Objects = new DbObject[] { alter },
					Commands = AlterCommands(syntax, alter)
				};
			}

			foreach (var drop in Drop)
			{
				yield return new ScriptBlock()
				{
					ActionType = ActionType.Drop,
					Objects = new DbObject[] { drop },
					Commands = DropCommands(syntax, drop)
				};
			}
			
			foreach (var fk in fkScript) yield return fk;
		}

		private IEnumerable<string> DropCommands(SqlSyntax syntax, DbObject drop)
		{
			var deps = drop.GetDependencies(Destination);
			foreach (var obj in deps)
			{
				foreach (var cmd in obj.DropCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
			}

			foreach (var cmd in drop.DropCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
		}

		private IEnumerable<string> AlterCommands(SqlSyntax syntax, DbObject alter)
		{
			foreach (var cmd in alter.AlterCommands(syntax, Source)) yield return syntax.ApplyDelimiters(cmd);
		}

		private IEnumerable<string> CreateCommands(SqlSyntax syntax, DbObject @object)
		{
			//yield return $"{syntax.CommentStart} table {tbl.Key} has {tbl.Key.RowCount:n0} rows, so new columns are added individually";
			foreach (var cmd in @object.CreateCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
		}

		private IEnumerable<string> RebuildCommands(SqlSyntax syntax, IGrouping<Table, Column> tblGrp, string columnList)
		{
			yield return $"{syntax.CommentStart} rebuilding empty table {tblGrp.Key} to add column(s) {columnList}";

			var deps = tblGrp.Key.GetDependencies(Destination);
			foreach (var obj in deps)
			{
				foreach (var cmd in obj.DropCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
			}

			foreach (var cmd in tblGrp.Key.AlterCommands(syntax, Source)) yield return syntax.ApplyDelimiters(cmd);

			foreach (var obj in deps)
			{
				foreach (var cmd in obj.CreateCommands(syntax)) yield return syntax.ApplyDelimiters(cmd);
			}
		}

		public IEnumerable<string> GetScriptCommands(SqlSyntax syntax)
		{
			return GetScriptBlocks(syntax).SelectMany(block => block.GetCommands(syntax));
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
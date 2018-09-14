using System;
using System.Collections.Generic;
using System.IO;

namespace SchemaSync.Library.Models
{
	[Flags]
	public enum ObjectTypeFlags
	{
		Schemas = 1, 
		Tables = 2,
		ForeignKeys = 4,
		Procedures = 8,
		Views = 16,
		All = Schemas | Tables | ForeignKeys | Procedures | Views
	}

	public class Database
	{
		public string Collation { get; set; }
		public IEnumerable<Schema> Schemas { get; set; }
		public IEnumerable<Table> Tables { get; set; }
		public IEnumerable<ForeignKey> ForeignKeys { get; set; }
		public IEnumerable<Procedure> Procedures { get; set; }
		public IEnumerable<View> Views { get; set; }
		public IEnumerable<IgnoredObject> IgnoredObjects { get; set; }

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
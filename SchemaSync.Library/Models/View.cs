﻿using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class View : DbObject
	{
		public string Name { get; set; }
		public string Body { get; set; }

		public override IEnumerable<string> AlterCommands(SqlSyntax syntax, Database database)
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			yield return $"CREATE VIEW <{ToString()}\r\nAS\r\n{Body}";
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
		{
			throw new System.NotImplementedException();
		}

		public override bool IsAltered(DbObject compare)
		{
			throw new System.NotImplementedException();
		}
	}
}
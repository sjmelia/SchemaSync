using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class Procedure : DbObject
	{
		public string Name { get; set; }
		public IEnumerable<Parameter> Parameters { get; set; } = Enumerable.Empty<Parameter>();
		public string Body { get; set; }

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{			
			string parameters = string.Join("\r\n, ", Parameters.Select(p => p.ToString()));
			yield return $"CREATE PROCEDURE <{ToString()}>\r\n{parameters}\r\nAS\r\n{Body}";
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
		{
			yield return $"DROP PROCEDURE <{Name}>";
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}
	}
}
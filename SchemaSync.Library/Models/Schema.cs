using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class Schema : DbObject
	{
		public string Name { get; set; }

		public override IEnumerable<string> CreateCommands(SqlSyntax syntax)
		{
			yield return $"CREATE SCHEMA <{Name}>";
		}

		public override IEnumerable<string> DropCommands(SqlSyntax syntax)
		{
			yield return $"DROP SCHEMA <{Name}>";
		}

		public override bool IsAltered(DbObject compare)
		{
			return false;
		}

		public override IEnumerable<DbObject> GetDependencies(Database database)
		{
			return database.Tables.Where(t => t.Schema.Equals(Name));
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			Schema test = obj as Schema;
			if (test != null) return test.Name.ToLower().Equals(Name.ToLower());
			return false;
		}

		public override int GetHashCode()
		{
			return Name.ToLower().GetHashCode();
		}
	}
}
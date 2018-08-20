using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class ForeignKey : DbObject
	{
		public string Name { get; set; }
		public Table ReferencedTable { get; set; }
		public Column ReferencingColumn { get; set; }
		public bool CascadeDelete { get; set; }
		public bool CascadeUpdate { get; set; }

		public override IEnumerable<string> AlterCommands()
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> CreateCommands()
		{
			string cmd = $"ALTER TABLE <{ReferencingColumn.Table}> ADD CONSTRAINT <{Name}> FOREIGN KEY (<{ReferencingColumn.Name}>) REFERENCES <{ReferencedTable}> (<{ReferencedTable.IdentityColumn}>)";
			if (CascadeUpdate) cmd += " ON UPDATE CASCADE";
			if (CascadeDelete) cmd += " ON DELETE CASCADE";
			yield return cmd;
		}

		public override IEnumerable<string> DropCommands()
		{
			yield return $"ALTER TABLE <{ReferencedTable}> DROP CONSTRAINT <{Name}>";
		}

		public override IEnumerable<DbObject> GetDependencies(Database database)
		{
			return Enumerable.Empty<DbObject>();
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}
	}
}
using System.Collections.Generic;

namespace SchemaSync.Library.Models
{
	public class Column : DbObject
	{
		public Table Table { get; set; }
		public string Name { get; set; }
		public string DataType { get; set; }
		public int Length { get; set; }
		public int Scale { get; set; }
		public int Precision { get; set; }
		public bool IsNullable { get; set; }
		public string Default { get; set; }

		public override IEnumerable<string> Alter()
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> Create()
		{
			throw new System.NotImplementedException();
		}

		public override IEnumerable<string> Drop()
		{
			throw new System.NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			Column col = obj as Column;
			if (col != null)
			{
				return Table.Equals(col.Table) && (Name ?? string.Empty).ToLower().Equals(col.Name.ToLower());
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Table.GetHashCode() + (Name ?? string.Empty).ToLower().GetHashCode();
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}
	}
}
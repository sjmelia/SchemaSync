using System.Collections.Generic;
using System.Linq;

namespace SchemaSync.Library.Models
{
	public class Table : DbObject
	{
		public string Schema { get; set; }
		public string Name { get; set; }
		public string IdentityColumn { get; set; }
		public string ClusteredIndex { get; set; }

		/// <summary>
		/// If empty, then it's okay to drop and rebuild table
		/// </summary>
		public bool IsEmpty { get; set; }

		public IEnumerable<Column> Columns { get; set; } = Enumerable.Empty<Column>();
		public IEnumerable<Index> Indexes { get; set; } = Enumerable.Empty<Index>();

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
			Table tbl = obj as Table;
			if (tbl != null)
			{
				return (Schema ?? string.Empty).ToLower().Equals(tbl.Schema.ToLower()) && (Name ?? string.Empty).ToLower().Equals(tbl.Name.ToLower());
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Schema ?? string.Empty).ToLower().GetHashCode() + (Name ?? string.Empty).ToLower().GetHashCode();
		}

		public override string ToString()
		{
			return (!string.IsNullOrEmpty(Schema)) ? $"{Schema}.{Name}" : Name;
		}

		public override bool IsAltered(object compare)
		{
			throw new System.NotImplementedException();
		}
	}
}
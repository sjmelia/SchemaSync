namespace SchemaSync.Library.Models
{
	public class ForeignKey
	{
		public string Name { get; set; }
		public Table ReferencedTable { get; set; }
		public Column ReferencingColumn { get; set; }
		public bool CascadeDelete { get; set; }
		public bool CascadeUpdate { get; set; }
	}
}
namespace SchemaSync.SqlServer.Models
{
	public class IndexColumnResult
	{
		public int object_id { get; set; }
		public int index_id { get; set; }
		public string name { get; set; }
		public byte key_ordinal { get; set; }
		public bool is_descending_key { get; set; }
	}

	internal class IndexKey
	{
		public int object_id { get; set; }
		public int index_id { get; set; }
	}
}
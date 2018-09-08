namespace SchemaSync.Library.Models
{
	public class IgnoredObject
	{
		public DbObject Object { get; set; }
		public string Reason { get; set; }
	}
}
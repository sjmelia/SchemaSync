namespace SchemaSync.SqlServer.Models
{
	public class SysTablesResult
	{
		public string TableName { get; set; }
		public string Schema { get; set; }
		public int ObjectId { get; set; }
	}
}
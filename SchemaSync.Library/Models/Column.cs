namespace SchemaSync.Library.Models
{
	public class Column
	{
		public Table Table { get; set; }
		public string Name { get; set; }
		public string DataType { get; set; }
		public int Length { get; set; }
		public int Scale { get; set; }
		public int Precision { get; set; }
		public bool IsNullable { get; set; }
		public string Default { get; set; }
	}
}
namespace SchemaSync.Library.Models
{
	public class Parameter
	{
		public Procedure Procedure { get; set; }
		public string Name { get; set; }
		public string DataType { get; set; }
		public string Default { get; set; }
		public bool IsOutput { get; set; }
		public int Position { get; set; }

		public override string ToString()
		{
			return $"{Name} {DataType}";
		}
	}
}
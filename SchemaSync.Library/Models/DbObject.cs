namespace SchemaSync.Library.Models
{
	public abstract class DbObject
	{
		public abstract bool IsAltered();

		/// <summary>
		/// Generates the SQL CREATE script for an object
		/// </summary>		
		public abstract string Create();

		/// <summary>
		/// Generates the SQL DROP script for an object
		/// </summary>		
		public abstract string Drop();

		/// <summary>
		/// Generates the SQL ALTER script for an object
		/// </summary>		
		public abstract string Alter();
	}
}
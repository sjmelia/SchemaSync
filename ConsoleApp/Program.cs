using SchemaSync.Library;
using SchemaSync.SqlServer;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace ConsoleApp
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (var cn = GetConnection())
			{
				var dbSource = new ConnectionProvider().GetDatabase(cn);
				//var dbSource = new ConnectionProvider().GetDatabase(Assembly.LoadFile(@"C:\Users\Adam\Source\Repos\Hs5\Hs5.Models\bin\Debug\Hs5.Models.dll"));
				//var dbDest = new ConnectionProvider(cn);				
				//var diff = SchemaComparison.Execute(dbSource, dbDest);
			}
		}

		private static SqlConnection GetConnection()
		{
			return new SqlConnection(@"Data Source=(localdb)\MSSqlLocalDb;Database=Hs5;Integrated Security=true");
		}
	}
}
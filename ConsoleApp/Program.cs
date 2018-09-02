using SchemaSync.Library.Models;
using SchemaSync.SqlServer;
using System;
using System.Data.SqlClient;
using System.IO;

namespace ConsoleApp
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (var cn = GetConnection())
			{
				var db = new SqlServerDatabase(cn);
				db.SaveScript(new SqlServerSyntax(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Hs5Script.sql"));
			}
		}

		private static SqlConnection GetConnection()
		{
			return new SqlConnection(@"Data Source=(localdb)\MSSqlLocalDb;Database=Hs5;Integrated Security=true");
		}
	}
}
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Postulate.Lite.Core.Extensions;
using SchemaSync.SqlServer;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Tests
{
	[TestClass]
	public class CreateObjects
	{
		private const string dbName = "Hs5_test";

		private SqlConnection GetConnection()
		{
			return new SqlConnection($"Data Source=(localdb)\\mssqllocaldb;Integrated Security=true;Database={dbName}");
		}

		[TestInitialize]
		public void CreateHs5Db()
		{
			using (var cn = new SqlConnection("Data Source=(localdb)\\mssqllocaldb;Integrated Security=true"))
			{
				if (cn.Exists("[sys].[databases] WHERE [name]=@name", new { name = dbName }))
				{
					cn.Execute($"DROP DATABASE [{dbName}]");
				}
			}

			string bacpac = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hs5.bacpac");
			if (File.Exists(bacpac)) File.Delete(bacpac);
			ExportResource("Tests.Resources.Hs5.bacpac", bacpac);

			// thanks to https://www.cyotek.com/blog/creating-and-restoring-bacpac-files-without-using-a-gui
			var psi = new ProcessStartInfo(@"C:\Program Files (x86)\Microsoft SQL Server\120\DAC\bin\SqlPackage.exe");
			psi.Arguments = $"/a:Import /tsn:(localdb)\\mssqllocaldb /sf:\"{bacpac}\" /tdn:{dbName}";
			var p = Process.Start(psi);
			p.WaitForExit();
		}

		private void ExportResource(string resourceName, string fileName)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				// thanks to https://stackoverflow.com/a/19982104/2023653
				using (var file = new FileStream(fileName, FileMode.Create))
				{
					stream.CopyTo(file);
				}
			}
		}

		[TestMethod]
		public void DatabaseFromConnection()
		{
			using (var cn = GetConnection())
			{
				var db = new ConnectionProvider(cn);
			}
		}
	}
}
﻿using SchemaSync.Library;
using SchemaSync.Postulate;
using SchemaSync.SqlServer;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ConsoleApp
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var dbSqlServer = new SqlServerDbProvider().GetDatabase(ConnectionString);
			var dbModel = new PostulateDbProvider().GetDatabase(@"C:\Users\Adam\Source\Repos\Hs5\Hs5.Models\bin\Debug\Hs5.Models.dll");
			var diff = SchemaComparison.Execute(dbModel, dbSqlServer);

			Console.WriteLine("Create:");
			foreach (var grp in diff.Create.GroupBy(row => row.GetType().Name))
			{
				Console.WriteLine(grp.Key);
				foreach (var item in grp)
				{
					Console.WriteLine($"- {item}");
				}
			}			

			foreach (var cmd in diff.GetScriptCommands(new SqlServerSyntax()))
			{
				Console.WriteLine(cmd);
				Console.WriteLine("GO");
				Console.WriteLine();
			}

			Console.ReadLine();
		}

		private static string ConnectionString
		{
			get { return @"Data Source=(localdb)\MSSqlLocalDb;Database=Hs5;Integrated Security=true"; }
		}

		private static SqlConnection GetConnection()
		{
			return new SqlConnection(ConnectionString);
		}
	}
}
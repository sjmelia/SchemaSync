using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaSync.MySql;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Tests.MySql
{
    [TestClass]
    public class SchemaTests
    {
        [TestMethod]
        public void SchemasShouldMatch()
        {
            const string connectionString = "server=localhost;user=root;port=3306;database=mysql;password=password";
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    using (var command = new MySqlCommand("DROP DATABASE IF EXISTS `a`; CREATE DATABASE `a`;", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new MySqlCommand("DROP DATABASE IF EXISTS `b`; CREATE DATABASE `b`;", connection))
                    { 
                        command.ExecuteNonQuery();
                    }

                    var dbProvider = new MySqlDbProvider();
                    var database = dbProvider.GetDatabase(connectionString);
                    var expected = new string[] { "a", "b" };
                    var actual = database.Schemas
                        .Select(s => s.Name)
                        .OrderBy(s => s)
                        .ToList();
                    CollectionAssert.AreEqual(expected, actual);
                }
                finally
                {
                    using (var command = new MySqlCommand("DROP DATABASE IF EXISTS `a`;", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new MySqlCommand("DROP DATABASE IF EXISTS `b`;", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}

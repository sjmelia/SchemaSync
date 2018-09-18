using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaSync.MySql;
using System.Linq;
using MySql.Data.MySqlClient;
using SchemaSync.Library.Models;

namespace Tests.MySql
{
    [TestClass]
    public class ForeignKeyTests
    {
        const string connectionString = "server=localhost;user=root;port=3306;password=password";

        private MySqlConnection connection;

        private void Execute(string sql)
        {
            using (var command = new MySqlCommand(sql, this.connection))
            {
                command.ExecuteNonQuery();
            }
        }

        [TestInitialize]
        public void Setup()
        {
            this.connection = new MySqlConnection(connectionString);
            this.connection.Open();
            this.Execute("DROP DATABASE IF EXISTS `a`;");
            this.Execute("CREATE DATABASE `a`;");
            this.Execute("USE `a`");
            this.Execute("CREATE TABLE `tablea` (`cola` INT NOT NULL AUTO_INCREMENT, `colb` VARCHAR(255), PRIMARY KEY (`cola`))");
            this.Execute("CREATE TABLE `tableb` (`cola` INT NOT NULL AUTO_INCREMENT, `colb` INT NOT NULL, `colc` INT NOT NULL DEFAULT 5, `cold` INT AS (`colb` + `colc`), PRIMARY KEY (`cola`))");
            this.Execute("CREATE INDEX `idx_b` ON `tableb` (`cola`, `colb`, `colc`)");
            this.Execute("ALTER TABLE `tableb` ADD CONSTRAINT `fk_table_a` FOREIGN KEY (`colb`) REFERENCES `tablea` (`cola`) ON DELETE CASCADE ON UPDATE CASCADE;");
        }

        [TestCleanup]
        public void Teardown()
        {
            this.connection.Dispose();
        }

        [TestMethod]
        public void ForeignKeysShouldMatch()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);

            var expected = new ForeignKey()
            {
                Name = "fk_table_a",
                CascadeDelete = true,
                CascadeUpdate = true
            };

            Assert.AreEqual(1, database.ForeignKeys.Count());
            var actual = database.ForeignKeys.Single(fk => fk.Name == "fk_table_a");
            AssertHelper.HasEqualFieldValues(expected, actual, new string[] { "ReferencedTable", "ReferencingTable", "ObjectId", "InternalId", "Columns" });

            Assert.AreEqual("tableb", actual.ReferencingTable.Name);
            Assert.AreEqual("tablea", actual.ReferencedTable.Name);
        }

        [TestMethod]
        public void ForeignKeyColumnsShouldMatch()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);

            var expected = new ForeignKey.Column()
            {
                ReferencedName = "cola",
                ReferencingName = "colb"
            };

            Assert.AreEqual(1, database.ForeignKeys.Count());
            var actual = database.ForeignKeys.Single(fk => fk.Name == "fk_table_a");
            Assert.AreEqual(1, actual.Columns.Count());
            var actualColumn = actual.Columns.First();
            AssertHelper.HasEqualFieldValues(expected, actualColumn, new string[] {  });
        }
    }
}

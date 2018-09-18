using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaSync.MySql;
using System.Linq;
using MySql.Data.MySqlClient;
using SchemaSync.Library.Models;
using System.Collections.Generic;

namespace Tests.MySql
{
    [TestClass]
    public class TableTests
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
        }

        [TestCleanup]
        public void Teardown()
        {
            this.connection.Dispose();
        }

        [TestMethod]
        public void TableShouldMatch()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);

            Table expected = new Table()
            {
                Schema = "a",
                Name = "tablea",
                IdentityColumn = "cola",
                ClusteredIndex = "PRIMARY",
                RowCount = 0,
                ObjectId = 0,
            };

            Assert.AreEqual(2, database.Tables.Count());
            var actual = database.Tables.First();
            AssertHelper.HasEqualFieldValues(expected, actual, new string[] { "Columns", "InternalId", "Indexes" });
        }

        [TestMethod]
        public void GenerationExpression()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);
            var tableb = database.Tables.Skip(1).First();
            var column = tableb.Columns.First(c => c.Name == "cold");
            Assert.AreEqual("(`colb` + `colc`)", column.Expression);
        }

        [TestMethod]
        public void ColumnDefault()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);
            var tableb = database.Tables.Skip(1).First();
            var column = tableb.Columns.First(c => c.Name == "colc");
            Assert.AreEqual("5", column.Default);
        }

        [TestMethod]
        public void ColumnsShouldMatch()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);
            
            Column expectedFirstColumn = new Column()
            {
                Name = "cola",
                DataType = "int(11)",
                IsNullable = false,
                Collation = null,
                MaxLength = 0,
                Precision = 10,
                Scale = 0,
                Expression = ""
            };

            Column expectedSecondColumn = new Column()
            {
                Name = "colb",
                DataType = "varchar(255)",
                IsNullable = true,
                Collation = "utf8mb4_0900_ai_ci",
                MaxLength = 255,
                Precision = 0,
                Scale = 0,
                Expression = ""
            };

            var actualTable = database.Tables.First();
            var actualFirstColumn = actualTable.Columns.First();
            AssertHelper.HasEqualFieldValues(expectedFirstColumn, actualFirstColumn,
                new string[] { "Table" });

            var actualSecondColumn = actualTable.Columns.Skip(1).First();
            AssertHelper.HasEqualFieldValues(expectedSecondColumn, actualSecondColumn,
                new string[] { "Table" });

        }

        [TestMethod]
        public void IndexesShouldMatch()
        {
            var dbProvider = new MySqlDbProvider();
            var database = dbProvider.GetDatabase(connectionString);

            Index expectedFirstIndex = new Index()
            {
                Name = "PRIMARY",
                Type = IndexType.PrimaryKey
            };

            Index expectedSecondIndex = new Index()
            {
                Name = "idx_b",
                Type = IndexType.NonUnique
            };

            var actualTable = database.Tables.Skip(1).First();
            var actualFirstIndex = actualTable.Indexes.Single(i => i.Name == "PRIMARY");
            AssertHelper.HasEqualFieldValues(expectedFirstIndex, actualFirstIndex,
                new string[] { "Table", "Columns", "IsClustered" });

            var actualSecondIndex = actualTable.Indexes.Single(i => i.Name == "idx_b");
            AssertHelper.HasEqualFieldValues(expectedSecondIndex, actualSecondIndex,
                new string[] { "Table", "Columns", "IsClustered" });
        }
    }
}

using Dapper;
using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SchemaSync.MySql
{
	public partial class MySqlDbProvider
	{
		private IEnumerable<Table> GetTables(IDbConnection connection)
        {
            // InnoDB index type enum: https://dev.mysql.com/doc/refman/8.0/en/innodb-indexes-table.html
            var tables = connection.Query<Table>(@"SELECT 
                `inno`.`TABLE_ID` AS `InternalId`,
                `tables`.`TABLE_NAME` AS `Name`,
                `tables`.`TABLE_SCHEMA` AS `Schema`,
                `inno_idx`.`NAME` AS `ClusteredIndex`,
                `tables`.`TABLE_ROWS` AS `RowCount`
                FROM `information_schema`.`tables` AS `tables`
                LEFT OUTER JOIN `information_schema`.`innodb_tables` AS `inno` ON `inno`.`NAME` = CONCAT(`tables`.`TABLE_SCHEMA`, '/', `tables`.`TABLE_NAME`)
                LEFT OUTER JOIN `information_schema`.`innodb_indexes` AS `inno_idx` ON 
                    `inno`.`table_id` = `inno_idx`.`table_id` AND 
                    (`inno_idx`.`Type` = 1 OR `inno_idx`.`Type` = 3)

                WHERE `TABLE_SCHEMA` NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
                GROUP BY `inno`.`TABLE_ID`, `tables`.`TABLE_NAME`, `tables`.`TABLE_SCHEMA`, `inno_idx`.`NAME`, `tables`.`TABLE_ROWS`;");

            var columnLookup = GetColumns(connection);
            var indexLookup = GetIndexes(connection);
            foreach (var table in tables)
            {
                // Pick the first only, as could be more than one, replicates mssql behaviour
                table.IdentityColumn = connection.QueryFirst<string>(@"SELECT `column_name` FROM `information_schema`.`columns`
                    WHERE `table_name` = @TableName AND `extra` = 'auto_increment';",
                    new { TableName = table.Name });

                var tableId = table.Schema + "/" + table.Name;

                table.Columns = columnLookup[tableId];
                foreach (var column in table.Columns)
                {
                    column.Table = table;
                }

                table.Indexes = indexLookup[tableId];
                foreach (var index in table.Indexes)
                {
                    index.Table = table;
                }
            }

            return tables;
        }

        private static ILookup<string, MySqlColumn> GetColumns(IDbConnection connection)
        {
            var columns = connection.Query<MySqlColumn>(@"SELECT
                CONCAT(`TABLE_SCHEMA`, '/', `TABLE_NAME`) AS `TableId`, 
                `COLUMN_NAME` AS `Name`,
                `COLUMN_TYPE` AS `DataType`,
                CASE 
                    WHEN `IS_NULLABLE` = 'NO' THEN false
                    WHEN `IS_NULLABLE` = 'YES' THEN true
                END AS `IsNullable`,
                `COLUMN_DEFAULT` AS `Default`,
                `COLLATION_NAME` AS `Collation`,
                `CHARACTER_MAXIMUM_LENGTH` AS `MaxLength`,
                `NUMERIC_PRECISION` AS `Precision`,
                `NUMERIC_SCALE` AS `Scale`,
                0 AS `InternalId`,
                `GENERATION_EXPRESSION` AS `Expression` 
                FROM `information_schema`.`columns`");

            var columnLookup = columns.ToLookup(row => row.TableId);
            return columnLookup;
        }

        private ILookup<string, MySqlIndex> GetIndexes(IDbConnection connection)
        {
            var mySqlIndexes = connection.Query<MySqlDenormalisedIndex>(@"SELECT
                    `TABLE_SCHEMA` AS `TableSchema`,
                    `TABLE_NAME` AS `TableName`,
                    `INDEX_NAME` AS `IndexName`,
                    CASE
                        WHEN INDEX_NAME = 'PRIMARY' THEN 1
                        WHEN NON_UNIQUE = 0 THEN 2
                        WHEN NON_UNIQUE = 1 THEN 4
                    END AS `IndexType`,
                    `COLUMN_NAME` AS `ColumnName`,
                    `SEQ_IN_INDEX` AS `ColumnPosition`
                    FROM `information_schema`.`statistics`");

            var indexes = mySqlIndexes
                .GroupBy(i => new
                {
                    i.TableSchema,
                    i.TableName,
                    i.IndexName,
                    i.IndexType
                })
                .Select(ig => new MySqlIndex()
                {
                    TableId = ig.Key.TableSchema + "/" + ig.Key.TableName,
                    Name = ig.Key.IndexName,
                    Type = ig.Key.IndexType,
                    Columns = ig.Select(c => new IndexColumn()
                    {
                        Name = c.ColumnName,
                        Position = c.ColumnPosition
                    })
                });

            return indexes.ToLookup(row => row.TableId);
        }
    }
}
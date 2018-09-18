using Dapper;
using SchemaSync.Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SchemaSync.MySql
{
	public partial class MySqlDbProvider
	{
		private IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection)
		{
            var foreignKeys = connection.Query<MySqlDenormalisedForeignKey>(@"SELECT
                rc.`CONSTRAINT_NAME` AS `ConstraintName`,
                rc.`CONSTRAINT_SCHEMA` AS `ReferencingSchema`,
                rc.`TABLE_NAME` AS `ReferencingTable`,
                rc.`CONSTRAINT_SCHEMA` AS `ReferencedSchema`,
                rc.`REFERENCED_TABLE_NAME` AS `ReferencedTableName`,
                CASE
	                WHEN rc.`UPDATE_RULE` = 'CASCADE' THEN 1
	                ELSE 0
                END AS `CascadeUpdate`,
                CASE
	                WHEN rc.`DELETE_RULE` = 'CASCADE' THEN 1
	                ELSE 0
                END AS `CascadeDelete`,
                kcu.`COLUMN_NAME` AS `ReferencingName`,
                kcu.`REFERENCED_COLUMN_NAME` AS `ReferencedName`
                FROM `INFORMATION_SCHEMA`.`REFERENTIAL_CONSTRAINTS` rc
                JOIN `INFORMATION_SCHEMA`.`KEY_COLUMN_USAGE` kcu ON 
	                rc.`CONSTRAINT_SCHEMA` = kcu.`CONSTRAINT_SCHEMA` AND
	                rc.`CONSTRAINT_NAME` = kcu.`CONSTRAINT_NAME`;");

            return foreignKeys.GroupBy(fk => new
            {
                fk.ConstraintName,
                fk.ReferencingSchema,
                fk.ReferencingTable,
                fk.ReferencedSchema,
                fk.ReferencedTableName,
                fk.CascadeUpdate,
                fk.CascadeDelete
            }).Select(fkg => new ForeignKey()
            {
                Name = fkg.Key.ConstraintName,
                ReferencedTable = new Table() { Schema = fkg.Key.ReferencedSchema, Name = fkg.Key.ReferencedTableName },
                ReferencingTable = new Table() { Schema = fkg.Key.ReferencingSchema, Name = fkg.Key.ReferencingTable },
                CascadeDelete = fkg.Key.CascadeDelete,
                CascadeUpdate = fkg.Key.CascadeUpdate,
                Columns = fkg.Select(fkgc => new ForeignKey.Column()
                {
                    ReferencedName = fkgc.ReferencedName,
                    ReferencingName = fkgc.ReferencingName
                })
            });
		}
	}
}
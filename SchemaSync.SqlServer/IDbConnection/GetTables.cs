using Dapper;
using SchemaSync.Library.Models;
using SchemaSync.SqlServer.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SchemaSync.SqlServer
{
	public partial class SqlServerDatabase
	{
		protected override IEnumerable<Table> GetTables(IDbConnection connection)
		{
			var tables = connection.Query<Table>(
				@"WITH [clusteredIndexes] AS (
					SELECT [name], [object_id] FROM [sys].[indexes] WHERE [type_desc]='CLUSTERED'
				), [identityColumns] AS (
					SELECT [object_id], [name] FROM [sys].[columns] WHERE [is_identity]=1
				) SELECT
					[t].[name] AS [Name],
					SCHEMA_NAME([t].[schema_id]) AS [Schema],
					[t].[object_id] AS [ObjectId],
					[c].[name] AS [ClusteredIndex],
					[i].[name] AS [IdentityColumn],
					(SELECT SUM(row_count) FROM [sys].[dm_db_partition_stats] WHERE [object_id]=[t].[object_id] AND [index_id] IN (0, 1)) AS [RowCount]
				FROM
					[sys].[tables] [t]
					LEFT JOIN [clusteredIndexes] [c] ON [t].[object_id]=[c].[object_id]
					LEFT JOIN [identityColumns] [i] ON [t].[object_id]=[i].[object_id]");

			var columns = connection.Query<Column>(
				@"SELECT
					[col].[object_id] AS [ObjectId],
					[col].[name] AS [Name],
					TYPE_NAME([system_type_id]) AS [DataType],
					[is_nullable] AS [IsNullable],
					[def].[definition]  AS [Default],
					[col].[collation_name] AS [Collation],
					[col].[max_length] AS [MaxLength],
					[col].[precision] AS [Precision],
					[col].[scale] AS [Scale],
					[col].[column_id] AS [InternalId]
				FROM
					[sys].[columns] [col]
					INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
					LEFT JOIN [sys].[default_constraints] [def] ON [col].[default_object_id]=[def].[object_id]
				WHERE
					[t].[type_desc]='USER_TABLE'");

			var indexes = connection.Query<Index>(
				@"SELECT
					[x].[object_id] AS [ObjectId],
					[x].[name] AS [Name],
					CONVERT(bit, CASE
						WHEN [x].[type_desc]='CLUSTERED' THEN 1
						ELSE 0
					END) AS [IsClustered],
					CASE
						WHEN [x].[is_primary_key]=1 THEN 1
						WHEN [x].[is_unique]=1 AND [x].[is_unique_constraint]=0 THEN 2
						WHEN [x].[is_unique_constraint]=1 THEN 3
						WHEN [x].[is_unique]=0 THEN 4
					END AS [Type],
					[x].[index_id] AS [InternalId]
				FROM
					[sys].[indexes] [x]
					INNER JOIN [sys].[tables] [t] ON [x].[object_id]=[t].[object_id]
				WHERE
					[t].[type_desc]='USER_TABLE' AND
					[x].[type]<>0");

			var indexCols = connection.Query<IndexColumnResult>(
				@"SELECT
					[xcol].[object_id],
					[xcol].[index_id],
					[col].[name],
					[xcol].[key_ordinal],
					[xcol].[is_descending_key]
				FROM
					[sys].[index_columns] [xcol]
					INNER JOIN [sys].[indexes] [x] ON [xcol].[object_id]=[x].[object_id] AND [xcol].[index_id]=[x].[index_id]
					INNER JOIN [sys].[columns] [col] ON [xcol].[object_id]=[col].[object_id] AND [xcol].[column_id]=[col].[column_id]
					INNER JOIN [sys].[tables] [t] ON [x].[object_id]=[t].[object_id]
				WHERE
					[t].[type_desc]='USER_TABLE'");

			var columnLookup = columns.ToLookup(row => row.ObjectId);
			var indexLookup = indexes.ToLookup(row => row.ObjectId);
			var indexColLookup = indexCols.ToLookup(row => new IndexKey() { object_id = row.object_id, index_id = row.index_id });

			foreach (var x in indexes)
			{
				var indexKey = new IndexKey() { object_id = x.ObjectId, index_id = x.InternalId };
				x.Columns = indexColLookup[indexKey].Select(row => new IndexColumn()
				{
					Name = row.name,
					Position = row.key_ordinal,
					SortDirection = (row.is_descending_key) ? SortDirection.Descending : SortDirection.Ascending
				});
			}

			foreach (var t in tables)
			{
				t.Columns = columnLookup[t.ObjectId].ToArray();
				foreach (var col in t.Columns) col.Table = t;

				t.Indexes = indexLookup[t.ObjectId].ToArray();
				foreach (var x in t.Indexes) x.Table = t;
			}

			return tables;
		}
	}
}
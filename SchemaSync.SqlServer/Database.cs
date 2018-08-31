using Dapper;
using SchemaSync.Library.Models;
using SchemaSync.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchemaSync.SqlServer
{
	public class Database : Library.Models.Database
	{
		protected override DatabaseSourceFlags SupportedSources => DatabaseSourceFlags.Assembly | DatabaseSourceFlags.Connection;

		protected override ObjectTypeFlags SupportedObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		protected override IEnumerable<Type> GetModelTypes(Assembly assembly)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<ForeignKey> GetForeignKeys(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<Procedure> GetProcedures(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<Procedure> GetProcedures(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

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
					[i].[name] AS [IdentityColumn]
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
					[column_id] AS [Position]
				FROM 
					[sys].[columns] [col]
					INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
					LEFT JOIN [sys].[default_constraints] [def] ON [col].[default_object_id]=[def].[object_id]
				WHERE
					[t].[type_desc]='USER_TABLE'");

			var columnLookup = columns.ToLookup(row => row.ObjectId);

			/*
			SELECT 
				[x].[object_id] AS [ObjectId]
				[x].*
			FROM 
				[sys].[indexes] [x]
				INNER JOIN [sys].[tables] [t] ON [x].[object_id]=[t].[object_id]
			WHERE
				[t].[type_desc]='USER_TABLE'
			*/

			throw new NotImplementedException();
		}

		protected override IEnumerable<Table> GetTables(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<View> GetViews(IDbConnection connection)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<View> GetViews(IEnumerable<Type> modelTypes)
		{
			throw new NotImplementedException();
		}

		protected override Column GetColumnFromProperty(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		protected override Table GetTableFromType(Type modelType)
		{
			throw new NotImplementedException();
		}
	}
}
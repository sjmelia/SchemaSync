using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace SchemaSync.Postulate
{
	public class AssemblyProvider : IDbProviderFromAssembly
	{
		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		public Database GetDatabase(Assembly assembly)
		{
			var types = assembly.GetExportedTypes();

			var db = new Database();
			db.Tables = GetTables(types);
			db.ForeignKeys = GetForeignKeys(db.Tables);
			return db;
		}

		private IEnumerable<Table> GetTables(Type[] types)
		{
			return types
				.Where(t => !t.HasAttribute<NotMappedAttribute>())
				.Select(t => new Table()
				{
					Schema = GetTableSchema(t),
					Name = GetTableName(t),
					IdentityColumn = GetIdentityColumn(t),
					Columns = GetColumns(t),
					Indexes = GetIndexes(t),
					ClusteredIndex = GetClusteredIndex(t)
				});
		}

		private string GetClusteredIndex(Type t)
		{
			return $"PK_{GetConstraintName(t)}";
		}

		private IEnumerable<Index> GetIndexes(Type t)
		{
			string constraintName = GetConstraintName(t);
			string identityCol = GetIdentityColumn(t);

			var pkColumns = GetMappedColumns(t).Where(pi => pi.HasAttribute<PrimaryKeyAttribute>());
			if (pkColumns.Any())
			{
				yield return new Index()
				{
					Name = $"PK_{constraintName}",
					Type = IndexType.PrimaryKey,
					Columns = pkColumns.Select((pi, i) => new IndexColumn() { Name = GetColumnName(pi), Position = i })
				};

				yield return new Index()
				{
					Name = $"U_{constraintName}_{identityCol}",
					Type = IndexType.UniqueConstraint,
					Columns = new IndexColumn[] { new IndexColumn() { Name = identityCol, Position = 1 } }
				};
			}
			else
			{
				yield return new Index()
				{
					Name = $"PK_{constraintName}",
					Type = IndexType.PrimaryKey,
					Columns = new IndexColumn[] { new IndexColumn() { Name = identityCol, Position = 1 } }
				};
			}
		}

		private string GetConstraintName(Type t)
		{
			return GetTableSchema(t) + GetTableName(t);
		}

		private IEnumerable<PropertyInfo> GetMappedColumns(Type t)
		{
			return t.GetProperties().Where(pi => !pi.HasAttribute<NotMappedAttribute>());
		}

		private string GetColumnName(PropertyInfo pi)
		{
			return (pi.HasAttribute(out ColumnAttribute attr, (a) => !string.IsNullOrEmpty(a.Name))) ? attr.Name : pi.Name;
		}

		private IEnumerable<Column> GetColumns(Type t)
		{
			return GetMappedColumns(t).Select(pi => ColumnFromProperty(pi));
		}

		private Column ColumnFromProperty(PropertyInfo pi)
		{
			return new Column()
			{
				Name = GetColumnName(pi),
				DataType = "whatever"
			};
		}

		private string GetIdentityColumn(Type t)
		{
			const string defaultIdentity = "Id";
			try
			{
				return (t.HasAttribute(out IdentityAttribute attr)) ? attr.PropertyName : t.GetProperty(defaultIdentity).Name;
			}
			catch (Exception exc)
			{
				throw new Exception($"Couldn't determine the Identity column of type {t.Name}, and property named '{defaultIdentity}' not found.", exc);
			}			
		}

		private string GetTableName(Type t)
		{
			return (t.HasAttribute(out TableAttribute attr, (a) => !string.IsNullOrEmpty(a.Name))) ? attr.Name : t.Name;
		}

		private string GetTableSchema(Type t)
		{
			return (t.HasAttribute(out TableAttribute attr, (a) => !string.IsNullOrEmpty(a.Schema))) ? attr.Schema : string.Empty;
		}

		private IEnumerable<ForeignKey> GetForeignKeys(IEnumerable<Table> tables)
		{
			throw new NotImplementedException();
		}
	}
}
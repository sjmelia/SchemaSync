using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.SqlServer;
using SchemaSync.Library;
using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace SchemaSync.Postulate
{
	public partial class PostulateDbProvider : IDbProviderFromAssembly
	{		
		private SqlServerIntegrator _integrator = new SqlServerIntegrator();
		private List<IgnoredTypeInfo> _ignoredTypes = null;

		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;		

		public Database GetDatabase(string path)
		{
			var assembly = Assembly.LoadFrom(path);
			return GetDatabase(assembly);
		}

		public string DefaultSchema { get; set; } = "dbo";

		public IEnumerable<IgnoredTypeInfo> IgnoredTypes
		{
			get { return _ignoredTypes; }
		}

		public Database GetDatabase(Assembly assembly)
		{			
			var types = assembly.GetExportedTypes();
			var typeTableMap = GetTypeTableDictionary(types);

			var db = new Database();
			db.Tables = typeTableMap.Select(kp => kp.Value);
			db.ForeignKeys = GetForeignKeys(typeTableMap);
			return db;
		}

		private Dictionary<Type, Table> GetTypeTableDictionary(Type[] types)
		{
			_ignoredTypes = new List<IgnoredTypeInfo>();

			var rules = GetTypeExcludeRules();

			foreach (var rule in rules)
			{
				_ignoredTypes.AddRange(
					types.Where(t => rule.Rule.Invoke(t))
					.Select(t => new IgnoredTypeInfo() { Type = t, Reason = rule.Description })
				);
			}

			var source = types
				.Where(t => !_ignoredTypes.Any(it => it.Type.Equals(t)))
				.Select(t => new
				{
					Type = t,
					Table = new Table()
					{
						Schema = GetTableSchema(t),
						Name = GetTableName(t),
						IdentityColumn = t.GetIdentityName(),
						Columns = GetColumns(t),
						Indexes = GetIndexes(t),
						ClusteredIndex = GetClusteredIndex(t)
					}
				}).ToList();

			foreach (var item in source)
			{
				foreach (var col in item.Table.Columns) col.Table = item.Table;
				foreach (var ndx in item.Table.Indexes) ndx.Table = item.Table;
			}

			return source.ToDictionary(row => row.Type, row => row.Table);
		}

		private bool HasIdentityProperty(Type t)
		{
			try
			{
				var pi = t.GetIdentityProperty();
				return (pi != null);
			}
			catch 
			{
				return false;
			}
		}

		private string GetClusteredIndex(Type t)
		{
			return $"PK_{GetConstraintName(t)}";
		}

		private IEnumerable<Index> GetIndexes(Type t)
		{
			string constraintName = GetConstraintName(t);
			string identityCol = t.GetIdentityName();

			var pkColumns = _integrator.GetMappedColumns(t).Where(pi => pi.HasAttribute<PrimaryKeyAttribute>());
			if (pkColumns.Any())
			{
				yield return new Index()
				{
					Name = $"PK_{constraintName}",
					Type = IndexType.PrimaryKey,
					Columns = pkColumns.Select((pi, i) => new IndexColumn() { Name = pi.GetColumnName(), Position = i })
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
			string schema = GetTableSchema(t);
			if (schema.Equals(DefaultSchema)) schema = string.Empty;
			return schema + GetTableName(t);
		}

		private IEnumerable<Column> GetColumns(Type t)
		{
			return _integrator.GetMappedColumns(t).Select(pi => ColumnFromProperty(pi)).ToList();
		}

		private Column ColumnFromProperty(PropertyInfo pi)
		{
			var scale = pi.GetAttribute<DecimalPrecisionAttribute>();
			return new Column()
			{
				Name = pi.GetColumnName(),
				DataType = _integrator.FindTypeInfo(pi.PropertyType).BaseName,
				IsNullable = GetPropertyIsNullable(pi),
				MaxLength = GetPropertyMaxLength(pi),
				Scale = (scale?.Scale ?? 0),
				Precision = (scale?.Precision ?? 0)				
			};
		}

		private int GetPropertyMaxLength(PropertyInfo pi)
		{
			if (pi.HasAttribute(out MaxLengthAttribute attr))
			{
				return attr.Length;
			}
			return -1;
		}

		private bool GetPropertyIsNullable(PropertyInfo pi)
		{
			if (pi.HasAttribute<RequiredAttribute>())
			{
				return false;
			}
			else
			{
				return pi.PropertyType.IsNullable();
			}				
		}
		
		private string GetTableName(Type t)
		{
			return (t.HasAttribute(out TableAttribute attr, (a) => !string.IsNullOrEmpty(a.Name))) ? attr.Name : t.Name;
		}

		private string GetTableSchema(Type t)
		{
			if (t.HasAttribute(out SchemaAttribute schemaAttr)) return schemaAttr.Name;
			if (t.HasAttribute(out TableAttribute tableAttr, (a) => !string.IsNullOrEmpty(a.Schema))) return tableAttr.Schema;
			return DefaultSchema;
		}

		private IEnumerable<ForeignKey> GetForeignKeys(Dictionary<Type, Table> typesAndTables)
		{
			var fkProps = typesAndTables.Select(kp => kp.Key)
				.SelectMany(t => t.GetProperties()
					.Where(pi => pi.HasAttribute<ReferencesAttribute>()));

			return fkProps.Select(pi => ForeignKeyFromProperty(typesAndTables, pi)).ToList();
		}

		private ForeignKey ForeignKeyFromProperty(Dictionary<Type, Table> typesAndTables, PropertyInfo pi)
		{
			var fk = pi.GetCustomAttribute<ReferencesAttribute>();
			return new ForeignKey()
			{
				Name = $"FK_{GetConstraintName(pi.DeclaringType)}_{pi.GetColumnName()}",
				CascadeDelete = fk.CascadeDelete,
				ReferencedTable = typesAndTables[fk.PrimaryType],
				ReferencingTable = typesAndTables[pi.DeclaringType],
				Columns = new ForeignKey.Column[] { new ForeignKey.Column() { ReferencingName = pi.GetColumnName(), ReferencedName = pi.DeclaringType.GetIdentityName() } }
			};
		}		
	}
}
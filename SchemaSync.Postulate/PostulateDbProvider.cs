using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.SqlServer;
using SchemaSync.Library.Interfaces;
using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SchemaSync.Postulate
{
	public partial class PostulateDbProvider : IDbAssemblyProvider
	{
		private SqlServerIntegrator _integrator = new SqlServerIntegrator();
		private List<IgnoredObject> _ignoredObjects = null;

		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		public Database GetDatabase(string path)
		{
			var assembly = Assembly.LoadFrom(path);
			return GetDatabase(assembly);
		}

		public string DefaultSchema { get; set; } = "dbo";

		public Database GetDatabase(Assembly assembly)
		{
			var types = assembly.GetExportedTypes();
			var typeTableMap = GetTypeTableDictionary(types);

			var db = new Database();
			db.Tables = typeTableMap.Select(kp => kp.Value);
			db.ForeignKeys = GetForeignKeys(typeTableMap);
			db.IgnoredObjects = _ignoredObjects;
			return db;
		}

		private Dictionary<Type, Table> GetTypeTableDictionary(Type[] types)
		{
			_ignoredObjects = new List<IgnoredObject>();
			var rules = GetTypeExcludeRules(_integrator);

			foreach (var rule in rules)
			{
				_ignoredObjects.AddRange(
					types.Where(t => rule.Rule.Invoke(t))
					.Select(t => new IgnoredObject() { Object = new Table() { Name = t.Name, SourceType = t }, Reason = rule.Description })
				);
			}

			var source = types
				.Where(t => !_ignoredObjects.Any(obj => obj.Object.SourceType.Equals(t)))
				.Select(t => new
				{
					Type = t,
					Table = new Table()
					{
						Schema = GetTableSchema(t),
						Name = GetTableName(t),
						IdentityColumn = t.GetIdentityName(),
						Columns = GetColumns(t),
						Indexes = GetIndexes(t).ToList(),
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

				var identity = t.GetIdentityProperty();
				if (identity != null)
				{
					yield return new Index()
					{
						Name = $"U_{constraintName}_{identityCol}",
						Type = IndexType.UniqueConstraint,
						Columns = new IndexColumn[] { new IndexColumn() { Name = identityCol, Position = 1 } }
					};
				}
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

			var uniqueColumns = _integrator.GetMappedColumns(t).Where(pi => pi.HasAttribute<UniqueKeyAttribute>());
			foreach (var col in uniqueColumns)
			{
				yield return new Index()
				{
					Name = $"U_{constraintName}_{col.GetColumnName()}",
					Type = IndexType.UniqueConstraint,
					Columns = new IndexColumn[] { new IndexColumn() { Name = col.GetColumnName(), Position = 1 } }
				};
			}

			var attr = t.GetAttribute<UniqueKeyAttribute>();
			if (attr != null)
			{
				var columnNames = string.Join("_", attr.ColumnNames);
				yield return new Index()
				{
					Name = $"U_{constraintName}_{columnNames}",
					Type = IndexType.UniqueConstraint,
					Columns = attr.ColumnNames.Select((col, index) => new IndexColumn() { Name = col, Position = index })
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
				DataType = GetDataType(pi, ref scale),
				IsNullable = GetPropertyIsNullable(pi),
				MaxLength = GetPropertyMaxLength(pi),
				Scale = (scale?.Scale ?? 0),
				Precision = (scale?.Precision ?? 0),
				Expression = GetCalculationExpression(pi)
			};
		}

		private string GetCalculationExpression(PropertyInfo pi)
		{
			if (pi.HasAttribute(out CalculatedAttribute attr))
			{
				return "(" + attr.Expression + ")";
			}
			return null;
		}

		private string GetDataType(PropertyInfo pi, ref DecimalPrecisionAttribute decimalAttr)
		{
			if (pi.HasAttribute(out ColumnAttribute attr, a => !string.IsNullOrEmpty(a.TypeName)))
			{
				const string scalePrecisionPattern = @"\(\d,(\s*)\d\)";
				string result = attr.TypeName;
				if (decimalAttr == null && Regex.IsMatch(result.ToLower(), $@"decimal{scalePrecisionPattern}"))
				{
					var scalePrecision = Regex.Match(result, scalePrecisionPattern).Value.Substring(1);
					scalePrecision = scalePrecision.Substring(0, scalePrecision.Length - 1);
					var parts = scalePrecision.Split(',').Select(s => s.Trim()).Select(s => Convert.ToByte(s)).ToArray();
					result = "decimal";
					decimalAttr = new DecimalPrecisionAttribute(parts[0], parts[1]);
				}
				return result;
			}
			else
			{
				return _integrator.FindTypeInfo(pi.PropertyType).BaseName;
			}
		}

		private int GetPropertyMaxLength(PropertyInfo pi)
		{
			if (pi.HasAttribute(out MaxLengthAttribute attr)) return attr.Length;
			return -1;
		}

		private bool GetPropertyIsNullable(PropertyInfo pi)
		{
			if (pi.HasAttribute<RequiredAttribute>() || pi.HasAttribute<PrimaryKeyAttribute>())
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
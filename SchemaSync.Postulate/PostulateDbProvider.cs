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

namespace SchemaSync.Postulate
{
	public class PostulateDbProvider : IDbProviderFromAssembly
	{		
		private SqlServerIntegrator _integrator = new SqlServerIntegrator();

		public ObjectTypeFlags ObjectTypes => ObjectTypeFlags.Tables | ObjectTypeFlags.ForeignKeys;

		public Database GetDatabase(string path)
		{
			var assembly = Assembly.LoadFrom(path);
			return GetDatabase(assembly);
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
			var source = types
				.Where(t => !t.HasAttribute<NotMappedAttribute>())
				.Select(t => new
				{
					Type = t,
					Table = new Table()
					{
						Schema = GetTableSchema(t),
						Name = GetTableName(t),
						IdentityColumn = TryGetIdentityName(t),
						Columns = GetColumns(t),
						Indexes = GetIndexes(t),
						ClusteredIndex = GetClusteredIndex(t)
					}
				});

			return source.ToDictionary(row => row.Type, row => row.Table);
		}

		private string TryGetIdentityName(Type t)
		{
			try
			{
				return t.GetIdentityName();
			}
			catch (Exception exc)
			{
				throw;
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

			var pkColumns = GetMappedColumns(t).Where(pi => pi.HasAttribute<PrimaryKeyAttribute>());
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
			return GetTableSchema(t) + GetTableName(t);
		}

		private IEnumerable<PropertyInfo> GetMappedColumns(Type t)
		{
			return t.GetProperties().Where(pi => !pi.HasAttribute<NotMappedAttribute>());
		}

		private IEnumerable<Column> GetColumns(Type t)
		{
			return GetMappedColumns(t).Select(pi => ColumnFromProperty(pi));
		}

		private Column ColumnFromProperty(PropertyInfo pi)
		{
			var scale = pi.GetAttribute<DecimalPrecisionAttribute>();
			return new Column()
			{
				Name = pi.GetColumnName(),
				DataType = _integrator.SupportedTypes(0, 0, 0)[pi.PropertyType].BaseName,
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

		private IEnumerable<ForeignKey> GetForeignKeys(Dictionary<Type, Table> typesAndTables)
		{
			return typesAndTables.Select(kp => kp.Key)
				.SelectMany(t => t.GetProperties()
					.Where(pi => pi.HasAttribute<ReferencesAttribute>()))
					.Select(pi => ForeignKeyFromProperty(typesAndTables, pi));
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
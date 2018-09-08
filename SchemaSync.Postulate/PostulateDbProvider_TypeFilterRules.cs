using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SchemaSync.Postulate
{
	public partial class PostulateDbProvider
	{
		private static IEnumerable<TypeExcludeRule> GetTypeExcludeRules(SqlIntegrator integrator)
		{
			return new TypeExcludeRule[]
			{
				new TypeExcludeRule()
				{
					Rule = (t) => { return t.HasAttribute<NotMappedAttribute>(); },
					Description = "Has the [NotMapped] attribute"
				},
				new TypeExcludeRule()
				{
					Rule = (t) => { return t.IsAbstract; },
					Description = "Abstract classes not supported"
				},
				new TypeExcludeRule()
				{
					Rule = (t) => { return t.IsEnum; },
					Description = "Enums not supported"
				},
				new TypeExcludeRule()
				{
					Rule = (t) => { return !HasPrimaryKeyOrIdentity(integrator, t); },
					Description = "No [PrimaryKey] properties, Id property, nor [Identity] attribute found"
				}
			};
		}

		internal class TypeExcludeRule
		{
			public Func<Type, bool> Rule { get; set; }
			public string Description { get; set; }
		}

		private static bool HasPrimaryKeyOrIdentity(SqlIntegrator integrator, Type t)
		{
			try
			{
				var pi = t.GetIdentityProperty();
				if (pi != null) return true;

				var pkCol = integrator.GetMappedColumns(t).Where(prop => prop.HasAttribute<PrimaryKeyAttribute>());
				return pkCol.Any();
			}
			catch
			{
				return false;
			}
		}
	}
}
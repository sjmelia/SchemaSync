using Postulate.Lite.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchemaSync.Postulate
{
	public partial class PostulateDbProvider
	{
		private IEnumerable<TypeExcludeRule> GetTypeExcludeRules()
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
					Rule = (t) => { return !HasIdentityProperty(t); },
					Description = "No Id property nor [Identity] attribute found"
				},
				new TypeExcludeRule()
				{
					Rule = (t) => { return t.IsAbstract; },
					Description = "Class is abstract"
				}
			};
		}

		internal class TypeExcludeRule
		{
			public Func<Type, bool> Rule { get; set; }
			public string Description { get; set; }
		}
	}
}
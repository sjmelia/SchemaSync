using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace SchemaSync.Library
{
	public abstract class SqlSyntax
	{
		/// <summary>
		/// Indicates whether object names are prefixed with the table schema
		/// </summary>
		public abstract bool SupportsSchemas { get; }

		/// <summary>
		/// Allows SQL Server to assume "dbo" as default schema
		/// </summary>
		public abstract string DefaultSchema { get; }

		/// <summary>
		/// Starting object name delimiter
		/// </summary>
		public abstract char StartDelimiter { get; }

		/// <summary>
		/// Ending object name delimiter
		/// </summary>
		public abstract char EndDelimiter { get; }

		/// <summary>
		/// Syntax for autoincrement/identity columns
		/// </summary>
		public abstract string IdentitySyntax { get; }

		public abstract string GetDataTypeDefinition(Column column);

		/// <summary>
		/// For example GO in T-SQL
		/// </summary>
		public abstract string BatchSeparator { get; }

		/// <summary>
		/// Applies start and end object name delimiters around names marked with open and close brackets in the specified input
		/// </summary>
		public string ApplyDelimiters(string input)
		{
			string result = input;
			var objectNames = Regex.Matches(input, @"(?<!<)(<[^<\r\n]*>)(?!<)");
			foreach (Match name in objectNames)
			{
				// strip start and end brackets
				string parsedName = name.Value.Substring(1, name.Value.Length - 2);
				string[] parts = parsedName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				string delimitedName = string.Join(".", parts.Select(s => $"{StartDelimiter}{s}{EndDelimiter}"));
				result = result.Replace(name.Value, delimitedName);
			}
			return result;
		}

		/// <summary>
		/// Override this to inject any database-level commands into a script, such as schema creation
		/// </summary>
		public virtual IEnumerable<string> DatabaseCommands(Database database)
		{
			return Enumerable.Empty<string>();
		}
	}
}
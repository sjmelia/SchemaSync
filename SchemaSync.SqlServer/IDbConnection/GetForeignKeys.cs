using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace SchemaSync.SqlServer
{
	public partial class SqlServerDatabase
	{
		protected override IEnumerable<ForeignKey> GetForeignKeys(IDbConnection connection)
		{
			throw new NotImplementedException();
		}
	}
}
using SchemaSync.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaSync.Library
{
	public class ScriptBlock
	{
		public IEnumerable<DbObject> Objects { get; set; }
		public IEnumerable<string> Commands { get; set; }
	}
}

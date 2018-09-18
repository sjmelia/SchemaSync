using SchemaSync.Library.Models;

namespace SchemaSync.MySql
{
    /// <summary>
    /// Class to represent MySql specific column info.
    /// </summary>
    class MySqlColumn : Column
    {
        /// <summary>
        /// Gets or sets the full name of the table.
        /// </summary>
        /// <remarks>
        /// Integer internal Ids are engine specific; e.g. InnoDB or MyISAM.
        /// So we use the &lt;schema&gt;\&lt;table name&gt;
        /// format to uniquely identify a table, for later joining to columns etc.
        /// </remarks>
        public string TableId { get; set; }
    }
}

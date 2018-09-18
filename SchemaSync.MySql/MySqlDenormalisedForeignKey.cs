namespace SchemaSync.MySql
{
    class MySqlDenormalisedForeignKey
    {
        public string ConstraintName { get; set; }
        public string ReferencingSchema { get; set; }
        public string ReferencingTable { get; set; }
        public string ReferencedSchema { get; set; }
        public string ReferencedTableName { get; set; }
        public bool CascadeUpdate { get; set; }
        public bool CascadeDelete { get; set; }
        public string ReferencingName { get; set; }
        public string ReferencedName { get; set; }
    }
}

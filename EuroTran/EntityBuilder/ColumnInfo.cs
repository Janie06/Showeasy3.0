namespace EntityBuilder
{
    public class ColumnInfo
    {
        public string DbColumnName { get; set; }
        public string DataType { get; set; }
        public string IsPrimarykey { get; set; }
        public string IsIdentity { get; set; }
        public string IsNullable { get; set; }
    }
}
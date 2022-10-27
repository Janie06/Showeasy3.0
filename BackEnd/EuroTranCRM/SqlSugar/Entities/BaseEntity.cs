namespace SqlSugar.Entities
{
    public class BaseEntity

    {
        [SugarColumn(IsIgnore = true)]
        public int RowIndex { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string CreateUserName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ModifyUserName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ExFeild1 { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ExFeild2 { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ExFeild3 { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ExFeild4 { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ExFeild5 { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ExFeild6 { get; set; }

        [SugarColumn(IsIgnore = true)]
        public bool ExFeild7 { get; set; }
    }
}
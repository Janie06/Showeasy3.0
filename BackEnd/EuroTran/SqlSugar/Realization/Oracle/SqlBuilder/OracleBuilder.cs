namespace SqlSugar
{
    public class OracleBuilder : SqlBuilderProvider
    {
        public override string SqlParameterKeyWord
        {
            get
            {
                return ":";
            }
        }
        public override string SqlDateNow
        {
            get
            {
                return "sysdate";
            }
        }
        public override string FullSqlDateNow
        {
            get
            {
                return "select sysdate from dual";
            }
        }
        public override string SqlTranslationLeft { get { return "\""; } }
        public override string SqlTranslationRight { get { return "\""; } }
        public override string GetTranslationTableName(string name)
        {
            var result= base.GetTranslationTableName(name);
            return result.ToUpper();
        }
        public override string GetTranslationColumnName(string entityName, string propertyName)
        {
            var result = base.GetTranslationColumnName(entityName, propertyName);
            return result.ToUpper();
        }
        public override string GetTranslationColumnName(string propertyName)
        {
            var result = base.GetTranslationColumnName(propertyName);
            return result.ToUpper();
        }

    }
}

namespace SqlSugar
{
    public partial class SqlServerExpressionContext : ExpressionContext, ILambdaExpressions
    {
        public SqlSugarClient Context { get; set; }
        public SqlServerExpressionContext()
        {
            base.DbMehtods = new SqlServerMethod();
        }

    }
    public partial class SqlServerMethod : DefaultDbMethod, IDbMethods
    {

    }
}

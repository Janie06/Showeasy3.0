using System;

namespace SqlSugar
{
    public class SqlFuncExternal
    {
        public string UniqueMethodName { get; set; }
        public Func<MethodCallExpressionModel, DbType, ExpressionContext, string> MethodValue { get; set; }
    }
}

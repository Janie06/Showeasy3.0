using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace SqlSugar
{
    public partial interface ILambdaExpressions
    {
        MappingColumnList MappingColumns { get; set; }
        MappingTableList MappingTables { get; set; }
        IgnoreColumnList IgnoreComumnList { get; set; }
        List<SqlFuncExternal> SqlFuncServices { get; set; }

        List<JoinQueryInfo> JoinQueryInfos { get; set; }
        bool IsSingle { get; set; }
        SqlSugarClient Context { get; set; }
        IDbMethods DbMehtods { get; set; }
        Expression Expression { get; set; }
        int Index { get; set; }
        int ParameterIndex { get; set; }
        List<SugarParameter> Parameters { get; set; }
        ExpressionResult Result { get; set; }
        string SqlParameterKeyWord { get; }
        string SingleTableNameSubqueryShortName { get; set; }
         Action<Type> InitMappingInfo { get; set; }
         Action RefreshMapping { get; set; }

        string GetAsString(string fieldName, string fieldValue);
        void Resolve(Expression expression, ResolveExpressType resolveType);
        void Clear();
    }
}

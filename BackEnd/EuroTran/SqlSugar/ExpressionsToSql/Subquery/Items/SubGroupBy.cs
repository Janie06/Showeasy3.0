using System.Linq;
using System.Linq.Expressions;

namespace SqlSugar
{
    public class SubGroupBy : ISubOperation
    {
        public string Name
        {
            get { return "GroupBy"; }
        }

        public Expression Expression
        {
            get; set;
        }

        public int Sort
        {
            get
            {
                return 479;
            }
        }

        public ExpressionContext Context
        {
            get; set;
        }

        public string GetValue(Expression expression)
        {
            var exp = expression as MethodCallExpression;
            var argExp = exp.Arguments[0];
            var result = "GROUP BY " + SubTools.GetMethodValue(this.Context, argExp, ResolveExpressType.FieldSingle);
            var selfParameterName = this.Context.GetTranslationColumnName((argExp as LambdaExpression).Parameters.First().Name) + UtilConstants.Dot;
            result = result.Replace(selfParameterName, string.Empty);
            return result;
        }
    }
}

using System.Linq;
using System.Linq.Expressions;

namespace SqlSugar
{
    public class SubWhere: ISubOperation
    {
        public string Name
        {
            get { return "Where"; }
        }

        public Expression Expression
        {
            get; set;
        }

        public int Sort
        {
            get
            {
                return 400;
            }
        }

        public ExpressionContext Context
        {
            get;set;
        }

        public string GetValue(Expression expression)
        {
            var exp = expression as MethodCallExpression;
            var argExp= exp.Arguments[0];
            var result= "WHERE "+SubTools.GetMethodValue(Context, argExp, ResolveExpressType.WhereMultiple);;
            var selfParameterName = Context.GetTranslationColumnName((argExp as LambdaExpression).Parameters.First().Name)+UtilConstants.Dot;
            result = result.Replace(selfParameterName,string.Empty);
            return result;
        }
    }
}

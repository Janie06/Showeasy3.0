using System.Linq.Expressions;

namespace SqlSugar
{
    public class SubSum:ISubOperation
    {
        public string Name
        {
            get
            {
                return "Sum";
            }
        }

        public Expression Expression
        {
            get; set;
        }


        public int Sort
        {
            get
            {
                return 200;
            }
        }

        public ExpressionContext Context
        {
            get; set;
        }

        public string GetValue(Expression expression = null)
        {
            var exp = expression as MethodCallExpression;
            return "SUM("+SubTools.GetMethodValue(this.Context, exp.Arguments[0], ResolveExpressType.FieldSingle)+")";
        }
    }
}

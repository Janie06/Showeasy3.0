using System.Linq.Expressions;

namespace SqlSugar
{
    public class NewArrayExpessionResolve : BaseResolve
    {
        public NewArrayExpessionResolve(ExpressionParameter parameter) : base(parameter)
        {
            var expression = (NewArrayExpression)base.Expression;
            switch (base.Context.ResolveType)
            {
                case ResolveExpressType.WhereSingle:
                case ResolveExpressType.WhereMultiple:
                case ResolveExpressType.SelectSingle:
                case ResolveExpressType.SelectMultiple:
                case ResolveExpressType.FieldSingle:
                case ResolveExpressType.FieldMultiple:
                    Check.ThrowNotSupportedException("NewArrayExpression");
                    break;
                case ResolveExpressType.ArraySingle:
                    foreach (var item in expression.Expressions)
                    {
                        base.Expression = item;
                        base.Start();
                    }
                    break;
                case ResolveExpressType.Join:
                    base.Context.ResolveType = ResolveExpressType.WhereMultiple;
                    var i = 0;
                    foreach (var item in expression.Expressions)
                    {
                        if (item is UnaryExpression)
                        {
                            base.Expression = item;
                            base.Start();
                            if (parameter.CommonTempData is JoinType)
                            {
                                if (i > 0)
                                {
                                    base.Context.Result.Append("," + parameter.CommonTempData.ObjToString() + ",");
                                }
                                else
                                {
                                    base.Context.Result.Append(parameter.CommonTempData.ObjToString() + ",");
                                }
                                ++i;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

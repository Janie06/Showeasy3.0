using System;
using System.Linq.Expressions;

namespace SqlSugar
{
    public class MemberConstExpressionResolve : BaseResolve
    {
        public MemberConstExpressionResolve(ExpressionParameter parameter) : base(parameter)
        {
            var expression = base.Expression as MemberExpression;
            var isLeft = parameter.IsLeft;
            var value = ExpressionTool.GetMemberValue(expression.Member, expression);
            var baseParameter = parameter.BaseParameter;
            var isSetTempData = baseParameter.CommonTempData.HasValue() && baseParameter.CommonTempData.Equals(CommonTempDataType.Result);
            switch (parameter.Context.ResolveType)
            {
                case ResolveExpressType.Update:
                case ResolveExpressType.SelectSingle:
                case ResolveExpressType.SelectMultiple:
                    if (value != null && value.GetType().IsEnum())
                    {
                        value = Convert.ToInt64(value);
                    }
                    parameter.BaseParameter.CommonTempData = value;
                    break;

                case ResolveExpressType.WhereSingle:
                case ResolveExpressType.WhereMultiple:
                    if (isSetTempData)
                    {
                        baseParameter.CommonTempData = value;
                    }
                    else
                    {
                        AppendValue(parameter, isLeft, value);
                    }
                    break;

                case ResolveExpressType.FieldSingle:
                case ResolveExpressType.FieldMultiple:
                    break;

                default:
                    break;
            }
        }
    }
}
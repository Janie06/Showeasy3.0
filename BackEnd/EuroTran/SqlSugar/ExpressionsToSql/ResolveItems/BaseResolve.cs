using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace SqlSugar
{
    public class BaseResolve
    {
        protected Expression Expression { get; set; }
        protected Expression ExactExpression { get; set; }
        public ExpressionContext Context { get; set; }
        public bool? IsLeft { get; set; }
        public int ContentIndex { get { return this.Context.Index; } }
        public int Index { get; set; }
        public ExpressionParameter BaseParameter { get; set; }

        private BaseResolve()
        {

        }
        public BaseResolve(ExpressionParameter parameter)
        {
            this.Expression = parameter.CurrentExpression;
            this.Context = parameter.Context;
            this.BaseParameter = parameter;
        }

        public BaseResolve Start()
        {
            Context.Index++;
            var expression = this.Expression;
            var parameter = new ExpressionParameter
            {
                Context = this.Context,
                CurrentExpression = expression,
                IsLeft = this.IsLeft,
                BaseExpression = this.ExactExpression,
                BaseParameter = this.BaseParameter,
                Index = Context.Index
            };
            if (expression is LambdaExpression)
            {
                return new LambdaExpressionResolve(parameter);
            }
            else if (expression is BinaryExpression)
            {
                return new BinaryExpressionResolve(parameter);
            }
            else if (expression is BlockExpression)
            {
                Check.ThrowNotSupportedException("BlockExpression");
            }
            else if (expression is ConditionalExpression)
            {
                return new ConditionalExpressionResolve(parameter);
            }
            else if (expression is MethodCallExpression)
            {
                return new MethodCallExpressionResolve(parameter);
            }
            else if (expression is MemberExpression && ((MemberExpression)expression).Expression == null)
            {
                return new MemberNoExpressionResolve(parameter);
            }
            else if (expression is MemberExpression && ((MemberExpression)expression).Expression.NodeType == ExpressionType.Constant)
            {
                return new MemberConstExpressionResolve(parameter);
            }
            else if (expression is MemberExpression && ((MemberExpression)expression).Expression.NodeType == ExpressionType.New)
            {
                return new MemberNewExpressionResolve(parameter);
            }
            else if (expression is ConstantExpression)
            {
                return new ConstantExpressionResolve(parameter);
            }
            else if (expression is MemberExpression)
            {
                return new MemberExpressionResolve(parameter);
            }
            else if (expression is UnaryExpression)
            {
                return new UnaryExpressionResolve(parameter);
            }
            else if (expression is MemberInitExpression)
            {
                return new MemberInitExpressionResolve(parameter);
            }
            else if (expression is NewExpression)
            {
                return new NewExpressionResolve(parameter);
            }
            else if (expression is NewArrayExpression)
            {
                return new NewArrayExpessionResolve(parameter);
            }
            else if (expression is ParameterExpression)
            {
                return new TypeParameterExpressionReolve(parameter);
            }
            else if (expression != null && expression.NodeType.IsIn(ExpressionType.NewArrayBounds))
            {
                Check.ThrowNotSupportedException("ExpressionType.NewArrayBounds");
            }
            return null;
        }

        protected void AppendMember(ExpressionParameter parameter, bool? isLeft, object appendValue)
        {

            Context.ParameterIndex++;
            if (isLeft == true)
            {
                appendValue += ExpressionConst.ExpressionReplace + parameter.BaseParameter.Index;
            }
            if (this.Context.Result.Contains(ExpressionConst.FormatSymbol))
            {
                this.Context.Result.Replace(ExpressionConst.FormatSymbol, appendValue.ObjToString());
            }
            else
            {
                this.Context.Result.Append(appendValue);
            }
        }
        protected void AppendValue(ExpressionParameter parameter, bool? isLeft, object value)
        {
            if (parameter.BaseExpression is BinaryExpression || parameter.BaseExpression == null)
            {
                var oppoSiteExpression = isLeft == true ? parameter.BaseParameter.RightExpression : parameter.BaseParameter.LeftExpression;
                if (parameter.CurrentExpression is MethodCallExpression)
                {
                    var appendValue = value;
                    if (this.Context.Result.Contains(ExpressionConst.FormatSymbol))
                    {
                        this.Context.Result.Replace(ExpressionConst.FormatSymbol, appendValue.ObjToString());
                    }
                    else
                    {
                        this.Context.Result.Append(appendValue);
                    }
                    this.AppendOpreator(parameter, isLeft);
                }
                else if (oppoSiteExpression is MemberExpression)
                {
                    var appendValue = Context.SqlParameterKeyWord
                        + ((MemberExpression)oppoSiteExpression).Member.Name
                        + Context.ParameterIndex;
                    if (value.ObjToString() != "NULL" && !parameter.ValueIsNull)
                    {
                        this.Context.Parameters.Add(new SugarParameter(appendValue, value));
                    }
                    else
                    {
                        appendValue = value.ObjToString();
                    }
                    Context.ParameterIndex++;
                    appendValue = string.Format(" {0} ", appendValue);
                    if (isLeft == true)
                    {
                        appendValue += ExpressionConst.ExpressionReplace + parameter.BaseParameter.Index;
                    }
                    if (this.Context.Result.Contains(ExpressionConst.FormatSymbol))
                    {
                        this.Context.Result.Replace(ExpressionConst.FormatSymbol, appendValue);
                    }
                    else
                    {
                        this.Context.Result.Append(appendValue);
                    }
                }
                else if ((oppoSiteExpression is UnaryExpression && (oppoSiteExpression as UnaryExpression).Operand is MemberExpression)) {
                    var appendValue = Context.SqlParameterKeyWord
                      + ((MemberExpression)(oppoSiteExpression as UnaryExpression).Operand).Member.Name
                      + Context.ParameterIndex;
                    if (value.ObjToString() != "NULL" && !parameter.ValueIsNull)
                    {
                        this.Context.Parameters.Add(new SugarParameter(appendValue, value));
                    }
                    else
                    {
                        appendValue = value.ObjToString();
                    }
                    Context.ParameterIndex++;
                    appendValue = string.Format(" {0} ", appendValue);
                    if (isLeft == true)
                    {
                        appendValue += ExpressionConst.ExpressionReplace + parameter.BaseParameter.Index;
                    }
                    if (this.Context.Result.Contains(ExpressionConst.FormatSymbol))
                    {
                        this.Context.Result.Replace(ExpressionConst.FormatSymbol, appendValue);
                    }
                    else
                    {
                        this.Context.Result.Append(appendValue);
                    }
                }
                else
                {
                    var appendValue = this.Context.SqlParameterKeyWord + ExpressionConst.Const + Context.ParameterIndex;
                    Context.ParameterIndex++;
                    if (value != null && value.GetType().IsEnum())
                    {
                        value = Convert.ToInt64(value);
                    }
                    this.Context.Parameters.Add(new SugarParameter(appendValue, value));
                    appendValue = string.Format(" {0} ", appendValue);
                    if (isLeft == true)
                    {
                        appendValue += ExpressionConst.ExpressionReplace + parameter.BaseParameter.Index;
                    }
                    if (this.Context.Result.Contains(ExpressionConst.FormatSymbol))
                    {
                        this.Context.Result.Replace(ExpressionConst.FormatSymbol, appendValue);
                    }
                    else
                    {
                        this.Context.Result.Append(appendValue);
                    }
                }
            }
        }
        protected void AppendOpreator(ExpressionParameter parameter, bool? isLeft)
        {
            if (isLeft == true)
            {
                this.Context.Result.Append(" " + ExpressionConst.ExpressionReplace + parameter.BaseParameter.Index);
            }
        }
        protected string AppendParameter(object paramterValue)
        {
            var parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
            this.Context.ParameterIndex++; ;
            this.Context.Parameters.Add(new SugarParameter(parameterName, paramterValue));
            return parameterName;
        }
        protected void AppendNot(object Value)
        {
            var isAppend = !this.Context.Result.Contains(ExpressionConst.FormatSymbol);
            var lastCharIsSpace = this.Context.Result.LastCharIsSpace;
            if (isAppend)
            {
                this.Context.Result.Append(lastCharIsSpace?"NOT":" NOT");
            }
            else
            {
                this.Context.Result.Replace(ExpressionConst.FormatSymbol, "NOT");
            }
        }

        protected MethodCallExpressionArgs GetMethodCallArgs(ExpressionParameter parameter, Expression item)
        {
            var newContext = this.Context.GetCopyContext();
            newContext.MappingColumns = this.Context.MappingColumns;
            newContext.MappingTables = this.Context.MappingTables;
            newContext.IgnoreComumnList = this.Context.IgnoreComumnList;
            newContext.SqlFuncServices = this.Context.SqlFuncServices;
            newContext.Resolve(item, this.Context.IsJoin ? ResolveExpressType.WhereMultiple : ResolveExpressType.WhereSingle);
            this.Context.Index = newContext.Index;
            this.Context.ParameterIndex = newContext.ParameterIndex;
            if (newContext.Parameters.HasValue())
            {
                this.Context.Parameters.AddRange(newContext.Parameters);
            }
            if (newContext.SingleTableNameSubqueryShortName.HasValue())
            {
                this.Context.SingleTableNameSubqueryShortName = newContext.SingleTableNameSubqueryShortName;
            }
            var methodCallExpressionArgs = new MethodCallExpressionArgs
            {
                IsMember = true,
                MemberName = newContext.Result.GetResultString()
            };
            return methodCallExpressionArgs;
        }

        protected string GetNewExpressionValue(Expression item)
        {
            var newContext = this.Context.GetCopyContext();
            newContext.Resolve(item, this.Context.IsJoin ? ResolveExpressType.WhereMultiple : ResolveExpressType.WhereSingle);
            this.Context.Index = newContext.Index;
            this.Context.ParameterIndex = newContext.ParameterIndex;
            if (newContext.Parameters.HasValue())
            {
                this.Context.Parameters.AddRange(newContext.Parameters);
            }
            return newContext.Result.GetResultString();
        }

        protected void ResolveNewExpressions(ExpressionParameter parameter, Expression item, string asName)
        {
            if (item is ConstantExpression)
            {
                this.Expression = item;
                this.Start();
                var parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                this.Context.ParameterIndex++;
                parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                this.Context.Parameters.Add(new SugarParameter(parameterName, parameter.CommonTempData));
            }
            else if ((item is MemberExpression) && ((MemberExpression)item).Expression == null)
            {
                var paramterValue = ExpressionTool.GetPropertyValue(item as MemberExpression);
                var parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                this.Context.ParameterIndex++;
                parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                this.Context.Parameters.Add(new SugarParameter(parameterName, paramterValue));
            }
            else if ((item is MemberExpression) && ((MemberExpression)item).Expression.NodeType == ExpressionType.Constant)
            {
                this.Expression = item;
                this.Start();
                var parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                this.Context.ParameterIndex++;
                parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                this.Context.Parameters.Add(new SugarParameter(parameterName, parameter.CommonTempData));
            }
            else if (item is MemberExpression)
            {
                if (!this.Context.Result.IsLockCurrentParameter)
                {
                    this.Context.Result.CurrentParameter = parameter;
                    this.Context.Result.IsLockCurrentParameter = true;
                    parameter.IsAppendTempDate();
                    this.Expression = item;
                    this.Start();
                    parameter.IsAppendResult();
                    this.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
                    this.Context.Result.CurrentParameter = null;
                }
            }
            else if (item is UnaryExpression && ((UnaryExpression)item).Operand is MemberExpression)
            {
                if (!this.Context.Result.IsLockCurrentParameter)
                {
                    var expression = ((UnaryExpression)item).Operand as MemberExpression;
                    if (expression.Expression == null)
                    {
                        this.Context.Result.CurrentParameter = parameter;
                        this.Context.Result.IsLockCurrentParameter = true;
                        parameter.IsAppendTempDate();
                        this.Expression = item;
                        this.Start();
                        parameter.IsAppendResult();
                        this.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
                        this.Context.Result.CurrentParameter = null;
                    }
                    else if (expression.Expression is ConstantExpression)
                    {
                        var parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                        this.Context.ParameterIndex++;
                        parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                        this.Context.Parameters.Add(new SugarParameter(parameterName, ExpressionTool.GetMemberValue(expression.Member, expression)));
                    }
                    else
                    {
                        this.Context.Result.CurrentParameter = parameter;
                        this.Context.Result.IsLockCurrentParameter = true;
                        parameter.IsAppendTempDate();
                        this.Expression = item;
                        this.Start();
                        parameter.IsAppendResult();
                        this.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
                        this.Context.Result.CurrentParameter = null;
                    }
                }
            }
            else if (item is UnaryExpression && ((UnaryExpression)item).Operand is ConstantExpression)
            {
                if (!this.Context.Result.IsLockCurrentParameter)
                {
                    this.Expression = ((UnaryExpression)item).Operand;
                    this.Start();
                    var parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                    this.Context.ParameterIndex++;
                    parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                    this.Context.Parameters.Add(new SugarParameter(parameterName, parameter.CommonTempData));
                }
            }
            else if (item is BinaryExpression)
            {
                if (!this.Context.Result.IsLockCurrentParameter)
                {
                    var newContext = this.Context.GetCopyContextWithMapping();
                    var resolveExpressType = this.Context.IsSingle ? ResolveExpressType.WhereSingle : ResolveExpressType.WhereMultiple;
                    newContext.Resolve(item, resolveExpressType);
                    this.Context.Index = newContext.Index;
                    this.Context.ParameterIndex = newContext.ParameterIndex;
                    if (newContext.Parameters.HasValue())
                    {
                        this.Context.Parameters.AddRange(newContext.Parameters);
                    }
                    this.Context.Result.Append(this.Context.GetAsString(asName, newContext.Result.GetString()));
                    this.Context.Result.CurrentParameter = null;
                }
            }
            else if (item.Type.IsClass())
            {
                this.Expression = item;
                this.Start();
                var shortName = parameter.CommonTempData;
                var listProperties = item.Type.GetProperties().Cast<PropertyInfo>().ToList();
                foreach (var property in listProperties)
                {
                    var hasIgnore = this.Context.IgnoreComumnList != null && this.Context.IgnoreComumnList.Any(it => it.EntityName.Equals(item.Type.Name, StringComparison.CurrentCultureIgnoreCase) && it.PropertyName.Equals(property.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (hasIgnore)
                    {
                        continue;
                    }
                    if (property.PropertyType.IsClass())
                    {

                    }
                    else
                    {
                        var fieldName = property.Name;
                        var mappingInfo = this.Context.MappingColumns.FirstOrDefault(it => it.EntityName == item.Type.Name && it.PropertyName.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase));
                        if (mappingInfo.HasValue()) {
                            fieldName = mappingInfo.DbColumnName;
                        }
                        asName = this.Context.GetTranslationText(item.Type.Name + "." + fieldName);
                        var columnName = property.Name;
                        if (Context.IsJoin)
                        {
                            this.Context.Result.Append(Context.GetAsString(asName, columnName, shortName.ObjToString()));
                        }
                        else
                        {
                            this.Context.Result.Append(Context.GetAsString(asName, columnName));
                        }
                    }
                }
            }
            else if (item is MethodCallExpression|| item is UnaryExpression)
            {
                this.Expression = item;
                this.Start();
                parameter.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
            }
            else
            {
                Check.ThrowNotSupportedException(item.GetType().Name);
            }
        }
    }
}

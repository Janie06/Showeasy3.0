using System.Linq;
using System.Linq.Expressions;

namespace SqlSugar
{
    public class SubFromTable : ISubOperation
    {
        public string Name
        {
            get
            {
                return @"Subqueryable";
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
                return 300;
            }
        }

        public ExpressionContext Context
        {
            get; set;
        }

        public string GetValue(Expression expression)
        {
            var exp = expression as MethodCallExpression;
            var resType = exp.Method.ReturnType;
            var entityType = resType.GetGenericArguments().First();
            var name = entityType.Name;
            if (this.Context.InitMappingInfo != null)
            {
                this.Context.InitMappingInfo(entityType);
                this.Context.RefreshMapping();
            }
            return "FROM " + this.Context.GetTranslationTableName(name, true);
        }
    }
}
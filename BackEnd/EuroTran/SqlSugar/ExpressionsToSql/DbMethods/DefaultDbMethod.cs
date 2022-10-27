using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public partial class DefaultDbMethod : IDbMethods
    {
        public virtual string IIF(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            return $"( CASE  WHEN {parameter.MemberName} THEN {parameter2.MemberName}  ELSE {parameter3.MemberName} END )";
        }

        public virtual string IsNullOrEmpty(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"( {parameter.MemberName}='' OR {parameter.MemberName} IS NULL )";
        }

        public virtual string HasValue(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"( {parameter.MemberName}<>'' AND {parameter.MemberName} IS NOT NULL )";
        }

        public virtual string HasNumber(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"( {parameter.MemberName}>0 AND {parameter.MemberName} IS NOT NULL )";
        }

        public virtual string ToUpper(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" (UPPER({parameter.MemberName})) ";
        }

        public virtual string ToLower(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" (LOWER({parameter.MemberName})) ";
        }

        public virtual string Trim(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" (rtrim(ltrim({parameter.MemberName}))) ";
        }

        public virtual string Contains(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" ({parameter.MemberName} like '%'+{parameter2.MemberName}+'%') ";
        }

        public virtual string ContainsArray(MethodCallExpressionModel model)
        {
            var inValueIEnumerable = (IEnumerable)model.Args[0].MemberValue;
            var inValues = new List<object>();
            if (inValueIEnumerable != null)
            {
                foreach (var item in inValueIEnumerable)
                {
                    if (item != null && item.GetType().IsEnum())
                    {
                        inValues.Add(Convert.ToInt64(item));
                    }
                    else
                    {
                        inValues.Add(item);
                    }
                }
            }
            var value = model.Args[1].MemberName;
            string inValueString = null;
            if (inValues != null && inValues.Count > 0)
            {
                inValueString = inValues.ToArray().ToJoinSqlInVals();
            }
            if (inValueString.IsNullOrEmpty())
            {
                return " (1=2) ";
            }
            else
            {
                return $" ({value} IN ({inValueString})) ";
            }
        }

        public virtual string Equals(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" ({parameter.MemberName} = {parameter2.MemberName}) "; ;
        }

        public virtual string DateIsSameDay(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" (DATEDIFF(day,{parameter.MemberName},{parameter2.MemberName})=0) "; ;
        }

        public virtual string DateIsSameByType(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            return $" (DATEDIFF({parameter3.MemberValue},{parameter.MemberName},{parameter2.MemberName})=0) ";
        }

        public virtual string DateAddByType(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            return $" (DATEADD({parameter3.MemberValue},{parameter2.MemberName},{parameter.MemberName})) ";
        }

        public virtual string DateAddDay(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" (DATEADD(day,{parameter2.MemberName},{parameter.MemberName})) ";
        }

        public virtual string Between(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter1 = model.Args[1];
            var parameter2 = model.Args[2];
            return $" ({parameter.MemberName} BETWEEN {parameter1.MemberName} AND {parameter2.MemberName}) ";
        }

        public virtual string StartsWith(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" ({parameter.MemberName} like {parameter2.MemberName}+'%') ";
        }

        public virtual string EndsWith(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" ({parameter.MemberName} like '%'+{parameter2.MemberName}) ";
        }

        public virtual string DateValue(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return $" DateName({parameter2.MemberValue},{parameter.MemberName}) ";
        }

        public virtual string ToInt32(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS INT)";
        }

        public virtual string ToInt64(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS BIGINT)";
        }

        public virtual string ToString(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS NVARCHAR(MAX))";
        }

        public virtual string ToGuid(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS UNIQUEIDENTIFIER)";
        }

        public virtual string ToDouble(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS FLOAT)";
        }

        public virtual string ToBool(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS BIT)";
        }

        public virtual string ToDate(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS DATETIME)";
        }

        public virtual string ToTime(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS TIME)";
        }

        public virtual string ToDecimal(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" CAST({parameter.MemberName} AS MONEY)";
        }

        public virtual string Substring(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            return $"SUBSTRING({parameter.MemberName},1 + {parameter2.MemberName},{parameter3.MemberName})";
        }

        public virtual string Length(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"LEN({parameter.MemberName})";
        }

        public virtual string Replace(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            return $"REPLACE({parameter.MemberName},{parameter2.MemberName},{parameter3.MemberName})";
        }

        public virtual string AggregateSum(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"SUM({parameter.MemberName})";
        }

        public virtual string AggregateAvg(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"AVG({parameter.MemberName})";
        }

        public virtual string AggregateMin(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"MIN({parameter.MemberName})";
        }

        public virtual string AggregateMax(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"MAX({parameter.MemberName})";
        }

        public virtual string AggregateCount(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $"COUNT({parameter.MemberName})";
        }

        public virtual string MappingColumn(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter1 = model.Args[1];
            return $"{parameter1.MemberValue}";
        }

        public virtual string IsNull(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter1 = model.Args[1];
            return $"ISNULL({parameter.MemberName},{parameter1.MemberName})";
        }

        public virtual string True()
        {
            return "( 1 = 1 ) ";
        }

        public virtual string False()
        {
            return "( 1 = 2 ) ";
        }

        public string GuidNew()
        {
            return "'" + Guid.NewGuid() + "' ";
        }

        public string GetSelfAndAutoFill(string shortName, bool isSingle)
        {
            if (isSingle) return "*";
            else
                return $"{shortName}.*";
        }

        public virtual string MergeString(params string[] strings)
        {
            return string.Join("+", strings);
        }

        public virtual string Pack(string sql)
        {
            return "(" + sql + ")";
        }

        public virtual string EqualTrue(string fieldName)
        {
            return "( " + fieldName + "=1 )";
        }

        public virtual string Null()
        {
            return "NULL";
        }

        public virtual string GetDate()
        {
            return "GETDATE()";
        }

        public virtual string CaseWhen(List<KeyValuePair<string, string>> sqls)
        {
            var reslut = new StringBuilder();
            foreach (var item in sqls)
            {
                if (item.Key == "IF")
                {
                    reslut.AppendFormat(" ( CASE  WHEN {0} ", item.Value);
                }
                else if (item.Key == "End")
                {
                    reslut.AppendFormat("ELSE {0} END )", item.Value);
                }
                else if (item.Key == "Return")
                {
                    reslut.AppendFormat(" THEN {0} ", item.Value);
                }
                else
                {
                    reslut.AppendFormat(" WHEN {0} ", item.Value);
                }
            }
            return reslut.ToString();
        }
    }
}
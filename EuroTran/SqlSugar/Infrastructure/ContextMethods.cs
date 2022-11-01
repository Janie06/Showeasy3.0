﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlSugar
{
    public class ContextMethods : IContextMethods
    {
        public SqlSugarClient Context { get; set; }

        #region DataReader

        /// <summary>
        ///DataReader to Dynamic
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public ExpandoObject DataReaderToExpandoObject(IDataReader reader)
        {
            var result = new ExpandoObject();
            var dic = ((IDictionary<string, object>)result);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    var addItem = reader.GetValue(i);
                    if (addItem == DBNull.Value)
                        addItem = null;
                    dic.Add(reader.GetName(i), addItem);
                }
                catch
                {
                    dic.Add(reader.GetName(i), null);
                }
            }
            return result;
        }

        /// <summary>
        ///DataReader to Dynamic List
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public List<ExpandoObject> DataReaderToExpandoObjectList(IDataReader reader)
        {
            using (reader)
            {
                var result = new List<ExpandoObject>();
                if (reader != null && !reader.IsClosed)
                {
                    while (reader.Read())
                    {
                        result.Add(DataReaderToExpandoObject(reader));
                    }
                }
                return result;
            }
        }

        /// <summary>
        ///DataReader to DataReaderToDictionary
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public Dictionary<string, object> DataReaderToDictionary(IDataReader reader)
        {
            var result = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    var addItem = reader.GetValue(i);
                    if (addItem == DBNull.Value)
                        addItem = null;
                    result.Add(reader.GetName(i), addItem);
                }
                catch
                {
                    result.Add(reader.GetName(i), null);
                }
            }
            return result;
        }

        /// <summary>
        /// DataReaderToDynamicList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public List<T> DataReaderToDynamicList<T>(IDataReader reader)
        {
            using (reader)
            {
                var tType = typeof(T);
                var classProperties = tType.GetProperties().ToList();
                var reval = new List<T>();
                if (reader != null && !reader.IsClosed)
                {
                    while (reader.Read())
                    {
                        var readerValues = DataReaderToDictionary(reader);
                        var result = new Dictionary<string, object>();
                        foreach (var item in classProperties)
                        {
                            var name = item.Name;
                            var typeName = tType.Name;
                            if (item.PropertyType.IsClass())
                            {
                                result.Add(name, DataReaderToDynamicList_Part(readerValues, item, reval));
                            }
                            else
                            {
                                if (readerValues.Any(it => it.Key.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    var addValue = readerValues.ContainsKey(name) ? readerValues[name] : readerValues[name.ToUpper()];
                                    if (addValue == DBNull.Value)
                                    {
                                        if (item.PropertyType.IsIn(UtilConstants.IntType, UtilConstants.DecType, UtilConstants.DobType, UtilConstants.ByteType))
                                        {
                                            addValue = 0;
                                        }
                                        else if (item.PropertyType == UtilConstants.GuidType)
                                        {
                                            addValue = Guid.Empty;
                                        }
                                        else if (item.PropertyType == UtilConstants.DateType)
                                        {
                                            addValue = DateTime.MinValue;
                                        }
                                        else
                                        {
                                            addValue = null;
                                        }
                                    }
                                    else if (item.PropertyType == UtilConstants.IntType)
                                    {
                                        addValue = Convert.ToInt32(addValue);
                                    }
                                    result.Add(name, addValue);
                                }
                            }
                        }
                        var stringValue = SerializeObject(result);
                        reval.Add((T)DeserializeObject<T>(stringValue));
                    }
                }
                return reval;
            }
        }

        private Dictionary<string, object> DataReaderToDynamicList_Part<T>(Dictionary<string, object> readerValues, PropertyInfo item, List<T> reval)
        {
            var result = new Dictionary<string, object>();
            var type = item.PropertyType;
            if (UtilConstants.SugarType == type)
            {
                return result;
            }
            if (type.FullName.IsCollectionsList())
            {
                return null;
            }
            var classProperties = type.GetProperties().ToList();
            foreach (var prop in classProperties)
            {
                var name = prop.Name;
                var typeName = type.Name;
                if (prop.PropertyType.IsClass())
                {
                    result.Add(name, DataReaderToDynamicList_Part(readerValues, prop, reval));
                }
                else
                {
                    var key = typeName + "." + name;
                    if (readerValues.ContainsKey(key))
                    {
                        var addItem = readerValues[key];
                        if (addItem == DBNull.Value)
                            addItem = null;
                        result.Add(name, addItem);
                    }
                }
            }
            return result;
        }

        #endregion DataReader

        #region Serialize

        /// <summary>
        /// Serialize Object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SerializeObject(object value)
        {
            DependencyManagement.TryJsonNet();
            return Context.CurrentConnectionConfig.ConfigureExternalServices.SerializeService.SerializeObject(value);
        }

        /// <summary>
        /// Serialize Object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public T DeserializeObject<T>(string value)
        {
            DependencyManagement.TryJsonNet();
            return Context.CurrentConnectionConfig.ConfigureExternalServices.SerializeService.DeserializeObject<T>(value);
        }

        #endregion Serialize

        #region Copy Object

        /// <summary>
        /// Copy new Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public T TranslateCopy<T>(T sourceObject)
        {
            if (sourceObject == null) return default(T);
            else
            {
                var jsonString = SerializeObject(sourceObject);
                return DeserializeObject<T>(jsonString);
            }
        }

        public SqlSugarClient CopyContext(bool isCopyEvents = false)
        {
            var newClient = new SqlSugarClient(this.TranslateCopy(Context.CurrentConnectionConfig))
            {
                MappingColumns = this.TranslateCopy(Context.MappingColumns),
                MappingTables = this.TranslateCopy(Context.MappingTables),
                IgnoreColumns = this.TranslateCopy(Context.IgnoreColumns)
            };
            if (isCopyEvents)
            {
                newClient.Ado.IsEnableLogEvent = Context.Ado.IsEnableLogEvent;
                newClient.Ado.LogEventStarting = Context.Ado.LogEventStarting;
                newClient.Ado.LogEventCompleted = Context.Ado.LogEventCompleted;
                newClient.Ado.ProcessingEventStartingSQL = Context.Ado.ProcessingEventStartingSQL;
            }
            return newClient;
        }

        #endregion Copy Object

        #region DataTable

        public dynamic DataTableToDynamic(DataTable table)
        {
            var deserializeObject = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    var addItem = row[col];
                    if (addItem == DBNull.Value)
                        addItem = null;
                    childRow.Add(col.ColumnName, addItem);
                }
                deserializeObject.Add(childRow);
            }
            return this.DeserializeObject<dynamic>(this.SerializeObject(deserializeObject));
        }

        #endregion DataTable

        #region Cache

        public ICacheService GetReflectionInoCacheInstance()
        {
            return Context.CurrentConnectionConfig.ConfigureExternalServices.ReflectionInoCacheService;
        }

        public void RemoveCacheAll()
        {
            ReflectionInoHelper.RemoveAllCache();
        }

        public void RemoveCacheAll<T>()
        {
            ReflectionInoCore<T>.GetInstance().RemoveAllCache();
        }

        public void RemoveCache<T>(string key)
        {
            ReflectionInoCore<T>.GetInstance().Remove(key);
        }

        #endregion Cache

        #region Query

        public KeyValuePair<string, SugarParameter[]> ConditionalModelToSql(List<IConditionalModel> models, int beginIndex = 0)
        {
            if (models.IsNullOrEmpty()) return new KeyValuePair<string, SugarParameter[]>();
            var builder = new StringBuilder();
            var parameters = new List<SugarParameter>();
            var sqlBuilder = InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
            foreach (var model in models)
            {
                if (model is ConditionalModel)
                {
                    var item = model as ConditionalModel;
                    var index = models.IndexOf(item) + beginIndex;
                    var type = index == 0 ? "" : "AND";
                    if (beginIndex > 0)
                    {
                        type = null;
                    }
                    var temp = " {0} {1} {2} {3}  ";
                    var parameterName = string.Format("{0}Conditional{1}{2}", sqlBuilder.SqlParameterKeyWord, item.FieldName, index);
                    switch (item.ConditionalType)
                    {
                        case ConditionalType.Equal:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "=", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.Like:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "LIKE", parameterName);
                            parameters.Add(new SugarParameter(parameterName, "%" + item.FieldValue + "%"));
                            break;

                        case ConditionalType.GreaterThan:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), ">", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.GreaterThanOrEqual:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), ">=", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.LessThan:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "<", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.LessThanOrEqual:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "<=", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.In:
                            if (item.FieldValue == null) item.FieldValue = string.Empty;
                            var inValue1 = ("(" + item.FieldValue.Split(',').ToJoinSqlInVals() + ")");
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "IN", inValue1);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.NotIn:
                            if (item.FieldValue == null) item.FieldValue = string.Empty;
                            var inValue2 = ("(" + item.FieldValue.Split(',').ToJoinSqlInVals() + ")");
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "NOT IN", inValue2);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.LikeLeft:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "LIKE", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue + "%"));
                            break;

                        case ConditionalType.LikeRight:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "LIKE", parameterName);
                            parameters.Add(new SugarParameter(parameterName, "%" + item.FieldValue));
                            break;

                        case ConditionalType.NoEqual:
                            builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "<>", parameterName);
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.IsNullOrEmpty:
                            builder.AppendFormat("{0} ({1}) OR ({2}) ", type, item.FieldName.ToSqlFilter() + " IS NULL ", item.FieldName.ToSqlFilter() + " = '' ");
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.IsNot:
                            if (item.FieldValue == null)
                            {
                                builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), " IS NOT ", "NULL");
                            }
                            else
                            {
                                builder.AppendFormat(temp, type, item.FieldName.ToSqlFilter(), "<>", parameterName);
                                parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            }
                            break;

                        case ConditionalType.CharIndexLeft:
                            builder.AppendFormat(temp, type, "CHARINDEX(" + item.FieldName.ToSqlFilter(), ",", parameterName + ")>0");
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        case ConditionalType.CharIndexRight:
                            builder.AppendFormat(temp, type, "CHARINDEX(" + parameterName, ",", item.FieldName.ToSqlFilter() + ")>0");
                            parameters.Add(new SugarParameter(parameterName, item.FieldValue));
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    var item = model as ConditionalCollections;
                    if (item != null)
                    {
                        foreach (var con in item.ConditionalList)
                        {
                            var index = item.ConditionalList.IndexOf(con);
                            var isFirst = index == 0;
                            var isLast = index == (item.ConditionalList.Count - 1);
                            if (isFirst)
                            {
                                builder.AppendFormat(" {0} ( ", con.Key.ToString().ToUpper());
                            }
                            var conModels = new List<IConditionalModel>();
                            conModels.Add(con.Value);
                            var childSqlInfo = ConditionalModelToSql(conModels, 1000 * (1 + index));
                            if (!isFirst)
                            {
                                builder.AppendFormat(" {0} ", con.Key.ToString().ToUpper());
                            }
                            builder.Append(childSqlInfo.Key);
                            parameters.AddRange(childSqlInfo.Value);
                            if (isLast)
                            {
                                builder.Append(" ) ");
                            }
                            else
                            {
                            }
                        }
                    }
                }
            }
            return new KeyValuePair<string, SugarParameter[]>(builder.ToString(), parameters.ToArray());
        }

        #endregion Query
    }
}
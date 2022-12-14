using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSugar
{
    public abstract partial class DbFirstProvider : IDbFirst
    {
        public virtual SqlSugarClient Context { get; set; }
        private string ClassTemplate { get; set; }
        private string ClassDescriptionTemplate { get; set; }
        private string PropertyTemplate { get; set; }
        private string PropertyCostTemplate { get; set; }
        private string PropertyDescriptionTemplate { get; set; }
        private string ConstructorTemplate { get; set; }
        private string UsingTemplate { get; set; }
        private string Namespace { get; set; }
        private bool IsAttribute { get; set; }
        private bool IsDefaultValue { get; set; }

        private ISqlBuilder SqlBuilder
        {
            get
            {
                return InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
            }
        }

        private List<DbTableInfo> TableInfoList { get; set; }

        protected DbFirstProvider()
        {
            this.ClassTemplate = DbFirstTemplate.ClassTemplate;
            this.ClassDescriptionTemplate = DbFirstTemplate.ClassDescriptionTemplate;
            this.PropertyTemplate = DbFirstTemplate.PropertyTemplate;
            this.PropertyCostTemplate = DbFirstTemplate.PropertyConstNameTemplate;
            this.PropertyDescriptionTemplate = DbFirstTemplate.PropertyDescriptionTemplate;
            this.ConstructorTemplate = DbFirstTemplate.ConstructorTemplate;
            this.UsingTemplate = DbFirstTemplate.UsingTemplate;
        }

        public void Init()
        {
            this.Context.Utilities.RemoveCacheAll();
            if (!this.Context.DbMaintenance.IsAnySystemTablePermissions())
            {
                Check.Exception(true, "Dbfirst and  Codefirst requires system table permissions");
            }
            this.TableInfoList = this.Context.Utilities.TranslateCopy(this.Context.DbMaintenance.GetTableInfoList());
            var viewList = this.Context.Utilities.TranslateCopy(this.Context.DbMaintenance.GetViewInfoList());
            if (viewList.HasValue())
            {
                this.TableInfoList.AddRange(viewList);
            }
        }

        #region Setting Template

        public IDbFirst SettingClassDescriptionTemplate(Func<string, string> func)
        {
            this.ClassDescriptionTemplate = func?.Invoke(this.ClassDescriptionTemplate);
            return this;
        }

        public IDbFirst SettingClassTemplate(Func<string, string> func)
        {
            this.ClassTemplate = func?.Invoke(this.ClassTemplate);
            return this;
        }

        public IDbFirst SettingConstructorTemplate(Func<string, string> func)
        {
            this.ConstructorTemplate = func?.Invoke(this.ConstructorTemplate);
            return this;
        }

        public IDbFirst SettingPropertyDescriptionTemplate(Func<string, string> func)
        {
            this.PropertyDescriptionTemplate = func?.Invoke(this.PropertyDescriptionTemplate);
            return this;
        }

        public IDbFirst SettingNamespaceTemplate(Func<string, string> func)
        {
            this.UsingTemplate = func?.Invoke(this.UsingTemplate);
            return this;
        }

        public IDbFirst SettingPropertyTemplate(Func<string, string> func)
        {
            this.PropertyTemplate = func?.Invoke(this.PropertyTemplate);
            return this;
        }

        #endregion Setting Template

        #region Setting Content

        public IDbFirst IsCreateAttribute(bool isCreateAttribute = true)
        {
            this.IsAttribute = isCreateAttribute;
            return this;
        }

        public IDbFirst IsCreateDefaultValue(bool isCreateDefaultValue = true)
        {
            this.IsDefaultValue = isCreateDefaultValue;
            return this;
        }

        #endregion Setting Content

        #region Where

        public IDbFirst Where(DbObjectType dbObjectType)
        {
            if (dbObjectType != DbObjectType.All)
                this.TableInfoList = this.TableInfoList.Where(it => it.DbObjectType == dbObjectType).ToList();
            return this;
        }

        public IDbFirst Where(Func<string, bool> func)
        {
            this.TableInfoList = this.TableInfoList.Where(it => func?.Invoke(it.Name) ?? default(bool)).ToList();
            return this;
        }

        public IDbFirst Where(params string[] objectNames)
        {
            if (objectNames.HasValue())
            {
                this.TableInfoList = this.TableInfoList.Where(it => objectNames.Contains(it.Name)).ToList();
            }
            return this;
        }

        #endregion Where

        #region Core

        public Dictionary<string, string> ToClassStringList(string nameSpace = "Models")
        {
            this.Namespace = nameSpace;
            var result = new Dictionary<string, string>();
            if (this.TableInfoList.HasValue())
            {
                foreach (var tableInfo in this.TableInfoList)
                {
                    try
                    {
                        string classText = null;
                        var className = tableInfo.Name;
                        classText = GetClassString(tableInfo, ref className);
                        result.Remove(className);
                        result.Add(className, classText);
                    }
                    catch (Exception ex)
                    {
                        Check.Exception(true, "Table '{0}' error,You can filter it with Db.DbFirst.Where(name=>name!=\"{0}\" ) \r\n Error message:{1}", tableInfo.Name, ex.Message);
                    }
                }
            }
            return result;
        }

        internal string GetClassString(DbTableInfo tableInfo, ref string className)
        {
            string classText;
            var columns = this.Context.DbMaintenance.GetColumnInfosByTableName(tableInfo.Name);
            if (this.Context.IgnoreColumns.HasValue())
            {
                var entityName = this.Context.EntityMaintenance.GetEntityName(tableInfo.Name);
                columns = columns.Where(c =>
                                         !this.Context.IgnoreColumns.Any(ig => ig.EntityName.Equals(entityName, StringComparison.CurrentCultureIgnoreCase) && c.DbColumnName == ig.PropertyName)
                                        ).ToList();
            }
            classText = this.ClassTemplate;
            var ConstructorText = IsDefaultValue ? this.ConstructorTemplate : null;
            if (this.Context.MappingTables.HasValue())
            {
                var mappingInfo = this.Context.MappingTables.FirstOrDefault(it => it.DbTableName.Equals(tableInfo.Name, StringComparison.CurrentCultureIgnoreCase));
                if (mappingInfo.HasValue())
                {
                    className = mappingInfo.EntityName;
                }
                if (mappingInfo != null)
                {
                    classText = classText.Replace(DbFirstTemplate.KeyClassName, mappingInfo.EntityName);
                }
            }
            classText = classText.Replace(DbFirstTemplate.KeyClassName, className);
            classText = classText.Replace(DbFirstTemplate.KeyNamespace, this.Namespace);
            classText = classText.Replace(DbFirstTemplate.KeyUsing, IsAttribute ? (this.UsingTemplate + "using " + UtilConstants.AssemblyName + ";\r\n") : this.UsingTemplate);
            classText = classText.Replace(DbFirstTemplate.KeyClassDescription, this.ClassDescriptionTemplate.Replace(DbFirstTemplate.KeyClassDescription, tableInfo.Description + "\r\n"));
            classText = classText.Replace(DbFirstTemplate.KeySugarTable, IsAttribute ? string.Format(DbFirstTemplate.ValueSugarTable, tableInfo.Name) : null);
            if (columns.HasValue())
            {
                foreach (var item in columns)
                {
                    var isLast = columns.Last() == item;
                    var index = columns.IndexOf(item);
                    var PropertyText = this.PropertyTemplate;
                    var PropertyCostText = this.PropertyCostTemplate;
                    var PropertyDescriptionText = this.PropertyDescriptionTemplate;
                    var propertyName = GetPropertyName(item);
                    var propertyTypeName = GetPropertyTypeName(item);
                    PropertyText = GetPropertyText(item, PropertyText);
                    PropertyCostText = GetPropertyCostText(item, PropertyCostText);
                    PropertyDescriptionText = GetPropertyDescriptionText(item, PropertyDescriptionText);
                    PropertyText = PropertyDescriptionText + PropertyText + PropertyCostText;
                    classText = classText.Replace(DbFirstTemplate.KeyPropertyName, PropertyText + (isLast ? "" : ("\r\n" + DbFirstTemplate.KeyPropertyName)));
                    if (ConstructorText.HasValue() && item.DefaultValue != null)
                    {
                        var hasDefaultValue = columns.Skip(index + 1).Any(it => it.DefaultValue.HasValue());
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyDefaultValue, GetPropertyTypeConvert(item)) + (!hasDefaultValue ? "" : this.ConstructorTemplate);
                    }
                }
            }
            if (!columns.Any(it => it.DefaultValue != null))
            {
                ConstructorText = null;
            }
            classText = classText.Replace(DbFirstTemplate.KeyConstructor, ConstructorText);
            classText = classText.Replace(DbFirstTemplate.KeyPropertyName, null);
            return classText;
        }

        internal string GetClassString(List<DbColumnInfo> columns, ref string className)
        {
            var classText = this.ClassTemplate;
            var ConstructorText = IsDefaultValue ? this.ConstructorTemplate : null;
            classText = classText.Replace(DbFirstTemplate.KeyClassName, className);
            classText = classText.Replace(DbFirstTemplate.KeyNamespace, this.Namespace);
            classText = classText.Replace(DbFirstTemplate.KeyUsing, IsAttribute ? (this.UsingTemplate + "using " + UtilConstants.AssemblyName + ";\r\n") : this.UsingTemplate);
            classText = classText.Replace(DbFirstTemplate.KeyClassDescription, this.ClassDescriptionTemplate.Replace(DbFirstTemplate.KeyClassDescription, "\r\n"));
            classText = classText.Replace(DbFirstTemplate.KeySugarTable, IsAttribute ? string.Format(DbFirstTemplate.ValueSugarTable, className) : null);
            if (columns.HasValue())
            {
                foreach (var item in columns)
                {
                    var isLast = columns.Last() == item;
                    var index = columns.IndexOf(item);
                    var PropertyText = this.PropertyTemplate;
                    var PropertyCostText = this.PropertyCostTemplate;
                    var PropertyDescriptionText = this.PropertyDescriptionTemplate;
                    var propertyName = GetPropertyName(item);
                    var propertyTypeName = item.PropertyName;
                    PropertyText = GetPropertyText(item, PropertyText);
                    PropertyCostText = GetPropertyCostText(item, PropertyText);
                    PropertyDescriptionText = GetPropertyDescriptionText(item, PropertyDescriptionText);
                    PropertyText = PropertyDescriptionText + PropertyText + PropertyCostText;
                    classText = classText.Replace(DbFirstTemplate.KeyPropertyName, PropertyText + (isLast ? "" : ("\r\n" + DbFirstTemplate.KeyPropertyName)));
                    if (ConstructorText.HasValue() && item.DefaultValue != null)
                    {
                        var hasDefaultValue = columns.Skip(index + 1).Any(it => it.DefaultValue.HasValue());
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyDefaultValue, GetPropertyTypeConvert(item)) + (!hasDefaultValue ? "" : this.ConstructorTemplate);
                    }
                }
            }
            if (!columns.Any(it => it.DefaultValue != null))
            {
                ConstructorText = null;
            }
            classText = classText.Replace(DbFirstTemplate.KeyConstructor, ConstructorText);
            classText = classText.Replace(DbFirstTemplate.KeyPropertyName, null);
            return classText;
        }

        public void CreateClassFile(string directoryPath, string nameSpace = "Models")
        {
            var seChar = Path.DirectorySeparatorChar.ToString();
            Check.ArgumentNullException(directoryPath, "directoryPath can't null");
            var classStringList = ToClassStringList(nameSpace);
            if (classStringList.IsValuable())
            {
                foreach (var item in classStringList)
                {
                    var filePath = directoryPath.TrimEnd('\\').TrimEnd('/') + string.Format(seChar + "{0}.cs", item.Key);
                    FileHelper.CreateFile(filePath, item.Value, Encoding.UTF8);
                }
            }
        }

        #endregion Core

        #region Private methods

        private string GetProertypeDefaultValue(DbColumnInfo item)
        {
            var result = item.DefaultValue;
            if (result == null) return null;
            if (Regex.IsMatch(result, @"^\(\'(.+)\'\)$"))
            {
                result = Regex.Match(result, @"^\(\'(.+)\'\)$").Groups[1].Value;
            }
            if (Regex.IsMatch(result, @"^\(\((.+)\)\)$"))
            {
                result = Regex.Match(result, @"^\(\((.+)\)\)$").Groups[1].Value;
            }
            if (Regex.IsMatch(result, @"^\((.+)\)$"))
            {
                result = Regex.Match(result, @"^\((.+)\)$").Groups[1].Value;
            }
            if (result.Equals(this.SqlBuilder.SqlDateNow, StringComparison.CurrentCultureIgnoreCase))
            {
                result = "DateTime.Now";
            }
            result = result.Replace("\r", "\t").Replace("\n", "\t");
            result = result.IsIn("''", "\"\"") ? string.Empty : result;
            return result;
        }

        private string GetPropertyText(DbColumnInfo item, string PropertyText)
        {
            var SugarColumnText = DbFirstTemplate.ValueSugarCoulmn;
            var propertyName = GetPropertyName(item);
            var isMappingColumn = propertyName != item.DbColumnName;
            var hasSugarColumn = item.IsPrimarykey || item.IsIdentity || isMappingColumn;
            if (hasSugarColumn && this.IsAttribute)
            {
                var joinList = new List<string>();
                if (item.IsPrimarykey)
                {
                    joinList.Add("IsPrimaryKey=true");
                }
                if (item.IsIdentity)
                {
                    joinList.Add("IsIdentity=true");
                }
                if (isMappingColumn)
                {
                    joinList.Add("ColumnName=\"" + item.DbColumnName + "\"");
                }
                SugarColumnText = string.Format(SugarColumnText, string.Join(",", joinList));
            }
            else
            {
                SugarColumnText = null;
            }
            var typeString = GetPropertyTypeName(item);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeySugarColumn, SugarColumnText);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeyPropertyType, typeString);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
            return PropertyText;
        }

        private string GetPropertyCostText(DbColumnInfo item, string PropertyText)
        {
            var propertyName = GetPropertyName(item);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeyPropertyConstName, propertyName.ToUpper());
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
            return PropertyText;
        }

        private string GetEnityName(DbColumnInfo item)
        {
            var mappingInfo = this.Context.MappingTables.FirstOrDefault(it => it.DbTableName.Equals(item.TableName, StringComparison.CurrentCultureIgnoreCase));
            return mappingInfo == null ? item.TableName : mappingInfo.EntityName;
        }

        private string GetPropertyName(DbColumnInfo item)
        {
            if (this.Context.MappingColumns.HasValue())
            {
                var mappingInfo = this.Context.MappingColumns.SingleOrDefault(it => it.DbColumnName == item.DbColumnName && it.EntityName == GetEnityName(item));
                return mappingInfo == null ? item.DbColumnName : mappingInfo.PropertyName;
            }
            else
            {
                return item.DbColumnName;
            }
        }

        private string GetPropertyTypeName(DbColumnInfo item)
        {
            var result = item.PropertyType != null ? item.PropertyType.Name : this.Context.Ado.DbBind.GetPropertyTypeName(item.DataType);
            if (result != "string" && result != "byte[]" && result != "object" && item.IsNullable)
            {
                result += "?";
            }
            if (result == "Int32")
            {
                result = "int";
            }
            if (result == "String")
            {
                result = "string";
            }
            return result;
        }

        private string GetPropertyTypeConvert(DbColumnInfo item)
        {
            var convertString = GetProertypeDefaultValue(item);
            if (convertString == "DateTime.Now" || convertString == null)
                return convertString;
            if (item.DataType == "bit")
                return (convertString == "1" || convertString.Equals("true", StringComparison.CurrentCultureIgnoreCase)).ToString().ToLower();
            var result = this.Context.Ado.DbBind.GetConvertString(item.DataType) + "(\"" + convertString + "\")";
            return result;
        }

        private string GetPropertyDescriptionText(DbColumnInfo item, string propertyDescriptionText)
        {
            propertyDescriptionText = propertyDescriptionText.Replace(DbFirstTemplate.KeyPropertyDescription, GetColumnDescription(item.ColumnDescription));
            propertyDescriptionText = propertyDescriptionText.Replace(DbFirstTemplate.KeyDefaultValue, GetProertypeDefaultValue(item));
            propertyDescriptionText = propertyDescriptionText.Replace(DbFirstTemplate.KeyIsNullable, item.IsNullable.ObjToString());
            return propertyDescriptionText;
        }

        private string GetColumnDescription(string columnDescription)
        {
            if (columnDescription == null) return columnDescription;
            columnDescription = columnDescription.Replace("\r", "\t");
            columnDescription = columnDescription.Replace("\n", "\t");
            columnDescription = Regex.Replace(columnDescription, "\t{2,}", "\t");
            return columnDescription;
        }

        #endregion Private methods
    }
}
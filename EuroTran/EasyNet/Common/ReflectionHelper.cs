using System;
using System.Reflection;

namespace EasyNet.Common
{
    public class ReflectionHelper
    {
        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        public static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        #region 快速執行方法

        /// <summary>
        /// 快速執行Method
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object FastMethodInvoke(object obj, MethodInfo method, params object[] parameters)
        {
            return DynamicCalls.GetMethodInvoker(method)(obj, parameters);
        }

        /// <summary>
        /// 快速產生實體一個T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>()
        {
            return (T)Create(typeof(T))();
        }

        /// <summary>
        /// 快速產生實體一個FastCreateInstanceHandler
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FastCreateInstanceHandler Create(Type type)
        {
            return DynamicCalls.GetInstanceCreator(type);
        }

        #endregion 快速執行方法

        #region 設置屬性值

        /// <summary>
        /// 設置屬性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, PropertyInfo property, object value)
        {
            if (property.CanWrite)
            {
                var propertySetter = DynamicCalls.GetPropertySetter(property);
                value = TypeUtils.ConvertForType(value, property.PropertyType);
                propertySetter(obj, value);
            }
        }

        /// <summary>
        /// 設置屬性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            SetPropertyValue(obj.GetType(), obj, propertyName, value);
        }

        /// <summary>
        /// 設置屬性值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(Type type, object obj, string propertyName, object value)
        {
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                SetPropertyValue(obj, property, value);
            }
        }

        /// <summary>
        /// 獲取屬性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(T entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(entity, null);
            }

            return null;
        }

        /// <summary>
        /// 獲取屬性值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property">todo: describe property parameter on GetPropertyValue</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object GetPropertyValue<T>(T entity, PropertyInfo property)
        {
            if (property != null)
            {
                return property.GetValue(entity, null);
            }

            return null;
        }

        /// <summary>
        /// 獲取屬性類型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetPropertyType<T>(T entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return property.PropertyType.FullName;
            }

            return null;
        }

        /// <summary>
        /// 獲取屬性類型
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="properties">todo: describe properties parameter on GetProperty</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PropertyInfo GetProperty(PropertyInfo[] properties, string propertyName)
        {
            foreach (PropertyInfo property in properties)
            {
                var name = property.Name;
                if (propertyName.ToLower() == name.ToLower())
                {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// 轉換值
        /// </summary>
        /// <param name="entity">todo: describe entity parameter on ConvertPropertyValue</param>
        /// <param name="propertyName">todo: describe propertyName parameter on ConvertPropertyValue</param>
        public static object ConvertPropertyValue<T>(T entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName);
            if (property != null)
            {
                if (property.CanWrite)
                {
                    return TypeUtils.ConvertForType(property.GetValue(entity, null), property.PropertyType);
                }
            }
            return null;
        }

        #endregion 設置屬性值

        /*public static void SetPropertyValue(Object obj, PropertyInfo property, Object value)
        {
            //創建Set委託
            SetHandler setter = DynamicMethodCompiler.CreateSetHandler(obj.GetType(),property);

            //先獲取該私有成員的資料類型
            Type type = property.PropertyType;

            //通過資料類型轉換
            value = TypeUtils.ConvertForType(value, type);

            //將值設置到對象中
            setter(obj, value);
        }

        public static Object GetPropertyValue(Object obj, PropertyInfo property)
        {
            //創建Set委託
            GetHandler getter = DynamicMethodCompiler.CreateGetHandler(obj.GetType(), property);

            //獲取屬性值
            return getter(obj);
        }

        public static void SetFieldValue(Object obj, FieldInfo field, Object value)
        {
            //創建Set委託
            SetHandler setter = DynamicMethodCompiler.CreateSetHandler(obj.GetType(), field);

            //先獲取該私有成員的資料類型
            Type type = field.FieldType;

            //通過資料類型轉換
            value = TypeUtils.ConvertForType(value, type);

            //將值設置到對象中
            setter(obj, value);
        }

        public static Object GetFieldValue(Object obj, FieldInfo field)
        {
            //創建Set委託
            GetHandler getter = DynamicMethodCompiler.CreateGetHandler(obj.GetType(), field);

            //獲取欄位值
            return getter(obj);
        }*/
    }
}
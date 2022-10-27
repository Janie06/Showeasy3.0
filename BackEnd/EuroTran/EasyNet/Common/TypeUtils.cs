using System;

namespace EasyNet.Common
{
    public class TypeUtils
    {
        public static object ConvertForType(object value, Type type)
        {
            if (Convert.IsDBNull(value) || (value == null))
            {
                return null;
            }

            var typeName = type.FullName.ToString();
            if (typeName.IndexOf("System.DateTime") > -1)
            {
                typeName = "System.DateTime";
            }
            else if (typeName.IndexOf("System.Single") > -1)
            {
                typeName = "System.Single";
            }
            else if (typeName.IndexOf("System.Float") > -1)
            {
                typeName = "System.Float";
            }
            else if (typeName.IndexOf("System.Double") > -1)
            {
                typeName = "System.Double";
            }
            else if (typeName.IndexOf("System.Decimal") > -1)
            {
                typeName = "System.Decimal";
            }
            Console.WriteLine(typeName);

            if (type == typeof(Nullable<UInt16>))
            {
                value = Convert.ToUInt16(value);
            }
            else if (type == typeof(Nullable<UInt32>))
            {
                value = Convert.ToUInt32(value);
            }
            else if (type == typeof(Nullable<UInt64>))
            {
                value = Convert.ToUInt64(value);
            }
            else if (type == typeof(Nullable<Int32>))
            {
                value = Convert.ToInt32(value);
            }
            else if (type == typeof(Nullable<Int64>))
            {
                value = Convert.ToInt64(value);
            }
            switch (typeName)
            {
                case "System.String":
                    if (!IsNullOrEmpty(value))
                        value = value.ToString();
                    break;

                case "System.Boolean":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToBoolean(value);
                    else
                        value = false;
                    break;

                case "System.Int16":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToInt16(value);
                    break;

                case "System.Int32":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToInt32(value);
                    break;

                case "System.Int64":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToInt64(value);
                    break;

                case "System.Double":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToDouble(value);
                    break;

                case "System.Float":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToDouble(value);
                    break;

                case "System.Single":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToSingle(value);
                    break;

                case "System.Decimal":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToDecimal(value);
                    break;

                case "System.DateTime":
                    if (!IsNullOrEmpty(value))
                        value = Convert.ToDateTime(value);
                    break;

                default:

                    break;
            }
            return value;
        }

        private static bool IsNullOrEmpty(object val)
        {
            if (val == null) return true;
            if (val.ToString() == "") return true;
            return false;
        }
    }
}
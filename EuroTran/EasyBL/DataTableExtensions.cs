using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace EasyBL
{
    public class DataTableExtensions
    {
        /*Converts List To DataTable*/

        public static DataTable ListToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propInfo in Properties)
            {
                dataTable.Columns.Add(propInfo.Name);
            }

            foreach (T item in items)
            {
                var values = new object[Properties.Length];

                for (int i = 0; i < Properties.Length; i++)
                {
                    values[i] = Properties[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}
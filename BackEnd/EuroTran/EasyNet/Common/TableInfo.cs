using EasyNet.DBUtility;
using System.Collections.Generic;
using System.Data;

namespace EasyNet.Common
{
    public class TableInfo
    {
        public string TableName { get; set; }

        public bool Strategy { get; set; } = false;

        public List<IdInfo> Ids { get; set; } = new List<IdInfo>();

        public IdInfo Id { get; set; } = new IdInfo();

        public ColumnInfo Columns { get; set; } = new ColumnInfo();

        public Map PropToColumn { get; set; } = new Map();

        public Map Keycolumns { get; set; } = new Map();

        public IDbDataParameter[] GetParameters()
        {
            IDbDataParameter[] parameters = null;
            if (this.Columns != null && this.Columns.Count > 0)
            {
                parameters = DbFactory.CreateDbParameters(this.Columns.Count);
                DbEntityUtils.SetParameters(this.Columns, parameters);
            }
            return parameters;
        }
    }
}
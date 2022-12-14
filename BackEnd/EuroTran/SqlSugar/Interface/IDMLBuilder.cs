using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public partial interface IDMLBuilder
    {
        string SqlTemplate { get; }
        List<SugarParameter> Parameters { get; set; }
        SqlSugarClient  Context { get; set; }
        StringBuilder sql { get; set; }
        string ToSqlString();
        void Clear();
    }
}

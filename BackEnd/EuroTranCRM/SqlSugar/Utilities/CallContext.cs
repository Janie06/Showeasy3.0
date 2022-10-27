using System.Collections.Generic;
using System.Threading;

namespace SqlSugar
{
    internal class CallContext
    {
        public static ThreadLocal<List<SqlSugarClient>> ContextList = new ThreadLocal<List<SqlSugarClient>>();
    }
}

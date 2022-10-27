using System.Threading;

namespace EasyNet.Manager
{
    public class ManagerThread
    {
        private static ThreadLocal<DbManager> m_ManagerLocal = new ThreadLocal<DbManager>();

        public static void Set(DbManager m)
        {
            m_ManagerLocal.Value = m;
        }

        public static DbManager Get()
        {
            return m_ManagerLocal.Value;
        }

        public static void Clear()
        {
            m_ManagerLocal.Value = null;
        }
    }
}
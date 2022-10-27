namespace EasyNet.Manager
{
    public class ManagerFactory
    {
        public static DbManager GetManager()
        {
            var m = ManagerThread.Get();
            if (m == null)
            {
                m = DbManager.PriviteInstance();
                ManagerThread.Set(m);
            }

            return m;
        }
    }
}
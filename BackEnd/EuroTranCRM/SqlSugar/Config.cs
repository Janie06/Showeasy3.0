namespace SqlSugar
{
    public static class Config
    {
        public static string ConnectionString = DBUnit.GetAppSettings("ConnectionString");
        public static string ConnectionString2 = DBUnit.GetAppSettings("ConnectionString2");
    }
}

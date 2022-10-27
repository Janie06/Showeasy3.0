using Newtonsoft.Json;
using SqlSugar.Entities;

namespace SqlSugar
{
    public class ModelContext : BaseEntity
    {
        [SugarColumn(IsIgnore = true)]
        [JsonIgnore]
        public SqlSugarClient Context { get; set; }

        public ISugarQueryable<T> CreateMapping<T>() where T : class, new()
        {
            Check.ArgumentNullException(Context, "Please use Sqlugar.ModelContext");
            using (Context)
            {
                return Context.Queryable<T>();
            }
        }
    }
}
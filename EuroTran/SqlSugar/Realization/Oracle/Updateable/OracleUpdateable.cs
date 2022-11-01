using System.Collections.Generic;
using System.Linq;

namespace SqlSugar
{
    public class OracleUpdateable<T> : UpdateableProvider<T> where T : class, new()
    {
        protected override List<string> GetIdentityKeys()
        {
            return this.EntityInfo.Columns.Where(it => it.OracleSequenceName.HasValue()).Select(it => it.DbColumnName).ToList();
        }
        public override int ExecuteCommand()
        {
            base.ExecuteCommand();
            return base.UpdateObjs.Count();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euro.Transfer.Jobs.BackRuns
{
    public class Config : Base.ServiceConfig
    {
        #region 基本属性

        private string mDescription;
        private string mEnabled;
        private string mAssembly;
        private int mInterval;

        /// <summary>
        /// 说明
        /// </summary>
        public override string Description
        {
            get { return this.mDescription; }
        }

        /// <summary>
        /// 是否开启
        /// </summary>
        public override string Enabled
        {
            get { return this.mEnabled; }
        }

        /// <summary>
        /// 处理程序集
        /// </summary>
        public override string Assembly
        {
            get { return this.mAssembly; }
        }

        /// <summary>
        /// 间隔时间
        /// </summary>
        public override int Interval
        {
            get { return this.mInterval; }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数，将配置项加载进对象
        /// </summary>
        public Config()
        {
            var nvc = Base.ServiceTools.GetSection(nameof(BackRuns));

            foreach (string s in nvc.Keys)
            {
                switch (s.ToLower())
                {
                    //基本
                    case "description":
                        this.mDescription = nvc[s].ToString();
                        break;
                    case "enabled":
                        this.mEnabled = nvc[s].ToString();
                        break;
                    case "assembly":
                        this.mAssembly = nvc[s].ToString();
                        break;
                    case "interval":
                        this.mInterval = int.Parse(nvc[s].ToString());
                        break;
                }
            }
        }

        #endregion

    }
}

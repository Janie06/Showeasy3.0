using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euro.Transfer.Base
{
    /// <summary>
    /// 工作项
    /// </summary>
    public abstract class ServiceTask : ServiceBase
    {
        //配置对象
        private ServiceConfig mConfigObject;
        //下次运行时间
        private DateTime mNextTime;
        //任务是否在运行中
        protected bool mIsRunning;
        //初始化客戶端
        public static HubTransfer hubClient = new HubTransfer();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ServiceTask()
        {
            //变量初始化
            this.mNextTime = DateTime.Now;
            this.mIsRunning = false;
        }

        /// <summary>
        /// 配置对象
        /// </summary>
        public ServiceConfig ConfigObject
        {
            get { return this.mConfigObject; }
            set { this.mConfigObject = value; }
        }

        /// <summary>
        /// 开始工作
        /// </summary>
        public void StartJob()
        {
            if (this.mConfigObject != null && this.mNextTime != null)
            {
                if (this.mConfigObject.Enabled.ToLower() == "true")
                {
                    if (DateTime.Now >= this.mNextTime)
                    {
                        if (!this.mIsRunning)
                        {
                            this.mNextTime = DateTime.Now.AddSeconds((double)this.mConfigObject.Interval);
                            this.Start();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 停止工作
        /// </summary>
        public void StopJob()
        {
            this.mConfigObject = null;
            this.mNextTime = DateTime.Now;
            this.mIsRunning = false;
            this.Stop();
        }

        #region 子类必需实现的抽象成员

        /// <summary>
        /// 开始工作
        /// </summary>
        protected abstract void Start();

        /// <summary>
        /// 停止工作
        /// </summary>
        protected abstract void Stop();

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*==================================================
 * Name:任務實體
 * Author:John.yuan
 * Time:2015-01-08
 * Modify:
====================================================*/
namespace TimingTask
{
    public class TimingTaskInfo
    {
        /// <summary>
        /// 任務執行類
        /// </summary>
        public TimingTaskBase.TimingTaskBase TaskObject
        {
            set;
            get;
        }

        /// <summary>
        /// 任務類型
        /// </summary>
        public TaskType TaskType
        {
            set;
            get;
        }

        /// <summary>
        /// 任務計時器
        /// </summary>
        public Timer TaskTimer
        {
            set;
            get;
        }

        /// <summary>
        /// 任務開始時間
        /// </summary>
        public DateTime BeginTime
        {
            set;
            get;
        }

        /// <summary>
        /// 任務結束時間
        /// </summary>
        public DateTime EndTime
        {
            set;
            get;
        }

        /// <summary>
        /// 任務執行週期 ms
        /// </summary>
        public long Period
        {
            set;
            get;
        }

        /// <summary>
        /// 任務名稱
        /// </summary>
        public string TaskName
        {
            set;
            get;
        }

        /// <summary>
        /// 任務編號
        /// </summary>
        public string TaskID
        {
            set;
            get;
        }

        /// <summary>
        /// 最後一次成功完成時間
        /// </summary>
        public DateTime? LastCompleteTime
        {
            set;
            get;
        }
    }
}

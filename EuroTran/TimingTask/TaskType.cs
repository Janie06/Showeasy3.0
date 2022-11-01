using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*==================================================
 * Name:任務類型
 * Author:John.yuan
 * Time:2015-01-08
 * Modify:
====================================================*/
namespace TimingTask
{
    public enum TaskType
    {
        /// <summary>
        /// 按月執行
        /// </summary>
        Month,

        /// <summary>
        /// 按天執行
        /// </summary>
        Day,

        /// <summary>
        /// 週期執行
        /// </summary>
        Period,

        /// <summary>
        /// 按週執行
        /// </summary>
        Week
    }
}

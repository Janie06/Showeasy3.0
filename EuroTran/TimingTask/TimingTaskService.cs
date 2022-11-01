using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;

/*==================================================
 * Name:定時執行任務
 * Author:John.yuan
 * Time:2015-01-08
 * Modify:
====================================================*/
namespace TimingTask
{
    public partial class TimingTaskService : ServiceBase
    {
        private string taskConnectionString;
        private string logPath;
        private long periodSecond;

        private List<TimingTaskInfo> taskList;

        /// <summary>
        /// 構造函數
        /// </summary>
        public TimingTaskService()
        {
            InitializeComponent();
            XmlDocument docConfig = new XmlDocument();
            try
            {
                docConfig.Load(System.Environment.CurrentDirectory + "/TimingTaskConfig.xml");
                XmlNodeList xnl = docConfig.SelectNodes(@"//Tasks");

                foreach (XmlNode xn in xnl)
                {
                    taskConnectionString = xn.Attributes["ConnectionString"].Value;
                    periodSecond = long.Parse(xn.Attributes["Period"].Value);
                    logPath = xn.Attributes["LogPath"].Value;
                }
            }
            catch (Exception e)
            {
                WriteLog("TimingTaskService:" + e.Message);
            }
        }

        /// <summary>
        /// 初始化定時任務
        /// </summary>
        private void InitializeTimingTask()
        {
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(System.Environment.CurrentDirectory + "/TimingTaskConfig.xml");
            XmlNodeList xnl = docConfig.SelectNodes(@"//Task[@IsEnabled='Y']");

            taskList = new List<TimingTaskInfo>();
            foreach (XmlNode xn in xnl)
            {
                string taskID = xn.Attributes["ID"].Value;
                string taskName = xn.Attributes["Name"].Value;
                TaskType timingTaskType;
                switch (xn.Attributes["TaskType"].Value)
                {
                    case "M":
                        timingTaskType = TaskType.Month;
                        break;
                    case "D":
                        timingTaskType = TaskType.Day;
                        break;
                    case "W":
                        timingTaskType = TaskType.Week;
                        break;
                    case "P":
                        timingTaskType = TaskType.Period;
                        break;
                    default:
                        WriteLog("InitializeTimingTask:任務類型配置異常。");
                        continue;
                }

                DateTime beginTime;
                DateTime endTime;
                try
                {
                    beginTime = AnalyzeTime(timingTaskType, xn.Attributes["BeginTime"].Value);
                    endTime = AnalyzeTime(timingTaskType, xn.Attributes["EndTime"].Value);
                    if (beginTime.Ticks >= endTime.Ticks)
                    {
                        WriteLog("InitializeTimingTask:開始時間或結束時間配置異常。");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    WriteLog("InitializeTimingTask:" + e.Message);
                    continue;
                }

                long period = long.Parse(xn.Attributes["Period"].Value);
                if (beginTime.Ticks > DateTime.Now.Ticks || endTime.Ticks <= DateTime.Now.Ticks)
                {
                    switch (timingTaskType)
                    {
                        case TaskType.Day:
                            beginTime = beginTime.AddDays(1);
                            endTime = endTime.AddDays(1);
                            break;
                        case TaskType.Month:
                            beginTime = beginTime.AddMonths(1);
                            endTime = endTime.AddMonths(1);
                            break;
                        case TaskType.Week:
                            beginTime = beginTime.AddDays(7);
                            endTime = endTime.AddDays(7);
                            break;
                        case TaskType.Period:
                            continue;
                        default:
                            WriteLog("InitializeTimingTask:任務類型配置異常。");
                            continue;
                    }
                }

                string className = xn.Attributes["ClassName"].Value;
                string assemblyName = xn.Attributes["AssemblyName"].Value;

                try
                {
                    Assembly taskAssembly = Assembly.LoadFrom(System.Environment.CurrentDirectory + @"/LIB/" + assemblyName);
                    Type taskAssType = taskAssembly.GetType(className);
                    TimingTaskBase.TimingTaskBase timingObj = (TimingTaskBase.TimingTaskBase)Activator.CreateInstance(taskAssType);

                    TimingTaskInfo taskInfo = new TimingTaskInfo()
                    {
                        TaskID = taskID,
                        TaskName = taskName,
                        BeginTime = beginTime,
                        EndTime = endTime,
                        TaskType = timingTaskType,
                        TaskObject = timingObj,
                        Period = period,
                        LastCompleteTime = null
                    };
                    taskList.Add(taskInfo);
                }
                catch (Exception e)
                {
                    WriteLog("InitializeTimingTask:" + e.Message);
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //初始化任務信息
                InitializeTimingTask();
                InitializeEachTaskTimer();
            }
            catch (Exception e)
            {
                WriteLog("OnStart:" + e.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    XmlDocument docConfig = new XmlDocument();
                    docConfig.Load(System.Environment.CurrentDirectory + "/TimingTaskConfig.xml");
                    foreach (TimingTaskInfo timingInfo in this.taskList)
                    {
                        timingInfo.TaskTimer.Change(-1, -1);
                        XmlNodeList xnl = docConfig.SelectNodes(@"//Tasks[@ID='" + timingInfo.TaskID + "']");
                        foreach (XmlNode xn in xnl)
                        {
                            XmlElement xmlEmt = (XmlElement)xn;
                            xmlEmt.SetAttribute("LastCompleteTime", timingInfo.LastCompleteTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                    }
                    docConfig.Save(System.Environment.CurrentDirectory + "/TimingTaskConfig.xml");
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                WriteLog("OnStop:" + e.Message);
            }
        }

        /// <summary>
        /// 計時器回呼函數
        /// </summary>
        /// <param name="state"></param>
        protected void InitializeEachTaskTimer()
        {
            foreach (TimingTaskInfo timingInfo in this.taskList)
            {
                if (timingInfo.BeginTime.Ticks <= DateTime.Now.Ticks && DateTime.Now.Ticks < timingInfo.EndTime.Ticks)
                {
                    try
                    {
                        if (timingInfo.LastCompleteTime == null || !timingInfo.LastCompleteTime.HasValue)
                        {
                            timingInfo.TaskTimer = new Timer(new TimerCallback(TaskExecute), timingInfo, 0, timingInfo.Period);
                        }
                        else
                        {
                            switch (timingInfo.TaskType)
                            {
                                case TaskType.Day:
                                    if (timingInfo.LastCompleteTime.Value.Ticks < timingInfo.BeginTime.Ticks)
                                    {
                                        timingInfo.TaskTimer = new Timer(new TimerCallback(TaskExecute), timingInfo, 0, timingInfo.Period);
                                    }
                                    break;
                                case TaskType.Week:
                                    if (timingInfo.LastCompleteTime.Value.Ticks < timingInfo.BeginTime.Ticks)
                                    {
                                        timingInfo.TaskTimer = new Timer(new TimerCallback(TaskExecute), timingInfo, 0, timingInfo.Period);
                                    }
                                    break;
                                case TaskType.Month:
                                    if (timingInfo.LastCompleteTime.Value.Ticks < timingInfo.BeginTime.Ticks)
                                    {
                                        timingInfo.TaskTimer = new Timer(new TimerCallback(TaskExecute), timingInfo, 0, timingInfo.Period);
                                    }
                                    break;
                                case TaskType.Period:
                                    timingInfo.TaskTimer = new Timer(new TimerCallback(TaskExecute), timingInfo, 0, timingInfo.Period);
                                    break;
                                default:
                                    WriteLog("任務配置資訊錯誤。");
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLog("CallEachTask:" + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 任務執行介面
        /// </summary>
        /// <param name="state"></param>
        private void TaskExecute(object state)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    TimingTaskInfo timingInfo = (TimingTaskInfo)state;
                    TimingTaskBase.TimingTaskBase timingBase = timingInfo.TaskObject;
                    timingBase.Run();

                    timingInfo.LastCompleteTime = DateTime.Now;
                    trans.Complete();
                    WriteLog(timingInfo.TaskName + ",執行成功。");
                }
                catch (Exception e)
                {
                    WriteLog("TaskExecute:" + e.Message);
                }
            }
        }


        /// <summary>
        /// 分析時間字串
        /// </summary>
        /// <param name="tkType"></param>
        /// <param name="timeString"></param>
        /// <returns></returns>
        private DateTime AnalyzeTime(TaskType tkType, string timeString)
        {
            DateTime resultTime;

            string[] timeArray = timeString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (timeArray.Length < 3 || timeArray.Length > 3)
            {
                throw new Exception("開始時間或結束時間參數設置錯誤。");
            }

            switch (tkType)
            {
                case TaskType.Day:
                    if ("*" == timeArray[1] || "*" == timeArray[2])
                    {
                        throw new Exception("開始時間或結束時間參數設置錯誤。");
                    }
                    else
                    {
                        resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                    }
                    break;
                case TaskType.Week:
                    if ("*" == timeArray[0] || "*" == timeArray[1] || "*" == timeArray[2])
                    {
                        throw new Exception("開始時間或結束時間參數設置錯誤。");
                    }
                    else
                    {
                        if ((int)DateTime.Now.DayOfWeek > int.Parse(timeArray[0]))
                        {
                            resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(7 + int.Parse(timeArray[0]) - (int)DateTime.Now.DayOfWeek).Day, int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                        }
                        else if ((int)DateTime.Now.DayOfWeek < int.Parse(timeArray[0]))
                        {
                            resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(int.Parse(timeArray[0]) - (int)DateTime.Now.DayOfWeek).Day, int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                        }
                        else
                        {
                            resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                        }
                    }
                    break;
                case TaskType.Month:
                    if ("*" == timeArray[0] || "*" == timeArray[1] || "*" == timeArray[2])
                    {
                        throw new Exception("開始時間或結束時間參數設置錯誤。");
                    }
                    else
                    {
                        if (DateTime.Now.Day > int.Parse(timeArray[0]))
                        {
                            resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-DateTime.Now.Day).AddMonths(1).AddDays(int.Parse(timeArray[0])).Day, int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                        }
                        else
                        {
                            resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(timeArray[0]), int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                        }
                    }
                    break;
                case TaskType.Period:
                    if ("*" != timeArray[0] && "*" != timeArray[1] && "*" != timeArray[2])
                    {
                        resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(timeArray[0]), int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                    }
                    else if ("*" != timeArray[1] && "*" != timeArray[2])
                    {
                        resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(timeArray[1]), int.Parse(timeArray[2]), 0);
                    }
                    else if ("*" != timeArray[2])
                    {
                        resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, int.Parse(timeArray[2]), 0);
                    }
                    else
                    {
                        resultTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, 0);
                    }
                    break;
                default:
                    resultTime = DateTime.Now;
                    break;
            }

            if (resultTime.Ticks > DateTime.Now.Ticks)
            {
                return resultTime;
            }
            else
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 寫定時任務日誌
        /// </summary>
        /// <param name="strMsg"></param>
        private void WriteLog(string strMsg)
        {
            string strFile = logPath + @"/TimingTaskLog.log";
            object objLock = new object();

            if (!File.Exists(strFile))
            {
                File.Create(strFile);
            }

            lock (objLock)
            {
                StreamWriter sw = new StreamWriter(strFile, true);

                sw.WriteLine(DateTime.Now.ToString());
                sw.WriteLine("message text:" + strMsg);
                sw.Close();
            }
        }
    }
}

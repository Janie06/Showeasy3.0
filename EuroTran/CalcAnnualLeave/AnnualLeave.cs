using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CalcAnnualLeave
{
    public class AnnualLeave : TimingTaskBase.TimingTaskBase
    {
        private static string connString;

        static AnnualLeave()
        {
            XmlDocument docConfig = new XmlDocument();
            docConfig.Load(System.Environment.CurrentDirectory + "/TimingTaskConfig.xml");
            XmlNode xnl = docConfig.SelectSingleNode(@"//Task[@ID='1']");

            connString = xnl.Attributes["connString"].Value;
        }

        public override void Run()
        {
            CalcAnnual();
        }

        private static void CalcAnnual()
        {
            DateTime dtNow = DateTime.Now;
            string strSql = @"update dbo.VocationAndJiaBanReport set ThisYearHasNian = ThisYearHasNian + 1 ";

            SqlConnection conn = new SqlConnection(connString);
            try
            {
                WriteLog("执行开始");
                conn.Open();
                SqlCommand cmmd = new SqlCommand(strSql, conn);
                cmmd.ExecuteNonQuery();
                WriteLog("ENd");
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
                conn.Close();
                conn.Dispose();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 写定时任务日志
        /// </summary>
        /// <param name="strMsg"></param>
        private static void WriteLog(string strMsg)
        {
            string strFile = @"C:/TimingTaskLog.log";
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

using Entity.Sugar;
using Euro.Transfer.Base;
using SqlSugar.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Euro.Transfer
{
    public partial class WordTransfer : Form
    {
        //用於存放任务项
        private Hashtable hashTasks = new Hashtable();

        //是否停止運行
        private bool bRun = true;

        public WordTransfer()
        {
            InitializeComponent();
            InitAsync();
            InitialTray();
        }

        #region 自定义方法

        public void RunTasks()
        {
            try
            {
                //加载工作项
                if (this.hashTasks.Count == 0)
                {
                    //获取configSections节点
                    var configSections = ServiceTools.GetConfigSections();
                    foreach (XmlNode section in configSections)
                    {
                        //过滤注释节点（如section中还包含其它节点需过滤）
                        if (section.Name.ToLower() == nameof(section))
                        {
                            //创建每个节点的配置对象
                            var sectionName = section.Attributes["name"].Value.Trim();
                            var sectionType = section.Attributes["type"].Value.Trim();

                            //程序集名称
                            var assemblyName = sectionType.Split(',')[1];
                            //完整类名
                            var classFullName = assemblyName + ".Jobs." + sectionName + ".Config";

                            //创建配置对象
                            var config = (ServiceConfig)Assembly.Load(assemblyName).CreateInstance(classFullName);

                            //创建工作对象
                            var job = (ServiceTask)Assembly.Load(config.Assembly.Split(',')[1]).CreateInstance(config.Assembly.Split(',')[0]);
                            job.ConfigObject = config;

                            //将工作对象加载进HashTable
                            this.hashTasks.Add(sectionName, job);
                        }
                    }
                }

                //执行工作项
                if (this.hashTasks.Keys.Count > 0)
                {
                    foreach (ServiceTask task in hashTasks.Values)
                    {
                        //插入一个新的请求到线程池
                        if (ThreadPool.QueueUserWorkItem(ThreadCallBack, task))
                        {
                            //方法成功排入队列
                        }
                        else
                        {
                            //失败
                        }
                    }
                }
            }
            catch (Exception error)
            {
                ServiceTools.WriteLog(ServiceBase.Errorlog_Path, error.ToString(), true);
            }
        }

        private void StopTasks()
        {
            //停止
            if (this.hashTasks != null)
            {
                this.hashTasks.Clear();
            }
        }

        /// <summary>
        /// 线程池回调方法
        /// </summary>
        /// <param name="state"></param>
        private void ThreadCallBack(Object state)
        {
            while (bRun)
            {
                ((ServiceTask)state).StartJob();
                //休眠1秒
                Thread.Sleep(1000);
            }
        }

        #endregion 自定义方法

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var user = ServiceTask.hubClient.OnlineUsers.FirstOrDefault(u => (u.OrgId == ServiceTask.hubClient.clientOrgId && u.UserId == ServiceTask.hubClient.clientId));
            if (user != null)
            {
                MessageBox.Show("小助手已經在別的機器上開啟並運行，請勿再次運行（OrgId：" + user.OrgId + "；UserId：" + user.UserId + "）");
                return;
            }
            if (ServiceTask.hubClient.connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                ServiceTask.hubClient.msgProxy.Invoke("Register", ServiceTask.hubClient.clientOrgId, ServiceTask.hubClient.clientId, ServiceTask.hubClient.clientName, true);
            }
            else
            {
                ServiceTask.hubClient.connection.Start().ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        //连接成功，调用Register方法
                        ServiceTask.hubClient.msgProxy.Invoke("Register", ServiceTask.hubClient.clientOrgId, ServiceTask.hubClient.clientId, ServiceTask.hubClient.clientName, true);
                    }
                    else
                    {
                        //MessageBox.Show("通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行");
                        ServiceTools.WriteLog(ServiceBase.Errorlog_Path, "Euro.Transfer.WordTransfer:通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行", true);
                    }
                });
            }
            btnStart.Enabled = false;
            btnEnd.Enabled = true;
            bRun = true;
            Thread.Sleep(2000); //延时两秒
            this.RunTasks();
            scrollingText1.BackgroundBrush =
                new LinearGradientBrush(this.scrollingText1.ClientRectangle,
                Color.Red, Color.Blue,
                LinearGradientMode.Horizontal);

            scrollingText1.ForeColor = Color.Yellow;
            scrollingText1.ScrollText = "運行中...";
            scrollingText1.ScrollDirection = ScrollingTextControl.ScrollDirection.LeftToRight;
            scrollingText1.Enabled = true;
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            if (ServiceTask.hubClient.connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                ServiceTask.hubClient.msgProxy.Invoke("offline");
            }
            else
            {
                ServiceTask.hubClient.connection.Start().ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        ServiceTask.hubClient.msgProxy.Invoke("offline");
                    }
                    else
                    {
                        //MessageBox.Show("通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行");
                        ServiceTools.WriteLog(ServiceBase.Errorlog_Path, "Euro.Transfer.WordTransfer:通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行", true);
                    }
                });
            }
            btnStart.Enabled = true;
            btnEnd.Enabled = false;
            bRun = false;
            this.StopTasks();
            scrollingText1.BackgroundBrush =
                new LinearGradientBrush(this.scrollingText1.ClientRectangle,
                Color.LightGray, Color.LightGray,
                LinearGradientMode.Horizontal);

            scrollingText1.ForeColor = Color.LightSlateGray;
            scrollingText1.ScrollText = "已停止";
            scrollingText1.Enabled = false;
        }

        private async void InitAsync()
        {
            var sWriteWordPath = Common.GetAppSetting("WriteWordPath");
            txtPath.Text = sWriteWordPath;
            scrollingText1.BackgroundBrush =
                new LinearGradientBrush(this.scrollingText1.ClientRectangle,
                Color.LightGray, Color.LightGray,
                LinearGradientMode.Horizontal);

            scrollingText1.ForeColor = Color.LightSlateGray;
            scrollingText1.ScrollText = "請點擊“啟動”運行小助手";
            scrollingText1.Enabled = false;
            var url = Common.GetAppSetting("EURO_MsgServerUrl");
            await ServiceTask.hubClient.RunAsync(url);
            ServiceTask.hubClient.writeOrLogs += new HubTransfer.WriteOrLogsHandler(this.WriteOrLogs);
        }

        /// <summary>
        /// 显示记录并写log
        /// </summary>
        /// <param name="msg"></param>
        private void WriteOrLogs(string text, int count)
        {
            try
            {
                this.BeginInvoke(new Action<string, int>(WriteMsg), text, count);
            }
            catch (Exception ex)
            {
                ServiceTools.WriteLog(ServiceBase.Errorlog_Path, ex.ToString(), true);
            }
        }

        private void WriteMsg(string text, int count = 0)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (this.txtSyntax.Lines.Length > 1 * 1000)
                {
                    this.txtSyntax.Text = "";
                }
                this.txtSyntax.AppendText(text);
                lbCount.Text = (count + int.Parse(lbCount.Text)).ToString();
            }
            var db = SugarBase.GetIntance();
            var iCount_Cus = db.Queryable<OTB_CRM_CustomersTransferBak>().Count();
            var iCount_Bills = db.Queryable<OTB_OPM_BillsBak>().Count();
            var iCount_Exh = db.Queryable<OTB_OPM_ExhibitionsTransferBak>().Count();
            lbTotalCount.Text = (iCount_Cus + iCount_Bills + iCount_Exh).ToString();
        }

        /// <summary>
        /// 窗体关闭的单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            //通过这里可以看出，这里的关闭其实不是真正意义上的“关闭”，而是将窗体隐藏，实现一个“伪关闭”
            this.Hide();
        }

        private void InitialTray()
        {
            //隐藏主窗体
            this.Hide();
            //true表示在托盘区可见，false表示在托盘区不可见
            notifyIcon1.Visible = true;
            //气泡显示的时间（单位是毫秒）
            notifyIcon1.ShowBalloonTip(2000);
            //notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_MouseClick);
            notifyIcon1.MouseDoubleClick += new MouseEventHandler(NotifyIcon_MouseClick);

            ////设置一级级菜单
            MenuItem home = new MenuItem("主頁面");
            home.Click += new EventHandler(ShowHome_Click);
            MenuItem exit = new MenuItem("退出");
            exit.Click += new EventHandler(Exit_Click);
            //退出菜单项
            var childen = new MenuItem[] { home, exit };
            notifyIcon1.ContextMenu = new ContextMenu(childen);

            //窗体关闭时触发
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            WriteMsg("");
        }

        /// <summary>
        /// 单击菜单"主页面"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHome_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.Activate();
        }

        /// <summary>
        /// 鼠标单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标左键单击
            if (e.Button == MouseButtons.Left)
            {
                //如果窗体是可见的，那么鼠标左击托盘区图标后，窗体为不可见
                //if (this.Visible == true)
                //{
                //    this.Visible = false;
                //}
                //else
                //{
                //    this.Visible = true;
                //    this.Activate();
                //}

                this.Visible = true;
                this.Activate();
            }
        }

        /// <summary>
        /// 退出选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, EventArgs e)
        {
            //退出程序
            Environment.Exit(0);
        }

        private void BtnSure_Click(object sender, EventArgs e)
        {
            try
            {
                var bError = true;
                do
                {
                    var dicUpdKeys = new Dictionary<string, string>
                    {
                        { "WriteWordPath", txtPath.Text }
                    };
                    foreach (string key in dicUpdKeys.Keys)
                    {
                        var bOk = Common.UpdateAppSettings(key, dicUpdKeys[key]);
                        if (!bOk)
                        {
                            bError = false;
                            break;
                        }
                    }
                }
                while (false);
                if (bError)
                {
                    MessageBox.Show("修改成功");
                }
                else
                {
                    MessageBox.Show("修改失敗");
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("修改失敗");
                ServiceTools.WriteLog(ServiceBase.Errorlog_Path, error.ToString(), true);
            }
        }
    }
}
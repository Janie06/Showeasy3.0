using Euro.Transfer.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Euro.Transfer
{
    public partial class FormTransfer : Form
    {
        //用於存放任务项
        private Hashtable hashTasks = new Hashtable();

        //是否停止運行
        private bool bRun = true;

        //这里在窗体上没有拖拽一个NotifyIcon控件，而是在这里定义了一个变量
        private NotifyIcon notifyIcon = null;

        public FormTransfer()
        {
            InitializeComponent();
            Init();
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

        private void BtnSure_Click(object sender, EventArgs e)
        {
            try
            {
                var bError = true;
                do
                {
                    var sWeeks = "";
                    var dicUpdKeys = new Dictionary<string, string>();

                    if (ckWeek_1.Checked)
                    {
                        sWeeks += "1,";
                    }
                    if (ckWeek_2.Checked)
                    {
                        sWeeks += "2,";
                    }
                    if (ckWeek_3.Checked)
                    {
                        sWeeks += "3,";
                    }
                    if (ckWeek_4.Checked)
                    {
                        sWeeks += "4,";
                    }
                    if (ckWeek_5.Checked)
                    {
                        sWeeks += "5,";
                    }
                    if (ckWeek_6.Checked)
                    {
                        sWeeks += "6,";
                    }
                    if (ckWeek_7.Checked)
                    {
                        sWeeks += "0";
                    }
                    dicUpdKeys.Add("TransferWeeks", sWeeks);
                    dicUpdKeys.Add("TransferTime", dateTime.Text);
                    dicUpdKeys.Add("WriteWordPath", txtPath.Text);
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

        private void BtnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnEnd.Enabled = true;
            bRun = true;
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

        private async void Init()
        {
            var sWriteWordPath = Common.GetAppSetting("WriteWordPath");
            var sTransferWeeks = Common.GetAppSetting("TransferWeeks");
            var sTransferTime = Common.GetAppSetting("TransferTime");
            dateTime.Text = sTransferTime;
            txtPath.Text = sWriteWordPath;
            if (sTransferWeeks.IndexOf("1") > -1)
            {
                ckWeek_1.Checked = true;
            }
            if (sTransferWeeks.IndexOf("2") > -1)
            {
                ckWeek_2.Checked = true;
            }
            if (sTransferWeeks.IndexOf("3") > -1)
            {
                ckWeek_3.Checked = true;
            }
            if (sTransferWeeks.IndexOf("4") > -1)
            {
                ckWeek_4.Checked = true;
            }
            if (sTransferWeeks.IndexOf("5") > -1)
            {
                ckWeek_5.Checked = true;
            }
            if (sTransferWeeks.IndexOf("6") > -1)
            {
                ckWeek_6.Checked = true;
            }
            if (sTransferWeeks.IndexOf("0") > -1)
            {
                ckWeek_7.Checked = true;
            }
            scrollingText1.BackgroundBrush =
                new LinearGradientBrush(this.scrollingText1.ClientRectangle,
                Color.LightGray, Color.LightGray,
                LinearGradientMode.Horizontal);

            scrollingText1.ForeColor = Color.LightSlateGray;
            scrollingText1.ScrollText = "請點擊“啟動”運行小助手";
            scrollingText1.Enabled = false;
            var url = Common.GetAppSetting("EURO_MsgServerUrl");
            await ServiceTask.hubClient.RunAsync(url);
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

            //实例化一个NotifyIcon对象
            notifyIcon = new NotifyIcon
            {
                //托盘图标气泡显示的内容
                BalloonTipText = "正在後臺運行",
                //托盘图标显示的内容
                Text = "奕達文字檔小助手"
            };
            //注意：下面的路径可以是绝对路径、相对路径。但是需要注意的是：文件必须是一个.ico格式
            var sIconPath = Application.StartupPath.ToString() + @"\eurtoran_ico.ico";
            notifyIcon.Icon = new Icon(sIconPath);
            //true表示在托盘区可见，false表示在托盘区不可见
            notifyIcon.Visible = true;
            //气泡显示的时间（单位是毫秒）
            notifyIcon.ShowBalloonTip(2000);
            //notifyIcon.MouseClick += new MouseEventHandler(notifyIcon_MouseClick);
            notifyIcon.MouseDoubleClick += new MouseEventHandler(notifyIcon_MouseClick);

            ////设置二级菜单
            //MenuItem setting1 = new MenuItem("二级菜单1");
            //MenuItem setting2 = new MenuItem("二级菜单2");
            //MenuItem setting = new MenuItem("一级菜单", new MenuItem[]{setting1,setting2});

            //帮助选项，这里只是“有名无实”在菜单上只是显示，单击没有效果，可以参照下面的“退出菜单”实现单击事件
            //MenuItem help = new MenuItem("帮助");

            ////关于选项
            //MenuItem about = new MenuItem("关于");

            //退出菜单项
            var exit = new MenuItem("退出");
            exit.Click += new EventHandler(exit_Click);

            ////关联托盘控件
            //注释的这一行与下一行的区别就是参数不同，setting这个参数是为了实现二级菜单
            //MenuItem[] childen = new MenuItem[] { setting, help, about, exit };
            var childen = new MenuItem[] { exit };
            //MenuItem[] childen = new MenuItem[] { help, about, exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);

            //窗体关闭时触发
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
        }

        /// <summary>
        /// 鼠标单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
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
        private void exit_Click(object sender, EventArgs e)
        {
            //退出程序
            Environment.Exit(0);
        }
    }
}
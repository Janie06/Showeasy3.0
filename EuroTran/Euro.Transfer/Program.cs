using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Euro.Transfer
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var processCount = 0;
            var pa = Process.GetProcesses();//獲取當前進程數組。
            foreach (Process PTest in pa)
            {
                if (PTest.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    processCount += 1;
                }
            }
            if (processCount > 1)
            {
                //如果程序已經運行，則給出提示。並退出本進程。
                DialogResult dr;
                dr = MessageBox.Show("小助手已經運行！", "退出程序", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //可能你不需要彈出窗口，在這裡可以屏蔽掉

                return; //Exit;
            }
            //GetOrig logFrm = new GetOrig();
            //if (logFrm.ShowDialog() == DialogResult.OK)//为第一次選擇公司
            //{
            //    Application.Run(new WordTransfer()); //跳轉到小助手

            //}
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var wordTransfer = new WordTransfer())
            {
                Application.Run(wordTransfer);
            }
        }
    }
}
using Euro.Transfer.Base;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Euro.Transfer
{
    public partial class GetOrig : Form
    {
        public GetOrig()
        {
            InitializeComponent();
            IList<KeyValue> lst = new List<KeyValue>();
            var kv = new KeyValue
            {
                key = "TE",
                value = "奕達"
            };
            lst.Add(kv);
            kv = new KeyValue
            {
                key = "TG",
                value = "駒驛"
            };
            lst.Add(kv);
            kv = new KeyValue
            {
                key = "SG",
                value = "上海駒驛"
            };
            lst.Add(kv);
            kv = new KeyValue
            {
                key = "TEST",
                value = "TEST"
            };
            lst.Add(kv);

            comorig.DataSource = lst;//绑定
            comorig.DisplayMember = "value";//显示的文本
            comorig.ValueMember = "key";//对应的值
            comorig.SelectedValue = "TE";
        }

        private void btnSure_Click(object sender, EventArgs e)
        {
            try
            {
                var bError = true;
                do
                {
                    var dicUpdKeys = new Dictionary<string, string>();
                    dicUpdKeys.Add("TransferOrgID", comorig.SelectedValue.ToString());
                    dicUpdKeys.Add("TransferUserID", txtID.Text);
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
                    this.DialogResult = DialogResult.OK;      //this 指向的是GetOrig
                }
                else
                {
                    MessageBox.Show("修改失敗");
                }
            }
            catch (Exception error)
            {
                ServiceTools.WriteLog(ServiceBase.Errorlog_Path, error.ToString(), true);
            }
        }
    }
}
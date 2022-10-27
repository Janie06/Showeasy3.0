using Kevin.SyntaxTextBox;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace EntityBuilder
{
    public partial class Form1 : Form
    {
        private SyntaxTextBox txtSyntax;

        public Form1()
        {
            InitializeComponent();
        }

        private List<DbTableInfo> m_tables = new List<DbTableInfo>();
        private List<DbTableInfo> m_SelTables = new List<DbTableInfo>();
        private List<ColumnInfo> m_tableColumns = new List<ColumnInfo>();

        #region "初始化datagridview数据"

        private void Form1_Load(object sender, EventArgs e)
        {
            m_tables = TableHelper.GetTables();

            m_tables = m_tables.OrderBy(u => u.Name).ToList();
            dataGridView1.DataSource = m_tables;

            //reLoadColumns("student");
        }

        #endregion "初始化datagridview数据"

        #region "查询所有数据"

        private void reLoadColumns(string tablename)
        {
            m_tableColumns.Clear();
            m_tableColumns = TableHelper.GetColumnField(tablename);
            dataGridView2.DataSource = m_tableColumns;
        }

        #endregion "查询所有数据"

        #region "选择行，并填充到表单中，然后可做修改和删除操作"

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            { //当单击複選框，同时处于组合编辑状态时
                var cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var ifcheck1 = Convert.ToBoolean(cell.FormattedValue);
                var ifcheck2 = Convert.ToBoolean(cell.EditedFormattedValue);

                if (ifcheck1 != ifcheck2)
                {
                    var tb = m_tables[e.RowIndex];
                    reLoadColumns(tb.Name);
                    var db = SugarBase.DB;
                    var cls = db.DbFirst.IsCreateAttribute().Where(tb.Name).ToClassStringList("Entity.Sugar").First();
                    var code = cls.Value;
                    txtSyntax.Text = code;
                }
            }
        }

        #endregion "选择行，并填充到表单中，然后可做修改和删除操作"

        private void btnGen_Click(object sender, EventArgs e)
        {
            m_SelTables.Clear();
            var iRow = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var iCol = 0;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (iCol == 0)
                    {
                        var ifcheck1 = Convert.ToBoolean(cell.FormattedValue);
                        if (ifcheck1)
                        {
                            var tb = m_tables[iRow];
                            m_SelTables.Add(tb);
                        }
                    }

                    iCol++;
                }

                iRow++;
            }

            if (m_SelTables.Count > 0)
            {
                var sPath = txtPath.Text.Trim();
                var db = SugarBase.DB;
                if (sPath == "")
                {
                    sPath = DBUnit.GetAppSettings("OrmSugarPath");
                }
                foreach (DbTableInfo n in m_SelTables)
                {
                    db.DbFirst.IsCreateAttribute().Where(n.Name).CreateClassFile(sPath, "Entity.Sugar");
                }
                MessageBox.Show(@"ORM(Sugar) 生成完成");
            }
            else
            {
                MessageBox.Show(@"請選擇要產生的table");
            }
        }

        private void allcheck_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                ((DataGridViewCheckBoxCell)row.Cells[0]).Value = allcheck.Checked;
            }
        }

        private void btnGen_Helper_Click(object sender, EventArgs e)
        {
            m_SelTables.Clear();
            var iRow = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var iCol = 0;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (iCol == 0)
                    {
                        var ifcheck1 = Convert.ToBoolean(cell.FormattedValue);
                        if (ifcheck1)
                        {
                            var tb = m_tables[iRow];
                            m_SelTables.Add(tb);
                        }
                    }
                    iCol++;
                }
                iRow++;
            }
            var sPath = txtPath.Text.Trim();

            if (m_SelTables.Count > 0)
            {
                var code = CreateFileHelper.BuilderEntityHelperCode(m_SelTables);
                code = code.Replace("\n", "\r\n");
                txtSyntax.Text = code;
                if (sPath == "")
                {
                    sPath = DBUnit.GetAppSettings("OrmEasyPath");
                }
                CreateFileHelper.CreateEntityHelper(m_SelTables, sPath);
            }
            else
            {
                MessageBox.Show("請選擇要產生的table");
            }
        }
    }
}

namespace Euro.Transfer
{
    partial class FormTransfer
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private ScrollingTextControl.ScrollingText scrollingText1;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTransfer));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnEnd = new System.Windows.Forms.Button();
            this.ckWeek_1 = new System.Windows.Forms.CheckBox();
            this.ckWeek_2 = new System.Windows.Forms.CheckBox();
            this.ckWeek_3 = new System.Windows.Forms.CheckBox();
            this.ckWeek_4 = new System.Windows.Forms.CheckBox();
            this.ckWeek_5 = new System.Windows.Forms.CheckBox();
            this.ckWeek_6 = new System.Windows.Forms.CheckBox();
            this.ckWeek_7 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSure = new System.Windows.Forms.Button();
            this.scrollingText1 = new ScrollingTextControl.ScrollingText();
            this.dateTime = new System.Windows.Forms.DateTimePicker();
            this.txtSyntax = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnStart.Location = new System.Drawing.Point(80, 518);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(181, 32);
            this.btnStart.TabIndex = 14;
            this.btnStart.Text = "啟動";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnEnd
            // 
            this.btnEnd.Enabled = false;
            this.btnEnd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnEnd.Location = new System.Drawing.Point(368, 518);
            this.btnEnd.Name = "btnEnd";
            this.btnEnd.Size = new System.Drawing.Size(177, 32);
            this.btnEnd.TabIndex = 15;
            this.btnEnd.Text = "停止";
            this.btnEnd.UseVisualStyleBackColor = true;
            this.btnEnd.Click += new System.EventHandler(this.BtnEnd_Click);
            // 
            // ckWeek_1
            // 
            this.ckWeek_1.AutoSize = true;
            this.ckWeek_1.Location = new System.Drawing.Point(131, 13);
            this.ckWeek_1.Name = "ckWeek_1";
            this.ckWeek_1.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_1.TabIndex = 22;
            this.ckWeek_1.Text = "週一";
            this.ckWeek_1.UseVisualStyleBackColor = true;
            // 
            // ckWeek_2
            // 
            this.ckWeek_2.AutoSize = true;
            this.ckWeek_2.Location = new System.Drawing.Point(197, 13);
            this.ckWeek_2.Name = "ckWeek_2";
            this.ckWeek_2.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_2.TabIndex = 23;
            this.ckWeek_2.Text = "週二";
            this.ckWeek_2.UseVisualStyleBackColor = true;
            // 
            // ckWeek_3
            // 
            this.ckWeek_3.AutoSize = true;
            this.ckWeek_3.Location = new System.Drawing.Point(266, 13);
            this.ckWeek_3.Name = "ckWeek_3";
            this.ckWeek_3.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_3.TabIndex = 24;
            this.ckWeek_3.Text = "周三";
            this.ckWeek_3.UseVisualStyleBackColor = true;
            // 
            // ckWeek_4
            // 
            this.ckWeek_4.AutoSize = true;
            this.ckWeek_4.Location = new System.Drawing.Point(330, 13);
            this.ckWeek_4.Name = "ckWeek_4";
            this.ckWeek_4.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_4.TabIndex = 25;
            this.ckWeek_4.Text = "週四";
            this.ckWeek_4.UseVisualStyleBackColor = true;
            // 
            // ckWeek_5
            // 
            this.ckWeek_5.AutoSize = true;
            this.ckWeek_5.Location = new System.Drawing.Point(393, 13);
            this.ckWeek_5.Name = "ckWeek_5";
            this.ckWeek_5.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_5.TabIndex = 26;
            this.ckWeek_5.Text = "週五";
            this.ckWeek_5.UseVisualStyleBackColor = true;
            // 
            // ckWeek_6
            // 
            this.ckWeek_6.AutoSize = true;
            this.ckWeek_6.Location = new System.Drawing.Point(458, 13);
            this.ckWeek_6.Name = "ckWeek_6";
            this.ckWeek_6.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_6.TabIndex = 27;
            this.ckWeek_6.Text = "週六";
            this.ckWeek_6.UseVisualStyleBackColor = true;
            // 
            // ckWeek_7
            // 
            this.ckWeek_7.AutoSize = true;
            this.ckWeek_7.Location = new System.Drawing.Point(524, 13);
            this.ckWeek_7.Name = "ckWeek_7";
            this.ckWeek_7.Size = new System.Drawing.Size(48, 16);
            this.ckWeek_7.TabIndex = 28;
            this.ckWeek_7.Text = "周日";
            this.ckWeek_7.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(43, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 21);
            this.label1.TabIndex = 29;
            this.label1.Text = "執行日：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label2.Location = new System.Drawing.Point(27, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 21);
            this.label2.TabIndex = 30;
            this.label2.Text = "執行時間：";
            // 
            // btnSure
            // 
            this.btnSure.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSure.Location = new System.Drawing.Point(131, 126);
            this.btnSure.Name = "btnSure";
            this.btnSure.Size = new System.Drawing.Size(216, 36);
            this.btnSure.TabIndex = 32;
            this.btnSure.Text = "確認";
            this.btnSure.UseVisualStyleBackColor = true;
            this.btnSure.Click += new System.EventHandler(this.BtnSure_Click);
            // 
            // scrollingText1
            // 
            this.scrollingText1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollingText1.BackgroundBrush = null;
            this.scrollingText1.BorderColor = System.Drawing.Color.Black;
            this.scrollingText1.Cursor = System.Windows.Forms.Cursors.Default;
            this.scrollingText1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scrollingText1.ForegroundBrush = null;
            this.scrollingText1.Location = new System.Drawing.Point(-4, 557);
            this.scrollingText1.Name = "scrollingText1";
            this.scrollingText1.ScrollDirection = ScrollingTextControl.ScrollDirection.Bouncing;
            this.scrollingText1.ScrollText = "請點擊“啟動”運行小助手";
            this.scrollingText1.ShowBorder = true;
            this.scrollingText1.Size = new System.Drawing.Size(637, 43);
            this.scrollingText1.StopScrollOnMouseOver = true;
            this.scrollingText1.TabIndex = 5;
            this.scrollingText1.Text = "scrollingText1";
            this.scrollingText1.TextScrollDistance = 2;
            this.scrollingText1.TextScrollSpeed = 25;
            this.scrollingText1.VerticleTextPosition = ScrollingTextControl.VerticleTextPosition.Center;
            // 
            // dateTime
            // 
            this.dateTime.CalendarFont = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTime.CustomFormat = "HH:mm";
            this.dateTime.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTime.Location = new System.Drawing.Point(130, 50);
            this.dateTime.Name = "dateTime";
            this.dateTime.ShowUpDown = true;
            this.dateTime.Size = new System.Drawing.Size(200, 26);
            this.dateTime.TabIndex = 33;
            // 
            // txtSyntax
            // 
            this.txtSyntax.Location = new System.Drawing.Point(45, 177);
            this.txtSyntax.Name = "txtSyntax";
            this.txtSyntax.Size = new System.Drawing.Size(539, 333);
            this.txtSyntax.TabIndex = 34;
            this.txtSyntax.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label3.Location = new System.Drawing.Point(12, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 21);
            this.label3.TabIndex = 35;
            this.label3.Text = "文字檔路徑：";
            // 
            // txtPath
            // 
            this.txtPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPath.ForeColor = System.Drawing.Color.Blue;
            this.txtPath.Location = new System.Drawing.Point(131, 86);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(433, 29);
            this.txtPath.TabIndex = 36;
            this.txtPath.Text = "C:\\Words";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 598);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSyntax);
            this.Controls.Add(this.dateTime);
            this.Controls.Add(this.btnSure);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ckWeek_7);
            this.Controls.Add(this.ckWeek_6);
            this.Controls.Add(this.ckWeek_5);
            this.Controls.Add(this.ckWeek_4);
            this.Controls.Add(this.ckWeek_3);
            this.Controls.Add(this.ckWeek_2);
            this.Controls.Add(this.ckWeek_1);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.scrollingText1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "奕達文字檔小助手";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.CheckBox ckWeek_1;
        private System.Windows.Forms.CheckBox ckWeek_2;
        private System.Windows.Forms.CheckBox ckWeek_3;
        private System.Windows.Forms.CheckBox ckWeek_4;
        private System.Windows.Forms.CheckBox ckWeek_5;
        private System.Windows.Forms.CheckBox ckWeek_6;
        private System.Windows.Forms.CheckBox ckWeek_7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSure;
        private System.Windows.Forms.DateTimePicker dateTime;
        private System.Windows.Forms.RichTextBox txtSyntax;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPath;
    }
}


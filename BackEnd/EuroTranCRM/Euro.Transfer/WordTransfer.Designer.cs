namespace Euro.Transfer
{
    partial class WordTransfer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WordTransfer));
            this.txtSyntax = new System.Windows.Forms.RichTextBox();
            this.btnEnd = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.scrollingText1 = new ScrollingTextControl.ScrollingText();
            this.label3 = new System.Windows.Forms.Label();
            this.lbTotalCount = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSure = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.lbCount = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.SuspendLayout();
            // 
            // txtSyntax
            // 
            this.txtSyntax.Location = new System.Drawing.Point(63, 122);
            this.txtSyntax.Margin = new System.Windows.Forms.Padding(4);
            this.txtSyntax.Name = "txtSyntax";
            this.txtSyntax.Size = new System.Drawing.Size(717, 474);
            this.txtSyntax.TabIndex = 35;
            this.txtSyntax.Text = "";
            // 
            // btnEnd
            // 
            this.btnEnd.Enabled = false;
            this.btnEnd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnEnd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnEnd.Location = new System.Drawing.Point(483, 624);
            this.btnEnd.Margin = new System.Windows.Forms.Padding(4);
            this.btnEnd.Name = "btnEnd";
            this.btnEnd.Size = new System.Drawing.Size(236, 40);
            this.btnEnd.TabIndex = 38;
            this.btnEnd.Text = "停止";
            this.btnEnd.UseVisualStyleBackColor = true;
            this.btnEnd.Click += new System.EventHandler(this.BtnEnd_Click);
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnStart.Location = new System.Drawing.Point(121, 624);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(241, 40);
            this.btnStart.TabIndex = 37;
            this.btnStart.Text = "啟動";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
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
            this.scrollingText1.Location = new System.Drawing.Point(-3, 672);
            this.scrollingText1.Margin = new System.Windows.Forms.Padding(4);
            this.scrollingText1.Name = "scrollingText1";
            this.scrollingText1.ScrollDirection = ScrollingTextControl.ScrollDirection.Bouncing;
            this.scrollingText1.ScrollText = "請點擊“啟動”運行小助手";
            this.scrollingText1.ShowBorder = true;
            this.scrollingText1.Size = new System.Drawing.Size(855, 54);
            this.scrollingText1.StopScrollOnMouseOver = true;
            this.scrollingText1.TabIndex = 36;
            this.scrollingText1.Text = "scrollingText1";
            this.scrollingText1.TextScrollDistance = 2;
            this.scrollingText1.TextScrollSpeed = 25;
            this.scrollingText1.VerticleTextPosition = ScrollingTextControl.VerticleTextPosition.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label3.Location = new System.Drawing.Point(71, 75);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 27);
            this.label3.TabIndex = 39;
            this.label3.Text = "累計轉檔資料筆數：";
            // 
            // lbTotalCount
            // 
            this.lbTotalCount.AutoSize = true;
            this.lbTotalCount.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbTotalCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbTotalCount.Location = new System.Drawing.Point(268, 76);
            this.lbTotalCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbTotalCount.Name = "lbTotalCount";
            this.lbTotalCount.Size = new System.Drawing.Size(24, 27);
            this.lbTotalCount.TabIndex = 40;
            this.lbTotalCount.Text = "0";
            // 
            // txtPath
            // 
            this.txtPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPath.ForeColor = System.Drawing.Color.Blue;
            this.txtPath.Location = new System.Drawing.Point(213, 20);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(472, 34);
            this.txtPath.TabIndex = 43;
            this.txtPath.Text = "C:\\Words";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(68, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 27);
            this.label1.TabIndex = 42;
            this.label1.Text = "文字檔路徑：";
            // 
            // btnSure
            // 
            this.btnSure.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnSure.Location = new System.Drawing.Point(693, 19);
            this.btnSure.Margin = new System.Windows.Forms.Padding(4);
            this.btnSure.Name = "btnSure";
            this.btnSure.Size = new System.Drawing.Size(84, 38);
            this.btnSure.TabIndex = 41;
            this.btnSure.Text = "確認";
            this.btnSure.UseVisualStyleBackColor = true;
            this.btnSure.Click += new System.EventHandler(this.BtnSure_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "奕達小助手";
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "正在运行";
            this.notifyIcon1.Visible = true;
            // 
            // lbCount
            // 
            this.lbCount.AutoSize = true;
            this.lbCount.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lbCount.Location = new System.Drawing.Point(630, 76);
            this.lbCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbCount.Name = "lbCount";
            this.lbCount.Size = new System.Drawing.Size(24, 27);
            this.lbCount.TabIndex = 45;
            this.lbCount.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label4.Location = new System.Drawing.Point(431, 75);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(192, 27);
            this.label4.TabIndex = 44;
            this.label4.Text = "當前處理資料筆數：";
            // 
            // WordTransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 720);
            this.Controls.Add(this.lbCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSure);
            this.Controls.Add(this.lbTotalCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnEnd);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.scrollingText1);
            this.Controls.Add(this.txtSyntax);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "WordTransfer";
            this.Text = "WordTransfer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtSyntax;
        private System.Windows.Forms.Button btnEnd;
        private System.Windows.Forms.Button btnStart;
        private ScrollingTextControl.ScrollingText scrollingText1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbTotalCount;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSure;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label lbCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}
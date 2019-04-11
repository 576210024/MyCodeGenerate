namespace LenovoCodeGenerate
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.rdioOracle = new System.Windows.Forms.RadioButton();
            this.rdioSqlserver = new System.Windows.Forms.RadioButton();
            this.txtDisk = new System.Windows.Forms.TextBox();
            this.txtAuthName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtProxy = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSolutionName = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTabLike1 = new System.Windows.Forms.TextBox();
            this.txtTabLike2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(209, 237);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "生成主体 2-9";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // rdioOracle
            // 
            this.rdioOracle.AutoSize = true;
            this.rdioOracle.Checked = true;
            this.rdioOracle.Location = new System.Drawing.Point(170, 23);
            this.rdioOracle.Name = "rdioOracle";
            this.rdioOracle.Size = new System.Drawing.Size(59, 16);
            this.rdioOracle.TabIndex = 1;
            this.rdioOracle.TabStop = true;
            this.rdioOracle.Text = "ORACLE";
            this.rdioOracle.UseVisualStyleBackColor = true;
            // 
            // rdioSqlserver
            // 
            this.rdioSqlserver.AutoSize = true;
            this.rdioSqlserver.Location = new System.Drawing.Point(271, 23);
            this.rdioSqlserver.Name = "rdioSqlserver";
            this.rdioSqlserver.Size = new System.Drawing.Size(77, 16);
            this.rdioSqlserver.TabIndex = 2;
            this.rdioSqlserver.Text = "SQLSERVER";
            this.rdioSqlserver.UseVisualStyleBackColor = true;
            // 
            // txtDisk
            // 
            this.txtDisk.Location = new System.Drawing.Point(170, 51);
            this.txtDisk.Name = "txtDisk";
            this.txtDisk.Size = new System.Drawing.Size(219, 21);
            this.txtDisk.TabIndex = 3;
            this.txtDisk.Text = "D:\\\\";
            // 
            // txtAuthName
            // 
            this.txtAuthName.Location = new System.Drawing.Point(170, 111);
            this.txtAuthName.Name = "txtAuthName";
            this.txtAuthName.Size = new System.Drawing.Size(219, 21);
            this.txtAuthName.TabIndex = 4;
            this.txtAuthName.Text = "张继阳";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "输入工程 存放 的盘符：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "输入你的名字：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "选择你要生成的DB种类：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 146);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "输入你的代理基类：";
            // 
            // txtProxy
            // 
            this.txtProxy.Location = new System.Drawing.Point(170, 143);
            this.txtProxy.Name = "txtProxy";
            this.txtProxy.Size = new System.Drawing.Size(219, 21);
            this.txtProxy.TabIndex = 8;
            this.txtProxy.Text = "ProxyConsultationBase";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "输 入 你 的 工 程 名：";
            // 
            // txtSolutionName
            // 
            this.txtSolutionName.Location = new System.Drawing.Point(170, 81);
            this.txtSolutionName.Name = "txtSolutionName";
            this.txtSolutionName.Size = new System.Drawing.Size(219, 21);
            this.txtSolutionName.TabIndex = 10;
            this.txtSolutionName.Text = "Lenovo.CIS.Consultation";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox1.Location = new System.Drawing.Point(415, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(313, 248);
            this.textBox1.TabIndex = 13;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 180);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 12);
            this.label6.TabIndex = 15;
            this.label6.Text = "表集合过滤条件：";
            // 
            // txtTabLike1
            // 
            this.txtTabLike1.Location = new System.Drawing.Point(170, 177);
            this.txtTabLike1.Name = "txtTabLike1";
            this.txtTabLike1.Size = new System.Drawing.Size(98, 21);
            this.txtTabLike1.TabIndex = 14;
            this.txtTabLike1.Text = "HD_CONSUL";
            // 
            // txtTabLike2
            // 
            this.txtTabLike2.Location = new System.Drawing.Point(285, 177);
            this.txtTabLike2.Name = "txtTabLike2";
            this.txtTabLike2.Size = new System.Drawing.Size(104, 21);
            this.txtTabLike2.TabIndex = 16;
            this.txtTabLike2.Text = "ITEMS";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(302, 237);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 23);
            this.button2.TabIndex = 17;
            this.button2.Text = "生成其他 10";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 270);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtTabLike2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtTabLike1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSolutionName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtProxy);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtAuthName);
            this.Controls.Add(this.txtDisk);
            this.Controls.Add(this.rdioSqlserver);
            this.Controls.Add(this.rdioOracle);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "联想医疗代码生成器-基本版";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RadioButton rdioOracle;
        private System.Windows.Forms.RadioButton rdioSqlserver;
        private System.Windows.Forms.TextBox txtDisk;
        private System.Windows.Forms.TextBox txtAuthName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtProxy;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSolutionName;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTabLike1;
        private System.Windows.Forms.TextBox txtTabLike2;
        private System.Windows.Forms.Button button2;
    }
}


namespace practice
{
    partial class Form_Z3950
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.radioButton_authenStyleIdpass = new System.Windows.Forms.RadioButton();
            this.radioButton_authenStyleOpen = new System.Windows.Forms.RadioButton();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel_query = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_groupID = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_database = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serverPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.button_search = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.button_nextBatch = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.radioButton_query_origin = new System.Windows.Forms.RadioButton();
            this.radioButton_query_easy = new System.Windows.Forms.RadioButton();
            this.textBox_queryString = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_use = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel_query.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(24, 27);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(731, 1104);
            this.webBrowser1.TabIndex = 0;
            // 
            // radioButton_authenStyleIdpass
            // 
            this.radioButton_authenStyleIdpass.AutoSize = true;
            this.radioButton_authenStyleIdpass.Location = new System.Drawing.Point(26, 83);
            this.radioButton_authenStyleIdpass.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton_authenStyleIdpass.Name = "radioButton_authenStyleIdpass";
            this.radioButton_authenStyleIdpass.Size = new System.Drawing.Size(141, 31);
            this.radioButton_authenStyleIdpass.TabIndex = 1;
            this.radioButton_authenStyleIdpass.TabStop = true;
            this.radioButton_authenStyleIdpass.Text = "&ID/Pass";
            this.radioButton_authenStyleIdpass.UseVisualStyleBackColor = true;
            // 
            // radioButton_authenStyleOpen
            // 
            this.radioButton_authenStyleOpen.AutoSize = true;
            this.radioButton_authenStyleOpen.Location = new System.Drawing.Point(26, 43);
            this.radioButton_authenStyleOpen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton_authenStyleOpen.Name = "radioButton_authenStyleOpen";
            this.radioButton_authenStyleOpen.Size = new System.Drawing.Size(99, 31);
            this.radioButton_authenStyleOpen.TabIndex = 0;
            this.radioButton_authenStyleOpen.TabStop = true;
            this.radioButton_authenStyleOpen.Text = "&Open";
            this.radioButton_authenStyleOpen.UseVisualStyleBackColor = true;
            // 
            // textBox_password
            // 
            this.textBox_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_password.Location = new System.Drawing.Point(159, 425);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(272, 38);
            this.textBox_password.TabIndex = 24;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel_query);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(1487, 1104);
            this.splitContainer1.SplitterDistance = 746;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 7;
            // 
            // panel_query
            // 
            this.panel_query.AutoScroll = true;
            this.panel_query.Controls.Add(this.radioButton_query_origin);
            this.panel_query.Controls.Add(this.radioButton_query_easy);
            this.panel_query.Controls.Add(this.textBox_queryString);
            this.panel_query.Controls.Add(this.label9);
            this.panel_query.Controls.Add(this.comboBox_use);
            this.panel_query.Controls.Add(this.label5);
            this.panel_query.Controls.Add(this.textBox_queryWord);
            this.panel_query.Controls.Add(this.label4);
            this.panel_query.Controls.Add(this.button_stop);
            this.panel_query.Controls.Add(this.textBox_password);
            this.panel_query.Controls.Add(this.label7);
            this.panel_query.Controls.Add(this.textBox_userName);
            this.panel_query.Controls.Add(this.label6);
            this.panel_query.Controls.Add(this.textBox_groupID);
            this.panel_query.Controls.Add(this.label8);
            this.panel_query.Controls.Add(this.groupBox1);
            this.panel_query.Controls.Add(this.button_nextBatch);
            this.panel_query.Controls.Add(this.button_close);
            this.panel_query.Controls.Add(this.textBox_database);
            this.panel_query.Controls.Add(this.button_search);
            this.panel_query.Controls.Add(this.label1);
            this.panel_query.Controls.Add(this.textBox_serverAddr);
            this.panel_query.Controls.Add(this.label2);
            this.panel_query.Controls.Add(this.textBox_serverPort);
            this.panel_query.Controls.Add(this.label3);
            this.panel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_query.Location = new System.Drawing.Point(0, 0);
            this.panel_query.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel_query.Name = "panel_query";
            this.panel_query.Size = new System.Drawing.Size(746, 1104);
            this.panel_query.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 427);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(122, 27);
            this.label7.TabIndex = 23;
            this.label7.Text = "密码(&P):";
            // 
            // textBox_userName
            // 
            this.textBox_userName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_userName.Location = new System.Drawing.Point(159, 379);
            this.textBox_userName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(272, 38);
            this.textBox_userName.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 383);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(149, 27);
            this.label6.TabIndex = 21;
            this.label6.Text = "用户名(&U):";
            // 
            // textBox_groupID
            // 
            this.textBox_groupID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_groupID.Location = new System.Drawing.Point(159, 336);
            this.textBox_groupID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_groupID.Name = "textBox_groupID";
            this.textBox_groupID.Size = new System.Drawing.Size(272, 38);
            this.textBox_groupID.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 339);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(138, 27);
            this.label8.TabIndex = 19;
            this.label8.Text = "&Group ID:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_authenStyleIdpass);
            this.groupBox1.Controls.Add(this.radioButton_authenStyleOpen);
            this.groupBox1.Location = new System.Drawing.Point(18, 167);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(414, 155);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 权限验证方式 ";
            // 
            // textBox_database
            // 
            this.textBox_database.Location = new System.Drawing.Point(138, 104);
            this.textBox_database.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_database.Name = "textBox_database";
            this.textBox_database.Size = new System.Drawing.Size(413, 38);
            this.textBox_database.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "地址:";
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Location = new System.Drawing.Point(138, 13);
            this.textBox_serverAddr.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(413, 38);
            this.textBox_serverAddr.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 63);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 27);
            this.label2.TabIndex = 2;
            this.label2.Text = "端口号:";
            // 
            // textBox_serverPort
            // 
            this.textBox_serverPort.Location = new System.Drawing.Point(138, 59);
            this.textBox_serverPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_serverPort.Name = "textBox_serverPort";
            this.textBox_serverPort.Size = new System.Drawing.Size(413, 38);
            this.textBox_serverPort.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 107);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 27);
            this.label3.TabIndex = 4;
            this.label3.Text = "数据库名:";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(291, 35);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1487, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 32);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 1129);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 17, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1487, 46);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(150, 769);
            this.button_search.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(133, 40);
            this.button_search.TabIndex = 14;
            this.button_search.Text = "检索";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(289, 817);
            this.button_close.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(133, 40);
            this.button_close.TabIndex = 17;
            this.button_close.Text = "切断通道";
            this.button_close.UseVisualStyleBackColor = true;
            // 
            // button_nextBatch
            // 
            this.button_nextBatch.Enabled = false;
            this.button_nextBatch.Location = new System.Drawing.Point(150, 817);
            this.button_nextBatch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_nextBatch.Name = "button_nextBatch";
            this.button_nextBatch.Size = new System.Drawing.Size(133, 40);
            this.button_nextBatch.TabIndex = 16;
            this.button_nextBatch.Text = ">>";
            this.button_nextBatch.UseVisualStyleBackColor = true;
            // 
            // button_stop
            // 
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(289, 769);
            this.button_stop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(133, 40);
            this.button_stop.TabIndex = 15;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            // 
            // radioButton_query_origin
            // 
            this.radioButton_query_origin.AutoSize = true;
            this.radioButton_query_origin.Location = new System.Drawing.Point(23, 664);
            this.radioButton_query_origin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton_query_origin.Name = "radioButton_query_origin";
            this.radioButton_query_origin.Size = new System.Drawing.Size(151, 31);
            this.radioButton_query_origin.TabIndex = 30;
            this.radioButton_query_origin.Text = "原始方式";
            this.radioButton_query_origin.UseVisualStyleBackColor = true;
            // 
            // radioButton_query_easy
            // 
            this.radioButton_query_easy.AutoSize = true;
            this.radioButton_query_easy.Checked = true;
            this.radioButton_query_easy.Location = new System.Drawing.Point(19, 534);
            this.radioButton_query_easy.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton_query_easy.Name = "radioButton_query_easy";
            this.radioButton_query_easy.Size = new System.Drawing.Size(151, 31);
            this.radioButton_query_easy.TabIndex = 25;
            this.radioButton_query_easy.TabStop = true;
            this.radioButton_query_easy.Text = "易用方式";
            this.radioButton_query_easy.UseVisualStyleBackColor = true;
            // 
            // textBox_queryString
            // 
            this.textBox_queryString.Enabled = false;
            this.textBox_queryString.Location = new System.Drawing.Point(154, 702);
            this.textBox_queryString.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_queryString.Name = "textBox_queryString";
            this.textBox_queryString.Size = new System.Drawing.Size(402, 38);
            this.textBox_queryString.TabIndex = 32;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(37, 706);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 27);
            this.label9.TabIndex = 31;
            this.label9.Text = "检索式:";
            // 
            // comboBox_use
            // 
            this.comboBox_use.FormattingEnabled = true;
            this.comboBox_use.Location = new System.Drawing.Point(154, 618);
            this.comboBox_use.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_use.Name = "comboBox_use";
            this.comboBox_use.Size = new System.Drawing.Size(402, 35);
            this.comboBox_use.TabIndex = 29;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(37, 622);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 27);
            this.label5.TabIndex = 28;
            this.label5.Text = "检索途径:";
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Location = new System.Drawing.Point(154, 572);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(402, 38);
            this.textBox_queryWord.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 576);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 27);
            this.label4.TabIndex = 26;
            this.label4.Text = "检索词:";
            // 
            // Form_Z3950
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1487, 1175);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form_Z3950";
            this.Text = "Form_Z3950";
            this.Load += new System.EventHandler(this.Form_Z3950_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel_query.ResumeLayout(false);
            this.panel_query.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.RadioButton radioButton_authenStyleIdpass;
        private System.Windows.Forms.RadioButton radioButton_authenStyleOpen;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel_query;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_groupID;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_database;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_serverAddr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_serverPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.RadioButton radioButton_query_origin;
        private System.Windows.Forms.RadioButton radioButton_query_easy;
        private System.Windows.Forms.TextBox textBox_queryString;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBox_use;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_nextBatch;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Button button_search;
    }
}
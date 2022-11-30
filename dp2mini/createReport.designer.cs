namespace dp2mini
{
    partial class createReport
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
            this.button_close = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_outputDir = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.dateTimePicker_start = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_end = new System.Windows.Forms.DateTimePicker();
            this.comboBox_quickSetFilenames = new System.Windows.Forms.ComboBox();
            this.button_selectBatcodeFile = new System.Windows.Forms.Button();
            this.textBox_patronBarcode = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_state = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_paiming = new System.Windows.Forms.Button();
            this.button_createXml = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button_xml2html = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_close
            // 
            this.button_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_close.Location = new System.Drawing.Point(943, 668);
            this.button_close.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(133, 41);
            this.button_close.TabIndex = 1;
            this.button_close.Text = "关闭";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(26, 83);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 27);
            this.label4.TabIndex = 41;
            this.label4.Text = "证条码号：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(26, 260);
            this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(147, 27);
            this.label5.TabIndex = 42;
            this.label5.Text = "日期范围：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(693, 261);
            this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 27);
            this.label2.TabIndex = 38;
            this.label2.Text = "至";
            // 
            // textBox_outputDir
            // 
            this.textBox_outputDir.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_outputDir.Location = new System.Drawing.Point(215, 30);
            this.textBox_outputDir.Name = "textBox_outputDir";
            this.textBox_outputDir.ReadOnly = true;
            this.textBox_outputDir.Size = new System.Drawing.Size(785, 38);
            this.textBox_outputDir.TabIndex = 44;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(26, 35);
            this.label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(201, 27);
            this.label7.TabIndex = 45;
            this.label7.Text = "报表输出目录：";
            // 
            // dateTimePicker_start
            // 
            this.dateTimePicker_start.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePicker_start.Location = new System.Drawing.Point(440, 256);
            this.dateTimePicker_start.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker_start.Name = "dateTimePicker_start";
            this.dateTimePicker_start.Size = new System.Drawing.Size(249, 38);
            this.dateTimePicker_start.TabIndex = 36;
            // 
            // dateTimePicker_end
            // 
            this.dateTimePicker_end.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePicker_end.Location = new System.Drawing.Point(741, 256);
            this.dateTimePicker_end.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker_end.Name = "dateTimePicker_end";
            this.dateTimePicker_end.Size = new System.Drawing.Size(249, 38);
            this.dateTimePicker_end.TabIndex = 37;
            // 
            // comboBox_quickSetFilenames
            // 
            this.comboBox_quickSetFilenames.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBox_quickSetFilenames.FormattingEnabled = true;
            this.comboBox_quickSetFilenames.Items.AddRange(new object[] {
            "今天",
            "本周",
            "本月",
            "本年",
            "最近 7 天",
            "最近 30 天",
            "最近 31 天",
            "最近 365 天",
            "最近 10 年"});
            this.comboBox_quickSetFilenames.Location = new System.Drawing.Point(215, 258);
            this.comboBox_quickSetFilenames.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.comboBox_quickSetFilenames.Name = "comboBox_quickSetFilenames";
            this.comboBox_quickSetFilenames.Size = new System.Drawing.Size(215, 35);
            this.comboBox_quickSetFilenames.TabIndex = 39;
            this.comboBox_quickSetFilenames.SelectedIndexChanged += new System.EventHandler(this.comboBox_quickSetFilenames_SelectedIndexChanged);
            // 
            // button_selectBatcodeFile
            // 
            this.button_selectBatcodeFile.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_selectBatcodeFile.Location = new System.Drawing.Point(741, 83);
            this.button_selectBatcodeFile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_selectBatcodeFile.Name = "button_selectBatcodeFile";
            this.button_selectBatcodeFile.Size = new System.Drawing.Size(259, 55);
            this.button_selectBatcodeFile.TabIndex = 43;
            this.button_selectBatcodeFile.Text = "选择条码号文件...";
            this.button_selectBatcodeFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_selectBatcodeFile.UseVisualStyleBackColor = true;
            this.button_selectBatcodeFile.Click += new System.EventHandler(this.button_selectBatcodeFile_Click);
            // 
            // textBox_patronBarcode
            // 
            this.textBox_patronBarcode.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_patronBarcode.Location = new System.Drawing.Point(217, 83);
            this.textBox_patronBarcode.Multiline = true;
            this.textBox_patronBarcode.Name = "textBox_patronBarcode";
            this.textBox_patronBarcode.Size = new System.Drawing.Size(515, 157);
            this.textBox_patronBarcode.TabIndex = 40;
            this.textBox_patronBarcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_patronBarcode_KeyDown);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(36, 36);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_state});
            this.statusStrip1.Location = new System.Drawing.Point(0, 675);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1089, 46);
            this.statusStrip1.TabIndex = 46;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_state
            // 
            this.toolStripStatusLabel_state.Name = "toolStripStatusLabel_state";
            this.toolStripStatusLabel_state.Size = new System.Drawing.Size(65, 35);
            this.toolStripStatusLabel_state.Text = "info";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(31, 507);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1021, 58);
            this.progressBar1.TabIndex = 47;
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(327, 349);
            this.button_stop.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(129, 58);
            this.button_stop.TabIndex = 49;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_paiming
            // 
            this.button_paiming.Location = new System.Drawing.Point(312, 419);
            this.button_paiming.Name = "button_paiming";
            this.button_paiming.Size = new System.Drawing.Size(231, 58);
            this.button_paiming.TabIndex = 51;
            this.button_paiming.Text = "按借阅量排名";
            this.button_paiming.UseVisualStyleBackColor = true;
            this.button_paiming.Click += new System.EventHandler(this.button_paiming_Click);
            // 
            // button_createXml
            // 
            this.button_createXml.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_createXml.Location = new System.Drawing.Point(31, 419);
            this.button_createXml.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_createXml.Name = "button_createXml";
            this.button_createXml.Size = new System.Drawing.Size(271, 58);
            this.button_createXml.TabIndex = 50;
            this.button_createXml.Text = "生成借阅报表xml";
            this.button_createXml.UseVisualStyleBackColor = true;
            this.button_createXml.Click += new System.EventHandler(this.button_createXml_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(31, 349);
            this.button2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(271, 58);
            this.button2.TabIndex = 52;
            this.button2.Text = "一键生成";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button_xml2html
            // 
            this.button_xml2html.Location = new System.Drawing.Point(587, 417);
            this.button_xml2html.Name = "button_xml2html";
            this.button_xml2html.Size = new System.Drawing.Size(231, 58);
            this.button_xml2html.TabIndex = 53;
            this.button_xml2html.Text = "xml2html";
            this.button_xml2html.UseVisualStyleBackColor = true;
            this.button_xml2html.Click += new System.EventHandler(this.button_xml2html_Click);
            // 
            // createReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 33F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 721);
            this.Controls.Add(this.button_xml2html);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_paiming);
            this.Controls.Add(this.button_createXml);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_outputDir);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.dateTimePicker_start);
            this.Controls.Add(this.dateTimePicker_end);
            this.Controls.Add(this.comboBox_quickSetFilenames);
            this.Controls.Add(this.button_selectBatcodeFile);
            this.Controls.Add(this.textBox_patronBarcode);
            this.Controls.Add(this.button_close);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "createReport";
            this.Text = "info";
            this.Load += new System.EventHandler(this.createReport_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_outputDir;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DateTimePicker dateTimePicker_start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_end;
        private System.Windows.Forms.ComboBox comboBox_quickSetFilenames;
        private System.Windows.Forms.Button button_selectBatcodeFile;
        private System.Windows.Forms.TextBox textBox_patronBarcode;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_state;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_paiming;
        private System.Windows.Forms.Button button_createXml;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button_xml2html;
    }
}
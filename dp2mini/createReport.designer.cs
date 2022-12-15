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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.button_selectDir = new System.Windows.Forms.Button();
            this.label_info = new System.Windows.Forms.Label();
            this.button_plan = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(16, 96);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 33);
            this.label4.TabIndex = 41;
            this.label4.Text = "证条码号：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(16, 332);
            this.label5.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(180, 33);
            this.label5.TabIndex = 42;
            this.label5.Text = "日期范围：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(520, 332);
            this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 33);
            this.label2.TabIndex = 38;
            this.label2.Text = "至";
            // 
            // textBox_outputDir
            // 
            this.textBox_outputDir.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_outputDir.Location = new System.Drawing.Point(190, 23);
            this.textBox_outputDir.Name = "textBox_outputDir";
            this.textBox_outputDir.Size = new System.Drawing.Size(781, 44);
            this.textBox_outputDir.TabIndex = 44;
            this.textBox_outputDir.TextChanged += new System.EventHandler(this.textBox_outputDir_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(16, 34);
            this.label7.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(180, 33);
            this.label7.TabIndex = 45;
            this.label7.Text = "报表目录：";
            // 
            // dateTimePicker_start
            // 
            this.dateTimePicker_start.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePicker_start.Location = new System.Drawing.Point(190, 323);
            this.dateTimePicker_start.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker_start.Name = "dateTimePicker_start";
            this.dateTimePicker_start.Size = new System.Drawing.Size(320, 44);
            this.dateTimePicker_start.TabIndex = 36;
            // 
            // dateTimePicker_end
            // 
            this.dateTimePicker_end.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dateTimePicker_end.Location = new System.Drawing.Point(578, 323);
            this.dateTimePicker_end.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dateTimePicker_end.Name = "dateTimePicker_end";
            this.dateTimePicker_end.Size = new System.Drawing.Size(312, 44);
            this.dateTimePicker_end.TabIndex = 37;
            // 
            // comboBox_quickSetFilenames
            // 
            this.comboBox_quickSetFilenames.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
            this.comboBox_quickSetFilenames.Location = new System.Drawing.Point(315, 391);
            this.comboBox_quickSetFilenames.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.comboBox_quickSetFilenames.Name = "comboBox_quickSetFilenames";
            this.comboBox_quickSetFilenames.Size = new System.Drawing.Size(575, 41);
            this.comboBox_quickSetFilenames.TabIndex = 39;
            this.comboBox_quickSetFilenames.SelectedIndexChanged += new System.EventHandler(this.comboBox_quickSetFilenames_SelectedIndexChanged);
            // 
            // button_selectBatcodeFile
            // 
            this.button_selectBatcodeFile.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_selectBatcodeFile.Location = new System.Drawing.Point(900, 96);
            this.button_selectBatcodeFile.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_selectBatcodeFile.Name = "button_selectBatcodeFile";
            this.button_selectBatcodeFile.Size = new System.Drawing.Size(176, 78);
            this.button_selectBatcodeFile.TabIndex = 43;
            this.button_selectBatcodeFile.Text = "选择条码号文件...";
            this.button_selectBatcodeFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_selectBatcodeFile.UseVisualStyleBackColor = true;
            this.button_selectBatcodeFile.Click += new System.EventHandler(this.button_selectBatcodeFile_Click);
            // 
            // textBox_patronBarcode
            // 
            this.textBox_patronBarcode.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_patronBarcode.Location = new System.Drawing.Point(190, 96);
            this.textBox_patronBarcode.Multiline = true;
            this.textBox_patronBarcode.Name = "textBox_patronBarcode";
            this.textBox_patronBarcode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_patronBarcode.Size = new System.Drawing.Size(700, 203);
            this.textBox_patronBarcode.TabIndex = 40;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(22, 730);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1054, 22);
            this.progressBar1.TabIndex = 47;
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(954, 592);
            this.button_stop.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(122, 58);
            this.button_stop.TabIndex = 49;
            this.button_stop.Text = "暂停";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_close
            // 
            this.button_close.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_close.Location = new System.Drawing.Point(954, 761);
            this.button_close.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(122, 58);
            this.button_close.TabIndex = 53;
            this.button_close.Text = "关闭";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click_1);
            // 
            // button_selectDir
            // 
            this.button_selectDir.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_selectDir.Location = new System.Drawing.Point(981, 25);
            this.button_selectDir.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_selectDir.Name = "button_selectDir";
            this.button_selectDir.Size = new System.Drawing.Size(95, 58);
            this.button_selectDir.TabIndex = 54;
            this.button_selectDir.Text = "...";
            this.button_selectDir.UseVisualStyleBackColor = true;
            this.button_selectDir.Click += new System.EventHandler(this.button_selectDir_Click);
            // 
            // label_info
            // 
            this.label_info.AutoSize = true;
            this.label_info.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_info.Location = new System.Drawing.Point(17, 761);
            this.label_info.Name = "label_info";
            this.label_info.Size = new System.Drawing.Size(68, 27);
            this.label_info.TabIndex = 55;
            this.label_info.Text = "info";
            // 
            // button_plan
            // 
            this.button_plan.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_plan.Location = new System.Drawing.Point(22, 570);
            this.button_plan.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button_plan.Name = "button_plan";
            this.button_plan.Size = new System.Drawing.Size(918, 89);
            this.button_plan.TabIndex = 57;
            this.button_plan.Text = "开始生成借阅报表";
            this.button_plan.UseVisualStyleBackColor = true;
            this.button_plan.Click += new System.EventHandler(this.button_plan_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(16, 398);
            this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(312, 33);
            this.label1.TabIndex = 58;
            this.label1.Text = "快速设置日期范围：";
            // 
            // createReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(216F, 216F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1101, 838);
            this.Controls.Add(this.button_plan);
            this.Controls.Add(this.label_info);
            this.Controls.Add(this.button_selectDir);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_outputDir);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.dateTimePicker_start);
            this.Controls.Add(this.dateTimePicker_end);
            this.Controls.Add(this.comboBox_quickSetFilenames);
            this.Controls.Add(this.button_selectBatcodeFile);
            this.Controls.Add(this.textBox_patronBarcode);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("宋体", 10.66667F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "createReport";
            this.Text = "创建借阅报表";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.createReport_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
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
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Button button_selectDir;
        private System.Windows.Forms.Label label_info;
        private System.Windows.Forms.Button button_plan;
        private System.Windows.Forms.Label label1;
    }
}
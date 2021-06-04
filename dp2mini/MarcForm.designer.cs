namespace dp2mini
{
    partial class MarcForm
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
            this.textBox_isofilename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_findFileName = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_encoding = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_marcSyntax = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_source = new System.Windows.Forms.ComboBox();
            this.button_tobdf = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_bookType = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_location = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_isofilename
            // 
            this.textBox_isofilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_isofilename.Location = new System.Drawing.Point(16, 40);
            this.textBox_isofilename.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_isofilename.Name = "textBox_isofilename";
            this.textBox_isofilename.Size = new System.Drawing.Size(696, 28);
            this.textBox_isofilename.TabIndex = 8;
            this.textBox_isofilename.TextChanged += new System.EventHandler(this.textBox_isofilename_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(269, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "MARC (ISO2709格式) 文件名(&F):";
            // 
            // button_findFileName
            // 
            this.button_findFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findFileName.Location = new System.Drawing.Point(720, 35);
            this.button_findFileName.Margin = new System.Windows.Forms.Padding(4);
            this.button_findFileName.Name = "button_findFileName";
            this.button_findFileName.Size = new System.Drawing.Size(50, 33);
            this.button_findFileName.TabIndex = 9;
            this.button_findFileName.Text = "...";
            this.button_findFileName.Click += new System.EventHandler(this.button_findFileName_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 118);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 18);
            this.label3.TabIndex = 12;
            this.label3.Text = "编码方式(&E):";
            // 
            // comboBox_encoding
            // 
            this.comboBox_encoding.Items.AddRange(new object[] {
            "GB2312",
            "UTF-8",
            "UTF-16"});
            this.comboBox_encoding.Location = new System.Drawing.Point(137, 115);
            this.comboBox_encoding.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_encoding.Name = "comboBox_encoding";
            this.comboBox_encoding.Size = new System.Drawing.Size(250, 26);
            this.comboBox_encoding.TabIndex = 13;
            this.comboBox_encoding.Text = "GB2312";
            this.comboBox_encoding.TextChanged += new System.EventHandler(this.comboBox_encoding_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 84);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 18);
            this.label4.TabIndex = 10;
            this.label4.Text = "MARC格式(&S):";
            // 
            // comboBox_marcSyntax
            // 
            this.comboBox_marcSyntax.Items.AddRange(new object[] {
            "UNIMARC"});
            this.comboBox_marcSyntax.Location = new System.Drawing.Point(137, 81);
            this.comboBox_marcSyntax.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_marcSyntax.Name = "comboBox_marcSyntax";
            this.comboBox_marcSyntax.Size = new System.Drawing.Size(250, 26);
            this.comboBox_marcSyntax.TabIndex = 11;
            this.comboBox_marcSyntax.Text = "UNIMARC";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 18);
            this.label1.TabIndex = 14;
            this.label1.Text = "来源系统:";
            // 
            // comboBox_source
            // 
            this.comboBox_source.Items.AddRange(new object[] {
            "DT1000",
            "金盘",
            "清大新洋"});
            this.comboBox_source.Location = new System.Drawing.Point(121, 34);
            this.comboBox_source.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_source.Name = "comboBox_source";
            this.comboBox_source.Size = new System.Drawing.Size(250, 26);
            this.comboBox_source.TabIndex = 15;
            this.comboBox_source.SelectedIndexChanged += new System.EventHandler(this.comboBox_source_SelectedIndexChanged);
            // 
            // button_tobdf
            // 
            this.button_tobdf.Location = new System.Drawing.Point(16, 415);
            this.button_tobdf.Name = "button_tobdf";
            this.button_tobdf.Size = new System.Drawing.Size(164, 39);
            this.button_tobdf.TabIndex = 16;
            this.button_tobdf.Text = "转换为BDF文件";
            this.button_tobdf.UseVisualStyleBackColor = true;
            this.button_tobdf.Click += new System.EventHandler(this.button_tobdf_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(451, 93);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(30, 30);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(363, 354);
            this.webBrowser1.TabIndex = 17;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_bookType);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_location);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBox_source);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(12, 158);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(384, 170);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            // 
            // textBox_bookType
            // 
            this.textBox_bookType.Location = new System.Drawing.Point(121, 121);
            this.textBox_bookType.Name = "textBox_bookType";
            this.textBox_bookType.Size = new System.Drawing.Size(250, 28);
            this.textBox_bookType.TabIndex = 21;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 124);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 18);
            this.label6.TabIndex = 20;
            this.label6.Text = "图书类型:";
            // 
            // textBox_location
            // 
            this.textBox_location.Location = new System.Drawing.Point(121, 75);
            this.textBox_location.Name = "textBox_location";
            this.textBox_location.Size = new System.Drawing.Size(250, 28);
            this.textBox_location.TabIndex = 19;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 78);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 18);
            this.label5.TabIndex = 15;
            this.label5.Text = "馆藏地:";
            // 
            // MarcForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 540);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.button_tobdf);
            this.Controls.Add(this.textBox_isofilename);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_findFileName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_encoding);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox_marcSyntax);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MarcForm";
            this.Text = "MARC转BDF";
            this.Load += new System.EventHandler(this.PrepForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox_isofilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_findFileName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_encoding;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_marcSyntax;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_source;
        private System.Windows.Forms.Button button_tobdf;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_bookType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_location;
        private System.Windows.Forms.Label label5;
    }
}
namespace dp2mini
{
    partial class toolForm
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
            this.button_listdb = new System.Windows.Forms.Button();
            this.textBox_info = new System.Windows.Forms.TextBox();
            this.button_circulationRight = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_entity = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button_right = new System.Windows.Forms.Button();
            this.button_patron = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_oneKey = new System.Windows.Forms.Button();
            this.button_clear = new System.Windows.Forms.Button();
            this.textBox_dir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_selectDir = new System.Windows.Forms.Button();
            this.button_verifyBarcode = new System.Windows.Forms.Button();
            this.button_checkPrice = new System.Windows.Forms.Button();
            this.button_checkAccessNo = new System.Windows.Forms.Button();
            this.button_checkBookType = new System.Windows.Forms.Button();
            this.button_checkLocation = new System.Windows.Forms.Button();
            this.button_loc = new System.Windows.Forms.Button();
            this.button_paijia = new System.Windows.Forms.Button();
            this.button_script = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_listdb
            // 
            this.button_listdb.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_listdb.Location = new System.Drawing.Point(22, 71);
            this.button_listdb.Margin = new System.Windows.Forms.Padding(7);
            this.button_listdb.Name = "button_listdb";
            this.button_listdb.Size = new System.Drawing.Size(128, 57);
            this.button_listdb.TabIndex = 2;
            this.button_listdb.Text = "书目库";
            this.button_listdb.UseVisualStyleBackColor = true;
            this.button_listdb.Click += new System.EventHandler(this.button_listdb_Click);
            // 
            // textBox_info
            // 
            this.textBox_info.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_info.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_info.Location = new System.Drawing.Point(3, 3);
            this.textBox_info.Multiline = true;
            this.textBox_info.Name = "textBox_info";
            this.textBox_info.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_info.Size = new System.Drawing.Size(970, 412);
            this.textBox_info.TabIndex = 5;
            // 
            // button_circulationRight
            // 
            this.button_circulationRight.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_circulationRight.Location = new System.Drawing.Point(234, 143);
            this.button_circulationRight.Margin = new System.Windows.Forms.Padding(7);
            this.button_circulationRight.Name = "button_circulationRight";
            this.button_circulationRight.Size = new System.Drawing.Size(179, 57);
            this.button_circulationRight.TabIndex = 6;
            this.button_circulationRight.Text = "流通权限";
            this.button_circulationRight.UseVisualStyleBackColor = true;
            this.button_circulationRight.Click += new System.EventHandler(this.button_circulationRight_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(3, 3);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(454, 412);
            this.webBrowser1.TabIndex = 7;
            // 
            // button_entity
            // 
            this.button_entity.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_entity.Location = new System.Drawing.Point(163, 73);
            this.button_entity.Margin = new System.Windows.Forms.Padding(7);
            this.button_entity.Name = "button_entity";
            this.button_entity.Size = new System.Drawing.Size(136, 52);
            this.button_entity.TabIndex = 8;
            this.button_entity.Text = "获取册";
            this.button_entity.UseVisualStyleBackColor = true;
            this.button_entity.Click += new System.EventHandler(this.button_entity_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(19, 244);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBox_info);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(1467, 432);
            this.splitContainer1.SplitterDistance = 989;
            this.splitContainer1.TabIndex = 9;
            // 
            // button_right
            // 
            this.button_right.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_right.Location = new System.Drawing.Point(620, 143);
            this.button_right.Margin = new System.Windows.Forms.Padding(7);
            this.button_right.Name = "button_right";
            this.button_right.Size = new System.Drawing.Size(193, 57);
            this.button_right.TabIndex = 11;
            this.button_right.Text = "检查权限";
            this.button_right.UseVisualStyleBackColor = true;
            this.button_right.Click += new System.EventHandler(this.button_right_Click);
            // 
            // button_patron
            // 
            this.button_patron.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_patron.Location = new System.Drawing.Point(427, 143);
            this.button_patron.Margin = new System.Windows.Forms.Padding(7);
            this.button_patron.Name = "button_patron";
            this.button_patron.Size = new System.Drawing.Size(184, 57);
            this.button_patron.TabIndex = 12;
            this.button_patron.Text = "统计读者";
            this.button_patron.UseVisualStyleBackColor = true;
            this.button_patron.Click += new System.EventHandler(this.button_patron_Click);
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(1109, 10);
            this.button_stop.Margin = new System.Windows.Forms.Padding(7);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(96, 51);
            this.button_stop.TabIndex = 13;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_oneKey
            // 
            this.button_oneKey.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_oneKey.Location = new System.Drawing.Point(920, 10);
            this.button_oneKey.Margin = new System.Windows.Forms.Padding(7);
            this.button_oneKey.Name = "button_oneKey";
            this.button_oneKey.Size = new System.Drawing.Size(175, 51);
            this.button_oneKey.TabIndex = 14;
            this.button_oneKey.Text = "一键巡检";
            this.button_oneKey.UseVisualStyleBackColor = true;
            this.button_oneKey.Click += new System.EventHandler(this.button_oneKey_Click);
            // 
            // button_clear
            // 
            this.button_clear.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_clear.Location = new System.Drawing.Point(1229, 7);
            this.button_clear.Margin = new System.Windows.Forms.Padding(7);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(135, 57);
            this.button_clear.TabIndex = 15;
            this.button_clear.Text = "清信息";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // textBox_dir
            // 
            this.textBox_dir.Location = new System.Drawing.Point(163, 10);
            this.textBox_dir.Multiline = true;
            this.textBox_dir.Name = "textBox_dir";
            this.textBox_dir.ReadOnly = true;
            this.textBox_dir.Size = new System.Drawing.Size(642, 51);
            this.textBox_dir.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 30);
            this.label1.TabIndex = 17;
            this.label1.Text = "输出目录";
            // 
            // button_selectDir
            // 
            this.button_selectDir.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_selectDir.Location = new System.Drawing.Point(815, 10);
            this.button_selectDir.Margin = new System.Windows.Forms.Padding(7);
            this.button_selectDir.Name = "button_selectDir";
            this.button_selectDir.Size = new System.Drawing.Size(91, 51);
            this.button_selectDir.TabIndex = 18;
            this.button_selectDir.Text = "...";
            this.button_selectDir.UseVisualStyleBackColor = true;
            this.button_selectDir.Click += new System.EventHandler(this.button_selectDir_Click);
            // 
            // button_verifyBarcode
            // 
            this.button_verifyBarcode.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_verifyBarcode.Location = new System.Drawing.Point(1311, 77);
            this.button_verifyBarcode.Margin = new System.Windows.Forms.Padding(7);
            this.button_verifyBarcode.Name = "button_verifyBarcode";
            this.button_verifyBarcode.Size = new System.Drawing.Size(202, 52);
            this.button_verifyBarcode.TabIndex = 19;
            this.button_verifyBarcode.Text = "校验册条码";
            this.button_verifyBarcode.UseVisualStyleBackColor = true;
            this.button_verifyBarcode.Click += new System.EventHandler(this.button_verifyBarcode_Click);
            // 
            // button_checkPrice
            // 
            this.button_checkPrice.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_checkPrice.Location = new System.Drawing.Point(22, 145);
            this.button_checkPrice.Margin = new System.Windows.Forms.Padding(7);
            this.button_checkPrice.Name = "button_checkPrice";
            this.button_checkPrice.Size = new System.Drawing.Size(198, 52);
            this.button_checkPrice.TabIndex = 20;
            this.button_checkPrice.Text = "检查册价格";
            this.button_checkPrice.UseVisualStyleBackColor = true;
            this.button_checkPrice.Click += new System.EventHandler(this.button_checkPrice_Click);
            // 
            // button_checkAccessNo
            // 
            this.button_checkAccessNo.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_checkAccessNo.Location = new System.Drawing.Point(920, 77);
            this.button_checkAccessNo.Margin = new System.Windows.Forms.Padding(7);
            this.button_checkAccessNo.Name = "button_checkAccessNo";
            this.button_checkAccessNo.Size = new System.Drawing.Size(198, 52);
            this.button_checkAccessNo.TabIndex = 21;
            this.button_checkAccessNo.Text = "检查索取号";
            this.button_checkAccessNo.UseVisualStyleBackColor = true;
            this.button_checkAccessNo.Click += new System.EventHandler(this.button_checkAccessNo_Click);
            // 
            // button_checkBookType
            // 
            this.button_checkBookType.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_checkBookType.Location = new System.Drawing.Point(519, 76);
            this.button_checkBookType.Margin = new System.Windows.Forms.Padding(7);
            this.button_checkBookType.Name = "button_checkBookType";
            this.button_checkBookType.Size = new System.Drawing.Size(204, 52);
            this.button_checkBookType.TabIndex = 22;
            this.button_checkBookType.Text = "统计书类型";
            this.button_checkBookType.UseVisualStyleBackColor = true;
            this.button_checkBookType.Click += new System.EventHandler(this.button_checkBookType_Click);
            // 
            // button_checkLocation
            // 
            this.button_checkLocation.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_checkLocation.Location = new System.Drawing.Point(313, 74);
            this.button_checkLocation.Margin = new System.Windows.Forms.Padding(7);
            this.button_checkLocation.Name = "button_checkLocation";
            this.button_checkLocation.Size = new System.Drawing.Size(204, 52);
            this.button_checkLocation.TabIndex = 23;
            this.button_checkLocation.Text = "统计馆藏地";
            this.button_checkLocation.UseVisualStyleBackColor = true;
            this.button_checkLocation.Click += new System.EventHandler(this.button_checkLocation_Click);
            // 
            // button_loc
            // 
            this.button_loc.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_loc.Location = new System.Drawing.Point(827, 143);
            this.button_loc.Margin = new System.Windows.Forms.Padding(7);
            this.button_loc.Name = "button_loc";
            this.button_loc.Size = new System.Drawing.Size(190, 57);
            this.button_loc.TabIndex = 24;
            this.button_loc.Text = "单馆藏地";
            this.button_loc.UseVisualStyleBackColor = true;
            this.button_loc.Click += new System.EventHandler(this.button_getLocation_Click);
            // 
            // button_paijia
            // 
            this.button_paijia.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_paijia.Location = new System.Drawing.Point(737, 75);
            this.button_paijia.Margin = new System.Windows.Forms.Padding(7);
            this.button_paijia.Name = "button_paijia";
            this.button_paijia.Size = new System.Drawing.Size(179, 57);
            this.button_paijia.TabIndex = 25;
            this.button_paijia.Text = "排架体系";
            this.button_paijia.UseVisualStyleBackColor = true;
            this.button_paijia.Click += new System.EventHandler(this.button_paijia_Click);
            // 
            // button_script
            // 
            this.button_script.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_script.Location = new System.Drawing.Point(1132, 72);
            this.button_script.Margin = new System.Windows.Forms.Padding(7);
            this.button_script.Name = "button_script";
            this.button_script.Size = new System.Drawing.Size(171, 57);
            this.button_script.TabIndex = 26;
            this.button_script.Text = "校验函数";
            this.button_script.UseVisualStyleBackColor = true;
            this.button_script.Click += new System.EventHandler(this.button_script_Click);
            // 
            // toolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1508, 688);
            this.Controls.Add(this.button_script);
            this.Controls.Add(this.button_paijia);
            this.Controls.Add(this.button_loc);
            this.Controls.Add(this.button_checkLocation);
            this.Controls.Add(this.button_checkBookType);
            this.Controls.Add(this.button_checkAccessNo);
            this.Controls.Add(this.button_checkPrice);
            this.Controls.Add(this.button_verifyBarcode);
            this.Controls.Add(this.button_selectDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_dir);
            this.Controls.Add(this.button_clear);
            this.Controls.Add(this.button_oneKey);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_patron);
            this.Controls.Add(this.button_right);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.button_entity);
            this.Controls.Add(this.button_circulationRight);
            this.Controls.Add(this.button_listdb);
            this.Margin = new System.Windows.Forms.Padding(7);
            this.Name = "toolForm";
            this.Text = "巡检工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.toolForm_FormClosing);
            this.Load += new System.EventHandler(this.toolForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_listdb;
        private System.Windows.Forms.TextBox textBox_info;
        private System.Windows.Forms.Button button_circulationRight;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button_entity;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button_right;
        private System.Windows.Forms.Button button_patron;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_oneKey;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.TextBox textBox_dir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_selectDir;
        private System.Windows.Forms.Button button_verifyBarcode;
        private System.Windows.Forms.Button button_checkPrice;
        private System.Windows.Forms.Button button_checkAccessNo;
        private System.Windows.Forms.Button button_checkBookType;
        private System.Windows.Forms.Button button_checkLocation;
        private System.Windows.Forms.Button button_loc;
        private System.Windows.Forms.Button button_paijia;
        private System.Windows.Forms.Button button_script;
    }
}
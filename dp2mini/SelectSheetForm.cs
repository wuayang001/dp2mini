using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.TabControl;

namespace dp2mini
{
    public partial class SelectSheetForm : Form
    {
        TransferStatisForm _mainFrom = null;

        public SelectSheetForm(TransferStatisForm mainForm)
        {
            this._mainFrom = mainForm;
            InitializeComponent();
        }


        public List<string> OutputTables = new List<string>();

        private void Form_SelectSheetForm_Load(object sender, EventArgs e)
        {
            TabPageCollection tables = this._mainFrom.tabControl_table.TabPages;
            if (tables == null || tables.Count == 0)
                return;

            int x = 10, y = 20;


            foreach (TabPage page in tables)
            {
                CheckBox cb = new CheckBox();
                cb.Text = page.Text;
                //cb.Tag = item.RecPath;
                cb.AutoSize = true;
                cb.Location = new Point(x, y);

                this.groupBox1.Controls.Add(cb);

                y += 30;
            }

            OutputTables.Clear();


        }

        private void button_ok_Click(object sender, EventArgs e)
        {

            foreach (Control cl in this.groupBox1.Controls)//循环整个form上的控件
            {
                if (cl is CheckBox)//看看是不是checkbox
                {
                    CheckBox cb = cl as CheckBox;//将找到的control转化成checkbox
                    if (cb.Checked)//判断是否选中
                    {
                        OutputTables.Add(cb.Text);
                    }
                }
            }

            if (OutputTables.Count == 0)
            {
                MessageBox.Show(this, "您尚未选择要导出的数据表，请先选择。如不导出，请按[取消]按钮。");
                return;
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
        }



        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


    }
}

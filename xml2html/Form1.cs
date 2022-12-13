
//test
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform;

namespace xml2html
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton_testconvert_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog()) 
            { 
            dlg.Title = "请指定读者 XML 文件名";
            dlg.FileName = "";
            dlg.Filter = "读者 XML 文件 （*.xml|*.xml|All files(*.*)|*.*";
            dlg.RestoreDirectory= true;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

                string target_filename = "C:\\Users\\佩奇\\Desktop\\xml change html\\test.html";
                ConvertHelper.Convert(dlg.FileName, target_filename);
                MessageBox.Show(this, $"转换完成。在文件 {target_filename} 中");
            }
        }
       
    }
}

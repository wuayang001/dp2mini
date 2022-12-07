using DigitalPlatform.ChargingAnalysis;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using xml2html;

namespace dp2mini
{
    public partial class createReport : Form
    {
        
        public createReport()
        {
            InitializeComponent();
        }

        public string OutputDir {
            get
            {
                return this.textBox_outputDir.Text;
            }
            set
            {this.textBox_outputDir.Text = value;
            }

        }


        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }




        private void createReport_Load(object sender, EventArgs e)
        {

        }

        public int GetInput(out string patronBarcodes,
            out string startDate,
            out string endDate,
            out string dir)
        {

            //时间范围
            startDate = this.dateTimePicker_start.Value.ToString("yyyy/MM/dd");
            endDate = this.dateTimePicker_end.Value.ToString("yyyy/MM/dd");

            patronBarcodes = "";
            // 输出目录
            dir = this.textBox_outputDir.Text.Trim();
            if (string.IsNullOrEmpty(dir) == true)
            {
                MessageBox.Show(this, "尚未设置报表输出目录。");
                return -1;
            }
            if (Directory.Exists(dir) == false)  // 如果目录不存在，则创建一个新目录
                Directory.CreateDirectory(dir);

            // 如果目录不是空目录，提醒。
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            if (dirInfo.GetFiles("*.xml").Length > 0
                || dirInfo.GetFiles("*.html").Length > 0)  // todo，后面里面可能会放一个cfg目录，里面是配置文件
            {
                MessageBox.Show(this, "报表目录[" + dir + "]里，存在已生成好的报表(xml/html文件)，创建报表时需要选择一个空目录。");
                return -1;
            }


            // 证条码号，可能多个
            patronBarcodes = this.textBox_patronBarcode.Text.Trim();
            if (string.IsNullOrEmpty(patronBarcodes) == true)
            {
                MessageBox.Show(this, "请先输入读者证条码号。");
                return -1;
            }

            // 拆分证条码号，每个号码一行
            patronBarcodes = patronBarcodes.Replace("\r\n", "\n");
            string[] patronBarcodeList = patronBarcodes.Split(new char[] { '\n' });
            if (patronBarcodeList.Length == 0)
            {
                MessageBox.Show(this, "请先输入读者证条码号2。");
                return -1;
            }

            return 0;
        }



        public int CreateXml(CancellationToken token,
            string patronBarcodes,
            string startDate,
            string endDate,
            string dir,
            out string error)
        {
            error = "";
            int nRet = 0;

            // 拆分证条码号，每个号码一行
            patronBarcodes = patronBarcodes.Replace("\r\n", "\n");
            string[] patronBarcodeList = patronBarcodes.Split(new char[] { '\n' });

            this.SetProcess(0,patronBarcodeList.Length);
            int index = 0;

            string errorlog = "";

            //string strError = "";
            // 循环每个证条码，生成报表
            foreach (string patronBarcode in patronBarcodeList)
            {
                index++;
                this.SetProcessValue(index);

                // 停止
                token.ThrowIfCancellationRequested();

                try
                {
                    BorrowAnalysisReport report = null;

                    // 创建数据
                    nRet = BorrowAnalysisService.Instance.Build(token,
                               patronBarcode,
                               startDate,
                               endDate,
                               out report,
                               out error);
                    if (nRet == -1)
                        goto ERROR1;

                    // 输出报表
                    string xml = "";
                    nRet = BorrowAnalysisService.Instance.OutputReport(report,
                        "xml",
                        out xml,
                        out error);
                    if (nRet == -1)
                        goto ERROR1;

                    string fileName = dir + "\\" + patronBarcode + ".xml";
                    // StreamWriter当文件不存在时，会自动创建一个新文件。
                    using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        // 写到打印文件
                        writer.Write(xml);
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    goto ERROR1;
                }

                continue;


            ERROR1:
                errorlog += patronBarcode + "\t" + error + "\r\n";
            }

            if (string.IsNullOrEmpty(errorlog) == false)
            {
                string fileName = dir + "\\error.txt";
                // StreamWriter当文件不存在时，会自动创建一个新文件。
                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    // 写到打印文件
                    writer.Write(errorlog);
                }
            }

            return 0;
        }

        // 创建xml
        private void button_createXml_Click(object sender, EventArgs e)
        {
            // 获取界面输入参数
            int nRet = this.GetInput(out string patronBarcodes,
                out string startDate,
                out string endDate,
                out string outputDir);
            if (nRet == -1)
                return;


            // 创建xml
            nRet = this.CreateXml(this._cancel.Token,
                patronBarcodes,
                startDate,
                endDate,
                outputDir,
                out string strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "出错：" + strError);
                return;
            }
           



            MessageBox.Show(this, "批量生成借阅报表完成。");
            return;

        }


        public void PaiMing(CancellationToken token, string dir)
        {

            List<paiMingItem> paiMingList = new List<paiMingItem>();

            string[] fiels = Directory.GetFiles(dir, "*.xml");

            this.SetProcess(0, fiels.Length);
            int index = 0;

            foreach (string file in fiels)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(file);
                }
                catch (Exception ex)
                {
                    //todo 怎么记录错误
                    continue;
                }
                XmlNode root = dom.DocumentElement;

                //patron/barcode取内容
                string barcode = DomUtil.GetElementInnerText(root, "patron/barcode");

                //borrowInfo 取 totalBorrowedCount 属性
                string totalBorrowedCount = DomUtil.GetAttr(root, "borrowInfo", "totalBorrowedCount");
                int totalCount = Convert.ToInt32(totalBorrowedCount);

                paiMingList.Add(new paiMingItem(barcode, totalCount, file, dom));
            }

            // 按总量倒序排
            List<paiMingItem> sortedList = paiMingList.OrderByDescending(x => x.totalBorrowedCount).ToList();


            // 写回xml
            for (int i = 0; i < sortedList.Count; i++)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                int paiming = i + 1;

                paiMingItem item = sortedList[i];
                //string fileName = dir + "\\" + item.PatronBarcode + ".xml";

                XmlNode borrowInfoNode = item.dom.DocumentElement.SelectSingleNode("borrowInfo");

                DomUtil.SetAttr(borrowInfoNode, "paiming", paiming.ToString());

                item.dom.Save(item.fileName);

                // 更改进度条
                index++;
                this.SetProcessValue(index);
            }

            return;
        }

        // 排名
        private void button_paiming_Click(object sender, EventArgs e)
        {
            string dir = this.textBox_outputDir.Text.Trim();
            if (string.IsNullOrEmpty(dir) == true)
            {
                MessageBox.Show(this, "尚未设置报表输出目录。");
                return;
            }

            this.PaiMing(this._cancel.Token, dir);


            // 在list显示最新的文件
            //this.ShowFiles();

            MessageBox.Show(this, "排名处理完成。");
        }


        public void Xml2Html(CancellationToken token, string dir)
        {
            string[] fiels = Directory.GetFiles(dir, "*.xml");

            this.SetProcess(0, fiels.Length);
            int index = 0;


            foreach (string xmlFile in fiels)
            {


                // 停止
                token.ThrowIfCancellationRequested();

                // 如果对应的html存在，则显示，到时点击第一行时，显示对应
                int nIndex = xmlFile.LastIndexOf('.');
                string left = xmlFile.Substring(0, nIndex);
                string htmlFile = left + ".html";
                try
                {
                    // 调接口将xml转为html
                    ConvertHelper.Convert(xmlFile, htmlFile);
                }
                catch (Exception ex)
                {

                    string error = ex.Message;

                    // todo这个错写在哪里？
                }

                index++;
                this.SetProcessValue(index);
            }
        }

        // xml2html
        private void button_xml2html_Click(object sender, EventArgs e)
        {
            string dir = this.textBox_outputDir.Text.Trim();
            if (string.IsNullOrEmpty(dir) == true)
            {
                MessageBox.Show(this, "尚未设置报表输出目录。");
                return;
            }

            this.Xml2Html(this._cancel.Token,dir);

            MessageBox.Show(this, "xml转html完成");
        }



        // 在证条码输入框回车时，自动发起创建阅读分析
        private void textBox_patronBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            //if条件检测按下的是不是Enter键
            if (e.KeyCode == Keys.Enter)
            {
                this.Create();
            }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }

        delegate void Delegate_QuickSetFilenames(Control control);


        #region 快速设置时间

        void QuickSetFilenames(Control control)
        {
            string strStartDate = "";
            string strEndDate = "";

            string strName = control.Text.Replace(" ", "").Trim();

            if (strName == "今天")
            {
                DateTime now = DateTime.Now;

                strStartDate = DateTimeUtil.DateTimeToString8(now);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                // strEndDate = DateTimeUtil.DateTimeToString8(start + new TimeSpan(7, 0,0,0));
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7 - 1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                MessageBox.Show(this, "无法识别的周期 '" + strName + "'");
                return;
            }

            this.dateTimePicker_start.Value = DateTimeUtil.Long8ToDateTime(strStartDate);
            this.dateTimePicker_end.Value = DateTimeUtil.Long8ToDateTime(strEndDate);
        }

        private void comboBox_quickSetFilenames_SelectedIndexChanged(object sender, EventArgs e)
        {
            Delegate_QuickSetFilenames d = new Delegate_QuickSetFilenames(QuickSetFilenames);
            this.BeginInvoke(d, new object[] { sender });
        }

        #endregion

        // 选择证条码号文件
        private void button_selectBatcodeFile_Click(object sender, EventArgs e)
        {
            string fileName = "";
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "请指定读者证条码号文件";
                dlg.FileName = "";
                dlg.Filter = "读者证条码号文件 （*.txt|*.txt|All files(*.*)|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;


                fileName = dlg.FileName;
            }


            // 取出条码号文件的内容，放在条码框里
            using (StreamReader reader = new StreamReader(fileName))//, Encoding.UTF8))
            {
                this.textBox_patronBarcode.Text = reader.ReadToEnd().Trim();
            }
        }

  
        // 显示用长
        static string GetTimeString(XmlDocument dom)
        {
            XmlElement time = dom.DocumentElement.SelectSingleNode("time") as XmlElement;
            if (time == null)
                return "";
            return time.GetAttribute("seconds") + " 秒; " + time.GetAttribute("start") + " - " + time.GetAttribute("end");
        }

        /// <summary>
        /// 清空界面数据
        /// </summary>
        public void ClearInfo()
        {
            /*
            // 清空listview
            this.listView_results.Items.Clear();

            // 清空借书列表
            this.listView_log_borrow.Items.Clear();
            this.listView_borrowStatis.Items.Clear();
            // 清空借书记录内存表
            _borrowItems.Clear();

            // 清空借书列表
            this.listView_return.Items.Clear();
            this.listView_returnStatis.Items.Clear();
            this._returnItems.Clear();

            // 借还统计信息
            this.listView_borrowAndReurn_statis.Items.Clear();
            this._borrowAndReturnItems.Clear();

            //设置父窗口状态栏参数
            this._mainForm.SetStatusMessage("");

            */
        }

        /// <summary>
        /// 设置控件是否可用
        /// </summary>
        /// <param name="bEnable"></param>
        void EnableControls(bool bEnable)
        {
            this.button_stop.Enabled = !(bEnable);

            this.button_onekey.Enabled = bEnable;
        }




        // 一键生成
        private void Create()
        {


           
        }

        // mid父窗口
        MainForm _mainForm = null;

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        //public BorrowAnalysisReport _report = null;

        public void SetProcess(int min, int max)
        {
            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                this.progressBar1.Minimum = min;
                this.progressBar1.Maximum = max;
            }
            ));
        }

        public void SetProcessValue(int value)
        {
            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                this.progressBar1.Value = value;
            }
            ));
        }

        /// <summary>
        /// 检索做事的函数
        /// </summary>
        /// <param name="token"></param>
        private void OneKey(CancellationToken token,
            string patronBarcode,
            string startDate,
            string endDate,
            string outputDir)
        {
            string strError = "";

            // 记下原来的光标
            Cursor oldCursor = Cursor.Current;

            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                // 设置按钮状态
                EnableControls(false);

                //清空界面数据
                this.ClearInfo();

                // 鼠标设为等待状态
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
            }
            ));

            try
            {

                // 创建xml
                int nRet = this.CreateXml(token,
                    patronBarcode, 
                    startDate,
                    endDate,
                    outputDir,
                     out strError);
                if (nRet == -1)
                    goto ERROR1;


                // 排名
                this.PaiMing(token, outputDir);

                // xml2html
                this.Xml2Html(token, outputDir);

                //
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show("处理完成。");
                }
));

                return;

            }
            finally
            {
                // 设置按置状态
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                    this.Cursor = oldCursor;

                }
                ));
            }

        ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(strError);
            }
            ));
        }

        private void 生成xml报表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_createXml_Click(null, null);
        }

        private void 报表xml转htmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_xml2html_Click(null,null);
        }

        private void 按借阅量排名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_paiming_Click(null,null);
        }

        private void button_onekey_Click(object sender, EventArgs e)
        {
            // 获取界面输入参数
            int nRet = this.GetInput(out string patronBarcodes,
                out string startDate,
                out string endDate,
                out string dir);
            if (nRet == -1)
                return;


            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                OneKey(this._cancel.Token,
                    patronBarcodes,
                    startDate,
                    endDate,
                    dir);
            });
        }

        private void button_close_Click_1(object sender, EventArgs e)
        {
            this.Close(); 
        }

        private void button_selectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();

            // todo记住上次选择的目录

            if (result != DialogResult.OK || string.IsNullOrEmpty(dlg.SelectedPath) == true)
            {
                //MessageBox.Show(this, "取消");
                return;
            }

            this.textBox_outputDir.Text = dlg.SelectedPath;
        }
    }
}

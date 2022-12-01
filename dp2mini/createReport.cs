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

        // 创建xml
        private void button_createXml_Click(object sender, EventArgs e)
        {
            //时间范围
            string startDate = this.dateTimePicker_start.Value.ToString("yyyy/MM/dd");
            string endDate = this.dateTimePicker_end.Value.ToString("yyyy/MM/dd");

            // 输出目录
            string dir = this.textBox_outputDir.Text.Trim();
            if (string.IsNullOrEmpty(dir) == true)
            {
                MessageBox.Show(this, "尚未设置报表输出目录。");
                return;
            }
            if (Directory.Exists(dir) == false)  // 如果目录不存在，则创建一个新目录
                Directory.CreateDirectory(dir);


            // 证条码号，可能多个
            string patronBarcodes = this.textBox_patronBarcode.Text.Trim();
            if (string.IsNullOrEmpty(patronBarcodes) == true)
            {
                MessageBox.Show(this, "请先输入读者证条码号。");
                return;
            }

           

            // 拆分证条码号，每个号码一行
            patronBarcodes = patronBarcodes.Replace("\r\n", "\n");
            string[] patronBarcodeList = patronBarcodes.Split(new char[] { '\n' });


            string strError = "";
            // 循环每个证条码，生成报表
            foreach (string patronBarcode in patronBarcodeList)
            {
                BorrowAnalysisReport report = null;

                // 创建数据
                int nRet = BorrowAnalysisService.Instance.Build(this._cancel.Token,
                            patronBarcode,
                            startDate,
                            endDate,
                            out report,
                            out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输出报表
                string xml = "";
                nRet = BorrowAnalysisService.Instance.OutputReport(report,
                    "xml",
                    out xml,
                    out strError);
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

            // 在list显示最新的文件
            //this.ShowFiles();

            MessageBox.Show(this, "批量生成借阅报表完成。");
            return;


        ERROR1:
            MessageBox.Show(this, "出错：" + strError);
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

            string temp = "";

            List<paiMingItem> paiMingList = new List<paiMingItem>();

            string[] fiels = Directory.GetFiles(dir, "*.xml");
            foreach (string file in fiels)
            {
                //temp+=file.Trim()+"\r\n";

                XmlDocument dom = new XmlDocument();
                dom.Load(file);
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
                int paiming = i + 1;

                paiMingItem item = sortedList[i];
                //string fileName = dir + "\\" + item.PatronBarcode + ".xml";

                XmlNode borrowInfoNode = item.dom.DocumentElement.SelectSingleNode("borrowInfo");

                DomUtil.SetAttr(borrowInfoNode, "paiming", paiming.ToString());

                item.dom.Save(item.fileName);

            }

            // 在list显示最新的文件
            //this.ShowFiles();

            MessageBox.Show(this, "排名处理完成。");
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

            string[] fiels = Directory.GetFiles(dir, "*.xml");
            foreach (string xmlFile in fiels)
            {
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
                    MessageBox.Show(this, error);
                }
            }

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
            //this.button_stop.Enabled = !(bEnable);
        }




        // 一键生成
        private void Create()
        {
            //时间范围
            string startDate = this.dateTimePicker_start.Value.ToString("yyyy/MM/dd");
            string endDate = this.dateTimePicker_end.Value.ToString("yyyy/MM/dd");

            // 读者证条码号
            string patronBarcode = this.textBox_patronBarcode.Text.Trim();
            if (string.IsNullOrEmpty(patronBarcode) == true)
            {
                MessageBox.Show(this, "请输入读者证条码号");
                return;
            }


            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                doSearch(this._cancel.Token,
                    patronBarcode,
                    startDate,
                    endDate);
            });
        }

        // mid父窗口
        MainForm _mainForm = null;

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        //public BorrowAnalysisReport _report = null;

        /// <summary>
        /// 检索做事的函数
        /// </summary>
        /// <param name="token"></param>
        private void doSearch(CancellationToken token,
            string patronBarcode,
            string startDate,
            string endDate)
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

            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                //
                //string times = startDate + "~" + endDate;

                //// 创建数据
                //int nRet = BorrowAnalysisService.Instance.Build(token,
                //    patronBarcode,
                //    startDate,
                //    endDate,
                //    out this._report,
                //    out strError);
                //if (nRet == -1)
                //    goto ERROR1;

                //// 输出报表
                //string xml = "";
                //nRet = BorrowAnalysisService.Instance.OutputReport(this._report,
                //    "xml",
                //    out xml,
                //    out strError);
                //if (nRet == -1)
                //    goto ERROR1;


                //// 把html显示在界面上
                //this.Invoke((Action)(() =>
                //{
                //    string temp = "<div>" + HttpUtility.HtmlEncode(xml) + "</div>";
                //    //SetHtmlString(this.webBrowser1, temp);

                //    // 把馆长评估提出来，显示在输入框中
                //    this.textBox_comment.Text = this._report.comment;

                //}
                //));

                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                // 设置按置状态
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                    this.Cursor = oldCursor;

                    this._mainForm.ReturnChannel(channel);
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
    }
}

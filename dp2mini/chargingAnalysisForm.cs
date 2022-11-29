using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Printing;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Forms;
using DigitalPlatform.LibraryRestClient;
using System.Collections.Generic;
using DigitalPlatform.IO;
using DigitalPlatform;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using DigitalPlatform.LibraryClient;
using ClosedXML.Excel;
using static System.Windows.Forms.ListView;
using DigitalPlatform.dp2.Statis;
using System.Linq;
using DigitalPlatform.ChargingAnalysis;
using DigitalPlatform.CirculationClient;
using System.Web;
using xml2html;

namespace dp2mini
{
    public partial class chargingAnalysisForm : Form
    {
        // mid父窗口
        MainForm _mainForm = null;

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();





        /// <summary>
        ///  构造函数
        /// </summary>
        public chargingAnalysisForm()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 窗体装载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrepForm_Load(object sender, EventArgs e)
        {
            this._mainForm = this.MdiParent as MainForm;

            // 阅读分析的数据目录
            string chargingAnalysisDataDir = ClientInfo.UserDir + "\\ChargingAnalysis\\";

            // 把登录相关参数传到ChargingAnalysisService服务类
            string serverUrl = this._mainForm.GetPurlUrl(this._mainForm.Setting.Url);
            string userName = this._mainForm.Setting.UserName;
            string password = this._mainForm.Setting.Password;
            string parameters = "type=worker,client=dp2mini|" + ClientInfo.ClientVersion;//Program.ClientVersion;
            string strError = "";
            int nRet = BorrowAnalysisService.Instance.Init(chargingAnalysisDataDir,
                serverUrl, userName, password, parameters,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }
        }

        /// <summary>
        /// 检索预约到书记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_search_Click(object sender, EventArgs e)
        {
            this.search();
        }

        private void search()
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


        // 给浏览器控件设置html
        public static void SetHtmlString(WebBrowser webBrowser,
            string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        // 设置文本
        static void SetTextString(WebBrowser webBrowser, string strText)
        {
            SetHtmlString(webBrowser, "<pre>" + HttpUtility.HtmlEncode(strText) + "</pre>");
        }

        /// <summary>
        /// 获得 RFC 1123 时间字符串的显示格式字符串
        /// </summary>
        /// <param name="strRfc1123TimeString">时间字符串。RFC1123 格式</param>
        /// <returns>显示格式</returns>
        public static string GetRfc1123DisplayString(string strRfc1123TimeString)
        {
            if (string.IsNullOrEmpty(strRfc1123TimeString) == true)
                return "";

            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123TimeString, "G");// + " (" + strRfc1123TimeString + ")";
            }
            catch (Exception ex)
            {
                return "解析 RFC1123 时间字符串 '" + strRfc1123TimeString + "' 时出错: " + ex.Message;
            }
        }

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
            //this.button_search.Enabled = bEnable;
            this.button_stop.Enabled = !(bEnable);

            //this.button_toExcel.Enabled = bEnable;
        }

        /// <summary>
        /// 停止检索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }



        delegate void Delegate_QuickSetFilenames(Control control);

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



        public void ToViewString(XmlDocument dom, string nodeName)
        {
            string partXml = DomUtil.GetElementInnerText(dom.DocumentElement, nodeName);
            if (string.IsNullOrEmpty(partXml) == false)
            {
                XmlDocument partDom = new XmlDocument();
                partDom.LoadXml(partXml);
                DomUtil.SetElementInnerXml(dom.DocumentElement, nodeName, partDom.OuterXml);
            }
        }




        // Borrow
        int GetBorrowInfo(XmlDocument dom,
            out BorrowLogItem borrowLog,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 返回的借书日志对象
            borrowLog = new BorrowLogItem();

            XmlNode root = dom.DocumentElement;

            borrowLog.readerBarcode = DomUtil.GetElementText(root, "readerBarcode");
            borrowLog.itemBarcode = DomUtil.GetElementText(root, "itemBarcode");

            string strBorrowDate = GetRfc1123DisplayString(DomUtil.GetElementText(root, "borrowDate"));
            string strBorrowPeriod = DomUtil.GetElementText(root, "borrowPeriod");

            XmlNode node = null;

            // 获取读者信息
            string strReaderXml = DomUtil.GetElementText(root, "readerRecord", out node);
            if (node != null)
            {
                XmlDocument reader_dom = new XmlDocument();
                try
                {
                    reader_dom.LoadXml(strReaderXml);
                }
                catch (Exception ex)
                {
                    strError = "读者记录XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                //string strBarcode = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "barcode");
                //string strState = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "state");
                borrowLog.readerType = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "readerType");
                //string strCardNumber = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "cardNumber");
                // string strComment = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "comment");
                //string strCreateDate = GetRfc1123DisplayString(DomUtil.GetElementInnerText(reader_dom.DocumentElement, "createDate"));
                //string strExpireDate = GetRfc1123DisplayString(DomUtil.GetElementInnerText(reader_dom.DocumentElement, "expireDate"));
                borrowLog.readerName = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "name");

                //><department>单位</department>
                borrowLog.dept = DomUtil.GetElementInnerText(reader_dom.DocumentElement, "department");

            }

            // 获取图书信息
            string strItemRecord = DomUtil.GetElementText(dom.DocumentElement, "itemRecord", out node);
            if (node != null)
            {

                XmlDocument item_dom = new XmlDocument();
                try
                {
                    item_dom.LoadXml(strItemRecord);
                }
                catch (Exception ex)
                {
                    strError = "读者记录XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                //string strBarcode = DomUtil.GetElementInnerText(item_dom.DocumentElement, "barcode");
                // string strState = DomUtil.GetElementInnerText(item_dom.DocumentElement, "state");
                borrowLog.bookType = DomUtil.GetElementInnerText(item_dom.DocumentElement, "bookType");
                // string strPublishTime = DomUtil.GetElementInnerText(item_dom.DocumentElement, "publishTime");
                borrowLog.location = DomUtil.GetElementInnerText(item_dom.DocumentElement, "location");
                //string strShelfNo = DomUtil.GetElementInnerText(item_dom.DocumentElement, "shelftNo");
            }



            return 0;
        }

        #region 关于点列头排序

        // 借书日志 点列头排序
        private void listView_log_borrow_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //int nClickColumn = e.Column;
            //SortCol(this.listView_log_borrow, SortColumns_borrow, nClickColumn);
        }

        // 借书统计 点列头排序
        private void listView_borrowStatis_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //int nClickColumn = e.Column;
            //SortCol(this.listView_borrowStatis, SortColumns_borrowStatis, nClickColumn);
        }

        // 还书日志 点列头排序
        private void listView_return_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //int nClickColumn = e.Column;
            //SortCol(this.listView_return, SortColumns_return, nClickColumn);
        }

        // 还书统计 点列头排序
        private void listView_returnStatis_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //    int nClickColumn = e.Column;
            //    SortCol(this.listView_returnStatis, SortColumns_borrowStatis, nClickColumn);
        }

        // 点击列头排序
        private void listView_results_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //int nClickColumn = e.Column;
            //SortCol(this.listView_results, SortColumns_all, nClickColumn);
        }

        private void listView_borrowAndReurn_statis_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            //SortCol(this.listView_borrowAndReurn_statis, SortColumns_borrowAndReturnStatis, nClickColumn);
        }

        SortColumns SortColumns_all = new SortColumns();
        SortColumns SortColumns_borrow = new SortColumns();
        SortColumns SortColumns_borrowStatis = new SortColumns();
        SortColumns SortColumns_return = new SortColumns();
        SortColumns SortColumns_returnStatis = new SortColumns();

        SortColumns SortColumns_borrowAndReturnStatis = new SortColumns();

        public static void SortCol(ListView myListView, SortColumns sortCol, int nClickColumn)
        {
            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            sortCol.SetFirstColumn(nClickColumn,
                sortStyle,
                myListView.Columns,
                true);

            // 排序
            myListView.ListViewItemSorter = new SortColumnsComparer(sortCol);

            myListView.ListViewItemSorter = null;
        }

        #endregion

        private void ToolStripMenuItem_huizong_Click(object sender, EventArgs e)
        {
            /*
            string totalCountText = "";
            ListView mylistview = null;
            if (this.tabControl_table.SelectedTab.Text == "借书统计")
            {
                mylistview = listView_borrowStatis;
                totalCountText = "借书";
            }
            else if (this.tabControl_table.SelectedTab.Text == "还书统计")
            {
                mylistview = this.listView_returnStatis;
                totalCountText = "还书";
            }

            if (mylistview == null)
                return;

            if (mylistview.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "请先选中要汇总的行。");
                return;
            }

            List<BorrowGroup> list = new List<BorrowGroup>();
            foreach (ListViewItem item in mylistview.SelectedItems)
            {
                BorrowGroup group = new BorrowGroup();
                group.location = item.SubItems[0].Text;
                group.readerBarcode = item.SubItems[1].Text;
                group.readerName = item.SubItems[2].Text;
                group.count = Convert.ToInt32(item.SubItems[4].Text);
                list.Add(group);
            }

            string text = "";

            var groups = list.GroupBy(p => p.location);
            foreach (var group in groups)
            {
                //Console.WriteLine(group.Key);

                string location = group.Key;

                int readerCount = 0;
                int totalCount = 0;
                foreach (var one in group)
                {
                    readerCount++;

                    totalCount += one.count;

                    //Console.WriteLine($"\t{person.Name},{person.Age}");
                }

                string line = location + "  读者数量:" + readerCount + "  "+totalCountText+"总量:" + totalCount;

                if (text != "")
                    text += "\r\n";
                text += line;
            }

            MessageBox.Show(this, text);
            */
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            if (this.tabControl_table.SelectedTab.Text == "借书统计")
            {
                foreach (ListViewItem item in this.listView_borrowStatis.Items)
                {
                    item.Selected = true;
                }
            }
            else
            {
                foreach (ListViewItem item in this.listView_returnStatis.Items)
                {
                    item.Selected = true;
                }

            }
            */

        }

        // 在证条码输入框回车时，自动发起创建阅读分析
        private void textBox_patronBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            //if条件检测按下的是不是Enter键
            if (e.KeyCode == Keys.Enter)
            {
                this.search();
            }
        }

        // 把html下载到本地
        private void button_download_Click(object sender, EventArgs e)
        {


            //// 输出报表
            //string xml = "";
            //string strError = "";
            //int nRet = BorrowAnalysisService.Instance.OutputReport(this._report,
            //    "xml",
            //    out xml,
            //    out strError);
            //if (nRet == -1)
            //{
            //    MessageBox.Show(this, strError);
            //    return;
            //}

            //// 把html保存到文件
            //// 询问文件名
            //SaveFileDialog dlg = new SaveFileDialog
            //{
            //    Title = "请指定文件名",
            //    CreatePrompt = false,
            //    OverwritePrompt = true,
            //    // dlg.FileName = this.ExportExcelFilename;
            //    // dlg.InitialDirectory = Environment.CurrentDirectory;
            //    Filter = "xml文档 (*.xml)|*.xml|All files (*.*)|*.*",


            //    RestoreDirectory = true
            //};

            //// 如果在询问文件名对话框，点了取消，退不处理，返回0，
            //if (dlg.ShowDialog() != DialogResult.OK)
            //    return;

            //string fileName = dlg.FileName;

            //// StreamWriter当文件不存在时，会自动创建一个新文件。
            //using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
            //{
            //    // 写到打印文件
            //    writer.Write(xml);
            //}

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //时间范围
            string startDate = this.dateTimePicker_start.Value.ToString("yyyy/MM/dd") + " 00:00";
            DateTime day1 = DateTimeUtil.ParseFreeTimeString(startDate);

            //DateTime qd=   day1.AddMonths(0 - ((day1.Month - 1) % 3));
            //  int q = qd.Month / 3 + 1;
            //  string text = qd.ToString("yyyy-MM")+";"+ qd.ToString("yyyy") + "第"+q+"季度";
            MessageBox.Show(this, DateTimeUtil.GetQuarter(day1));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 设置评语
            string comment = this.textBox_comment.Text.Trim();

            if (this.listView_files.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "请先从列表中选择要修改评语的读者记录。");
                return;
            }

            string file = this.listView_files.SelectedItems[0].Text;

            XmlDocument dom = new XmlDocument();
            dom.Load(file);
            XmlNode root = dom.DocumentElement;

            // 设到dom
             DomUtil.SetElementText(root, "comment",comment);

            // 保存到文件
            dom.Save(file);

            MessageBox.Show(this, "评语保存成功。");

            /*
            //ChargingAnalysisService.Instance._pa

            if (this._report == null
                || this._report.built == false)
            {
                MessageBox.Show(this, "请先创建报表。");
                return;
            }

            // 设置评语
            string comment = this.textBox_comment.Text.Trim();
            BorrowAnalysisService.Instance.SetComment(this._report, comment);
            //this._report.comment = comment;

            // 重新生成报表
            string html = "";
            string strError = "";
            int nRet = BorrowAnalysisService.Instance.OutputReport(this._report,
                "html",
                out html,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }
            //SetHtmlString(this.webBrowser1, html);
            */
        }

        private void button3_Click(object sender, EventArgs e)
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


               fileName= dlg.FileName;
            }


            // 取出条码号文件的内容，放在条码框里
            using (StreamReader reader = new StreamReader(fileName))//, Encoding.UTF8))
            {
                this.textBox_patronBarcode.Text = reader.ReadToEnd().Trim();
            }


        }

        private void button4_Click(object sender, EventArgs e)
        {
            //时间范围
            string startDate = this.dateTimePicker_start.Value.ToString("yyyy/MM/dd");
            string endDate = this.dateTimePicker_end.Value.ToString("yyyy/MM/dd");
            
            // 输出目录
            string dir =this.textBox_outputDir.Text.Trim();
            if (string.IsNullOrEmpty(dir) == true)
            {
                MessageBox.Show(this, "尚未设置报表输出目录。");
                return ;
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

            this.listView_files.Items.Clear(); 
            Application.DoEvents();

            // 拆分证条码号，每个号码一行
            patronBarcodes = patronBarcodes.Replace("\r\n","\n");
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

                string fileName =dir+"\\"+patronBarcode+".xml";

                // StreamWriter当文件不存在时，会自动创建一个新文件。
                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    // 写到打印文件
                    writer.Write(xml);
                }


            }

            // 在list显示最新的文件
            this.ShowFiles();

            MessageBox.Show(this, "批量生成借阅报表完成。");
            return;


        ERROR1:
            MessageBox.Show(this, "出错："+strError);
            return;
        }

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

           string[] fiels= Directory.GetFiles(dir, "*.xml");
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

                paiMingList.Add(new paiMingItem(barcode,totalCount,file,dom));
            }

            // 按总量倒序排
            List<paiMingItem> sortedList= paiMingList.OrderByDescending(x => x.totalBorrowedCount).ToList();


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
            this.ShowFiles();

            MessageBox.Show(this, "排名处理完成。");
        }

        class paiMingItem
        {
            public paiMingItem(string barcode, int count,string fileName, XmlDocument dom)
            {
                this.PatronBarcode = barcode;
                this.totalBorrowedCount=count;
                this.fileName = fileName;
                this.dom = dom;
            }

            public string PatronBarcode;
            public int totalBorrowedCount;

            public string fileName;
            public XmlDocument dom;
        }

        private void button_selectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            
            if (result != DialogResult.OK || string.IsNullOrEmpty(dlg.SelectedPath)==true)
            {
                //MessageBox.Show(this, "取消");
                return;
            }

            string dir = dlg.SelectedPath;
            //MessageBox.Show(this, dir);

            this.textBox_outputDir.Text = dir;

            this.ShowFiles();
        }


        private void ShowFiles()
        {
            try
            {
                string dir = this.textBox_outputDir.Text.Trim();
                if (string.IsNullOrEmpty(dir) == true)
                    return;

                this.listView_files.Items.Clear();

                string[] fiels = Directory.GetFiles(dir, "*.xml");
                foreach (string file in fiels)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.Load(file);
                    XmlNode root = dom.DocumentElement;

                    //patron/barcode取内容
                    string barcode = DomUtil.GetElementInnerText(root, "patron/barcode");

                    //borrowInfo 取 totalBorrowedCount 属性
                    string totalBorrowedCount = DomUtil.GetAttr(root, "borrowInfo", "totalBorrowedCount");
                    int totalCount = Convert.ToInt32(totalBorrowedCount);

                    string paiming = DomUtil.GetAttr(root, "borrowInfo", "paiming");

                    //comment 取 title 属性
                    string title = DomUtil.GetAttr(root, "comment", "title");

                    ListViewItem item = new ListViewItem(file);
                    item.SubItems.Add(barcode);
                    item.SubItems.Add(totalBorrowedCount);
                    item.SubItems.Add(paiming);
                    item.SubItems.Add(title);

                    // 如果对应的html存在，则显示，到时点击第一行时，显示对应
                    int nIndex = file.LastIndexOf('.');
                    string left = file.Substring(0, nIndex);
                    string htmlFile = left + ".html";
                    if (File.Exists(htmlFile) == true)
                    {
                        item.SubItems.Add(htmlFile);
                    }


                    this.listView_files.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }
        }

        private void listView_files_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_files.SelectedItems.Count > 0)
            {
                ListViewItem item = this.listView_files.SelectedItems[0];

                //MessageBox.Show(this, item.Text);

                string file = item.Text;

                XmlDocument dom = new XmlDocument();
                dom.Load(file);
                XmlNode root = dom.DocumentElement;

                //从馆员评语显示在输入框
                string comment = DomUtil.GetElementText(root, "comment");
                this.textBox_comment.Text = comment;


                // 如果存在html文件，显示出来
                if (item.SubItems.Count >= 6)
                {
                    string htmlFile = item.SubItems[5].Text;
                    this.showHtml(htmlFile);
                }
                else
                {
                    SetHtmlString(this.webBrowser1, "");

                }

            }
        }

        public void showHtml(string htmlFile)
        {
            string content = "";
            using (StreamReader reader = new StreamReader(htmlFile))//, Encoding.UTF8))
            {
                content = reader.ReadToEnd().Trim();
            }

            SetHtmlString(this.webBrowser1, content);

            //MessageBox.Show(this, htmlFile);

            ////webBrowser1.Navigate(@".\Documentation\index.html");
            ////this.webBrowser1.Url = new Uri(String.Format("file:///{0}/my_html.html", curDir));
            //this.webBrowser1.Navigate(htmlFile);
            ////this.webBrowser1.
            //this.webBrowser1.Document.Encoding = "utf-8";
        }

        private void button_stop_Click_1(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }

        private void contextMenuStrip_analysis_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ToolStripMenuItem_createhtml_Click(object sender, EventArgs e)
        {
            if (this.listView_files.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择记录行");
                return;
            }

            ListViewItem item = this.listView_files.SelectedItems[0];

            // 先得到xml文件
            string xmlFile = item.Text;

            // 如果对应的html存在，则显示，到时点击第一行时，显示对应
            int nIndex = xmlFile.LastIndexOf('.');
            string left = xmlFile.Substring(0, nIndex);
            string htmlFile = left + ".html";
            //if (File.Exists(htmlFile) == true)
            //{
            //    item.SubItems.Add(htmlFile);
            //}

            try
            {
                // 调接口将xml转为html
                ConvertHelper.Convert(xmlFile, htmlFile);
                // 显示出来 
                this.showHtml(htmlFile);

                // 加到命令行中
                item.SubItems.Add(htmlFile);

            }
            catch (Exception ex)
            {

                if (File.Exists(htmlFile) == true)
                { 
                    File.Delete(htmlFile);                
                }

                string error = ex.Message;
                SetHtmlString(this.webBrowser1,"<b>"+error+"</b>");
            }


        }
    }


}

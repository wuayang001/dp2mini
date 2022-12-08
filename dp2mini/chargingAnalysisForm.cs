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

            //把评语模板列出来
            if (BorrowAnalysisService.Instance.CommentTemplates != null
                && BorrowAnalysisService.Instance.CommentTemplates.Count > 0)
            {
                this.comboBox_comment.Text = C_SelectComment;
                this.comboBox_comment.Items.Add(C_SelectComment);
                foreach (string comment in BorrowAnalysisService.Instance.CommentTemplates)
                {
                    this.comboBox_comment.Items.Add(comment);
                }
            }

            // 把主窗口的状态条清一下。
            this._mainForm.SetStatusMessage("");
        }




        #region 报表目录

        // 报表目录
        public string ReportDir
        {
            get
            {
                return this.textBox_outputDir.Text.Trim();
            }
        }

        // 选择目录
        private void button_selectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();

            // todo记住上次选择的目录
            
            if (result != DialogResult.OK || string.IsNullOrEmpty(dlg.SelectedPath)==true)
            {
                //MessageBox.Show(this, "取消");
                return;
            }

            string dir = dlg.SelectedPath;
            this.textBox_outputDir.Text = dir;

            // 把评语输入框清空。
            this.textBox_comment.Text = "";
            this._startTime = DateTime.MinValue; //todo清掉最小时间

            // 重新显示文件
            this.ShowFiles();
        }

        // 手工输入目录，按回车
        private void textBox_outputDir_KeyDown(object sender, KeyEventArgs e)
        {
            //if条件检测按下的是不是Enter键
            if (e.KeyCode == Keys.Enter)
            {
                this.ShowFiles();
            }
        }

        // 显示目录中的文件
        private void ShowFiles()
        {
            try
            {
                string dir = this.textBox_outputDir.Text.Trim();
                if (string.IsNullOrEmpty(dir) == true)
                    return;

                this.listView_files.Items.Clear();

                if (Directory.Exists(dir) == false)
                    return;

                string[] fiels = Directory.GetFiles(dir, "*.xml");
                foreach (string xmlFile in fiels)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.Load(xmlFile);
                    XmlNode root = dom.DocumentElement;

                    //证条码号 patron/barcode 
                    string barcode = DomUtil.GetElementText(root, "patron/barcode");

                    //姓名 patron/name 
                    string name = DomUtil.GetElementText(root, "patron/name");

                    // 班级/部门  patron/department
                    string department = DomUtil.GetElementText(root, "patron/department");

                    //借阅量 borrowInfo/@totalBorrowedCount 属性
                    string totalBorrowedCount = DomUtil.GetAttr(root, "borrowInfo", "totalBorrowedCount");

                    // 排名 borrowInfo/@paiming 属性
                    string paiming = DomUtil.GetAttr(root, "borrowInfo", "paiming");

                    // 荣誉称号 borrowInfo/@ title 属性
                    string title = DomUtil.GetAttr(root, "borrowInfo", "title");

                    // 馆长评语
                    string comment = DomUtil.GetElementText(root, "comment");

                    string timeInfo = "";
                    XmlNode commentNode=root.SelectSingleNode("comment");
                    if (commentNode != null)
                    {
                        string startTime = DomUtil.GetAttr(commentNode, "startTime");
                        string endTime = DomUtil.GetAttr(commentNode, "endTime");
                        string usedSeconds = DomUtil.GetAttr(commentNode, "usedSeconds");

                        if (string.IsNullOrEmpty(usedSeconds) == false)
                            timeInfo = usedSeconds;//startTime + "/" + endTime + "/" + usedSeconds;
                    }

                    ListViewItem item = new ListViewItem(barcode);
                    item.SubItems.Add(name);
                    item.SubItems.Add(department);
                    item.SubItems.Add(totalBorrowedCount);
                    item.SubItems.Add(paiming);
                    item.SubItems.Add(title);
                    item.SubItems.Add(comment);
                    item.SubItems.Add(xmlFile);


                    // 如果对应的html存在，则显示，到时点击第一行时，显示对应
                    string htmlFile = dir + "\\" + barcode + ".html";
                    if (File.Exists(htmlFile) == true)
                    {
                        item.SubItems.Add(htmlFile);
                    }
                    else
                    {
                        item.SubItems.Add("");
                    }

                    // 加一个评语时间
                    item.SubItems.Add(timeInfo);  //9

                    // 加到列表集合中
                    this.listView_files.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }
        }

        #endregion

        #region 选行和显示html

        // 选择其中一行
        private void listView_files_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_files.SelectedItems.Count > 0)
            {
                ListViewItem item = this.listView_files.SelectedItems[0];

                //从馆员评语显示在输入框
                string comment = item.SubItems[6].Text;
                //this.textBox_comment.Text = comment;
                this.SetCommentForEdit(comment);


                // 如果存在html文件，显示出来
                if (item.SubItems.Count >= 9)
                {
                    string htmlFile = item.SubItems[8].Text;
                    if (string.IsNullOrEmpty(htmlFile) == false || File.Exists(htmlFile) == true)
                    {
                        this.showHtml(htmlFile);
                    }
                    else
                    {
                        SetHtmlString(this.webBrowser1, "<div>读者报表尚未转成html格式，请联系管理员。</div>");
                    }
                }
                else
                {
                    SetHtmlString(this.webBrowser1, "<div>读者报表尚未转成html格式，请联系管理员。</div>");
                }

            }
        }

        // 显示html文件
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

        // 给浏览器控件设置html
        public static void SetHtmlString(WebBrowser webBrowser,
            string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }
        #endregion


        #region 评语相关

        // 评语模板列表第一行
        public const string C_SelectComment = "请选择评语模板";

        // 提交评语
        private void button_setComment_Click(object sender, EventArgs e)
        {
            // 设置评语
            string comment = this.textBox_comment.Text.Trim();

            if (this.listView_files.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "请先从列表中选择要修改评语的读者记录。");
                return;
            }

            this.SaveComment(comment);


            MessageBox.Show(this, "评语保存成功。");
        }

        // 提交评语
        private void SaveComment(string comment1)
        {
            if (this.listView_files.SelectedItems.Count == 0)
                return;

            
            // 结束时间
            DateTime endTime = DateTime.Now;

            // 结束时间-开始时间=用时
            double usedSeconds = (endTime - this._startTime).TotalSeconds;


            // 支持多条一起编辑评语
            foreach (ListViewItem item in this.listView_files.SelectedItems)
            {
                // 因为里面会替换宏变量，所以先将comment设到一个新变量上。
                string thisComment = comment1;



                string barcode = item.Text;

                string xmlFile = this.ReportDir + "\\" + barcode + ".xml";
                string htmlFile = this.ReportDir + "\\" + barcode + ".html";

                XmlDocument dom = new XmlDocument();
                dom.Load(xmlFile);
                XmlNode root = dom.DocumentElement;


                // 评语中可以带着宏变量
                //<comment>{patronName}，您在校期间读了{borrowCount}书。读万卷书，行万里路。</comment>
                string patronName = item.SubItems[1].Text;
                string borrowCount=item.SubItems[3].Text;
                thisComment = thisComment.Replace("{patronName}", patronName);
                thisComment = thisComment.Replace("{borrowCount}", borrowCount);

                // 设到dom
                DomUtil.SetElementText(root, "comment", thisComment);



                string strStartTime = DateTimeUtil.DateTimeToString(this._startTime);
                string strEndTime = DateTimeUtil.DateTimeToString(endTime);
                string strUsedSeconds = usedSeconds.ToString("#0.00");
                string timeInfo = strUsedSeconds;//strStartTime + "/" + strEndTime + "/" + strUsedSeconds;


                XmlNode commentNode = root.SelectSingleNode("comment");
                DomUtil.SetAttr(commentNode, "startTime",strStartTime);
                DomUtil.SetAttr(commentNode, "endTime",strEndTime );
                DomUtil.SetAttr(commentNode, "usedSeconds",strUsedSeconds );



                // 保存到文件
                dom.Save(xmlFile);

                // 更新界面listview这一行里的评估
                item.SubItems[6].Text = thisComment;

                item.SubItems[9].Text = timeInfo;


                // 重新转出一个html
                try
                {
                    ConvertHelper.Convert(xmlFile, htmlFile);
                }
                catch (Exception ex)
                {
                    //todo 这种情况如何处理
                }

                // 保存到专门的评语文件
                BorrowAnalysisService.Instance.SetComment2file(barcode, thisComment);
            }

            // 设为最小值
            this._startTime = DateTime.MinValue;
        }


        // 选择评语
        private void comboBox_comment_SelectedIndexChanged(object sender, EventArgs e)
        {
            string comment = this.comboBox_comment.Text;
            if (comment == C_SelectComment)
                comment = "";

            // 设到编辑框
            this.SetCommentForEdit(comment);
            

            // 也同时保存一下评语,感觉直接保存，这样太快太自动了，还是让用户点一下提交评语为好。
            // this.SaveComment(comment);

        }
        
        // 评语开始时间
        DateTime _startTime = DateTime.MinValue;
        private void textBox_comment_Enter(object sender, EventArgs e)
        {
            //MessageBox.Show(this, "h");

            // 记下时间
            if (this._startTime == DateTime.MinValue)
            {
                this._startTime = DateTime.Now;
            }
        }

        public void SetCommentForEdit(string comment)
        {
            // 设到编辑框
            this.textBox_comment.Text = comment;

            // 设一下开始时间
            this._startTime = DateTime.Now;
        }

        #endregion

        #region 点击表头排序

        // 排序
        SortColumns SortColumns_report = new SortColumns();
        private void listView_files_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            chargingAnalysisForm.SortCol(this.listView_files, SortColumns_report, nClickColumn);
        }

        public static void SortCol(ListView myListView, SortColumns sortCol, int nClickColumn)
        {
            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第3列借阅量，第4列排名，这两名是数值排序
            if (nClickColumn == 3 || nClickColumn==4)
                sortStyle = ColumnSortStyle.RightAlign;

            sortCol.SetFirstColumn(nClickColumn,
                sortStyle,
                myListView.Columns,
                true);

            // 排序
            myListView.ListViewItemSorter = new SortColumnsComparer(sortCol);

            myListView.ListViewItemSorter = null;
        }


        #endregion

        #region 按钮事件

        // 点创建报表按钮
        private void button_createReport_Click(object sender, EventArgs e)
        {

            // 输出目录
            string dir = this.textBox_outputDir.Text.Trim();

            // 2022/12/7 没必要检查，因为进到里面，可以是做一些处理的事情，不一定是一键生成。
            //// 当设置的目录时，才进行目录是否合格，用户可能在此界面不选择，在创建报表界面才选择。
            //if (string.IsNullOrEmpty(dir) == false)
            //{

            //    // 如果目录不存在，则创建一个新目录
            //    if (Directory.Exists(dir) == false)
            //        Directory.CreateDirectory(dir);

            //    // 如果目录不是空目录，提醒。
            //    DirectoryInfo dirInfo = new DirectoryInfo(dir);
            //    if (dirInfo.GetFiles("*.xml").Length > 0
            //        || dirInfo.GetFiles("*.html").Length > 0)  // todo，后面里面可能会放一个cfg目录，里面是配置文件
            //    {
            //        MessageBox.Show(this, "报表目录[" + dir + "]里，存在已生成好的报表(xml/html文件)，创建报表时需要选择一个空目录。");
            //        return;
            //    }
            //}


            createReport dlg = new createReport();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.OutputDir = dir;
            //dlg.ShowDialog(this);
            DialogResult result = dlg.ShowDialog(this);


            // 需要重新显示报表，另外有可能目录都变了
            this.textBox_outputDir.Text = dlg.OutputDir;
            // 重新显示一下文件列表
            this.ShowFiles();
        }


        // 另存
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

           

        }

        #endregion

        private void ToolStripMenuItem_download_Click(object sender, EventArgs e)
        {

            // 先做到另存一个文件

            if (this.listView_files.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "请先选择另存的读者行");
                return;
            }


            if (this.listView_files.SelectedItems.Count >1)
            {
                MessageBox.Show(this, "您选择了多条读者，请选择一个读者另存。");
                return;
            }

            ListViewItem viewItem = this.listView_files.SelectedItems[0];

            // 用于拼文件名
            string barcode=viewItem.SubItems[0].Text;  
            string name=viewItem.SubItems[1].Text;
            string tempFileName = barcode + "_" + name + ".html";

            // 源文件
            string htmlFile= viewItem.SubItems[8].Text;

            //把html保存到文件
            //询问文件名
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "请指定读者报表文件名",
                CreatePrompt = false,
                OverwritePrompt = true,
                FileName = tempFileName,

                //InitialDirectory = Environment.CurrentDirectory,
                Filter = "html文档 (*.html)|*.html|All files (*.*)|*.*",

                RestoreDirectory = true
            };

            // 如果在询问文件名对话框，点了取消，退不处理，返回0，
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string targetFileName = dlg.FileName;

            string html = "";
            using (StreamReader reader = new StreamReader(htmlFile))//, Encoding.UTF8))
            {
                html = reader.ReadToEnd().Trim();
            }

            // StreamWriter当文件不存在时，会自动创建一个新文件。
            using (StreamWriter writer = new StreamWriter(targetFileName, false, Encoding.UTF8))
            {
                // 写到打印文件
                writer.Write(html);
            }

            // 打开文件
            Process.Start(targetFileName);
        }
    }


}

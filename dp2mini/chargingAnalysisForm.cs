﻿using System;
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

        private void button_createReport_Click(object sender, EventArgs e)
        {

        }
    }


}

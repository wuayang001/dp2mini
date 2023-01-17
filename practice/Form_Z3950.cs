using DigitalPlatform.Net;
using DigitalPlatform.Script;
using DigitalPlatform.Z3950;
using practice.Properties;
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

namespace practice
{
    public partial class Form_Z3950 : Form
    {
        #region 成员
        //Z39.50 前端类。维持通讯通道
        ZClient _zclient = new ZClient();

        //ISBN号分析器，todo isbn需要进行哪些处理？
        public IsbnSplitter _isbnSplitter = null;

        // 检索途径集合
        public UseCollection _useList = new UseCollection();

        #endregion

        public Form_Z3950()
        {
            InitializeComponent();
        }

        private void button_search_Click(object sender, EventArgs e)
        {

        }

        private void Form_Z3950_Load(object sender, EventArgs e)
        {
            // 把以前的记下来的信息装载到界面上。
            LoadSettings();

            // 准备环境，包括isbn的rangemessage和检索途径。
            // 注意这里用的了一个Result作为返回结果，这样
            Result result = LoadEnvironment();
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);
        }

        // 准备环境
        Result LoadEnvironment()
        {
            // 清空右侧html
            this.ClearHtml();

            // 装载rangemessage.xml文件，这个处理是定义isbn的一些规则。
            try
            {
                this._isbnSplitter = new IsbnSplitter(Path.Combine(Environment.CurrentDirectory,
                    "rangemessage.xml"));  // "\\isbn.xml"
            }
            catch (FileNotFoundException ex)
            {
                return new Result { Value = -1, ErrorInfo = "装载本地 isbn 规则文件 rangemessage.xml 发生错误 :" + ex.Message };
#if NO
                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("rangemessage.xml",    // "isbn.xml"
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
#endif
            }
            catch (Exception ex)
            {
                return new Result { Value = -1, ErrorInfo = "装载本地 isbn 规则文件发生错误 :" + ex.Message };
            }

            // 装载检索途径文件bib1use.xml
            Result result = _useList.Load(Path.Combine(Environment.CurrentDirectory, "bib1use.xml"));
            if (result.Value == -1)
                return result;

            // 得到检索途径数组
            string[] fromlist = this._useList.GetDropDownList();
            // 把检索途径装载到下拉列表中
            this.comboBox_use.Items.AddRange(fromlist);

            return new Result();
        }

        // 装载记载的上次界面信息
        void LoadSettings()
        {
            this.textBox_serverAddr.Text = Settings.Default.serverAddr;
            this.textBox_serverPort.Text = Settings.Default.serverPort;
            this.textBox_database.Text = Settings.Default.databaseNames;
            this.textBox_queryWord.Text = Settings.Default.queryWord;
            this.comboBox_use.Text = Settings.Default.queryUse;

            string strStyle = Settings.Default.authenStyle;
            if (strStyle == "idpass")
                this.radioButton_authenStyleIdpass.Checked = true;
            else
                this.radioButton_authenStyleOpen.Checked = true;

            this.textBox_groupID.Text = Settings.Default.groupID;
            this.textBox_userName.Text = Settings.Default.userName;
            this.textBox_password.Text = Settings.Default.password;

           // this.textBox_queryString.Text = Settings.Default.queryString;

            string strQueryStyle = Settings.Default.queryStyle;
            if (strQueryStyle == "easy")
                this.radioButton_query_easy.Checked = true;
            else
                this.radioButton_query_origin.Checked = true;

        }

        // 保存界面信息
        void SaveSettings()
        {
            Settings.Default.serverAddr = this.textBox_serverAddr.Text;
            Settings.Default.serverPort = this.textBox_serverPort.Text;
            Settings.Default.databaseNames = this.textBox_database.Text;
            Settings.Default.queryWord = this.textBox_queryWord.Text;
            Settings.Default.queryUse = this.comboBox_use.Text;

            if (this.radioButton_authenStyleIdpass.Checked == true)
                Settings.Default.authenStyle = "idpass";
            else
                Settings.Default.authenStyle = "open";

            Settings.Default.groupID = this.textBox_groupID.Text;
            Settings.Default.userName = this.textBox_userName.Text;
            Settings.Default.password = this.textBox_password.Text;

            Settings.Default.queryString = this.textBox_queryString.Text;

            if (this.radioButton_query_easy.Checked == true)
                Settings.Default.queryStyle = "easy";
            else
                Settings.Default.queryStyle = "origin";

            Settings.Default.Save();
        }




        #region 浏览器控件

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(Environment.CurrentDirectory, "operloghtml.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        /// <summary>
        /// 向一个浏览器控件中追加写入 HTML 字符串
        /// 不支持异步调用
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                Navigate(webBrowser, "about:blank");

                doc = webBrowser.Document;
            }

            doc.Write(strHtml);
        }

        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents();
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        #endregion
    }





}

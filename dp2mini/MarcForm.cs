using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Printing;

//using DigitalPlatform.Xml;
//using DigitalPlatform.Marc;
//using DigitalPlatform.Forms;
using DigitalPlatform.LibraryRestClient;
using System.Collections.Generic;
using DigitalPlatform.IO;
using DigitalPlatform;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using System.Text.RegularExpressions;
using DigitalPlatform.Text;

namespace dp2mini
{
    public partial class MarcForm : Form
    {
        // mid父窗口
        MainForm _mainForm = null;


        /// <summary>
        ///  构造函数
        /// </summary>
        public MarcForm()
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
        }


        private void button_findFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要导入的 MARC(ISO2709格式) 文件名";
            dlg.FileName = this.textBox_isofilename.Text;

            dlg.Filter = "ISO2709 文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_isofilename.Text = dlg.FileName;
        }

        /// <summary>
        /// 获取或设置 MARC 格式。为 "unimarc" "usmarc" 之一
        /// </summary>
        public string MarcSyntax
        {
            get
            {
                return this.comboBox_marcSyntax.Text.ToLower();
            }
            set
            {
                this.comboBox_marcSyntax.Text = value.ToUpper();
            }
        }

        /// <summary>
        /// 获得编码方式
        /// 可能会抛出异常
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                // 2014/3/10
                if (string.IsNullOrEmpty(this.comboBox_encoding.Text) == true)
                    return null;

                if (StringUtil.IsNumber(this.comboBox_encoding.Text) == true)
                    return Encoding.GetEncoding(Convert.ToInt32(this.comboBox_encoding.Text));


                    return Encoding.GetEncoding(this.comboBox_encoding.Text);
            }
        }

        #region 显示第一条记录

        private void textBox_filename_TextChanged(object sender, EventArgs e)
        {
            // 自动显示文件的第一条
            DisplayFirstRecord(this.FileName, this.Encoding);
        }


        /// <summary>
        /// 显示第一条记录
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="encoding"></param>
        void DisplayFirstRecord(string strFileName,
            Encoding encoding)
        {
            if (string.IsNullOrEmpty(strFileName) == true)
                goto ERROR1;
            if (File.Exists(strFileName) == false)
                goto ERROR1;
            if (encoding == null)
                goto ERROR1;

            string strMARC = "";
            string strError = "";
            // return:
            //      -1  出错
            //      0   正常
            int nRet = LoadFirstRecord(strFileName,
            encoding,
            out strMARC,
            out strError);
            if (nRet == -1)
                goto ERROR1;



            string strHead = @"<head>
<style type='text/css'>
BODY {
	FONT-FAMILY: Microsoft YaHei, Verdana, 宋体;
	FONT-SIZE: 8pt;
}
TABLE.marc
{
    font-size: 8pt;
    width: auto;
}
TABLE.marc TD
{
   vertical-align:text-top;
}
TABLE.marc TR.header
{
    background-color: #eeeeee;
}
TABLE.marc TR.datafield
{
}
TABLE.marc TD.fieldname
{
    border: 0px;
    border-top: 1px;
    border-style: dotted;
    border-color: #cccccc;
}
TABLE.marc TD.fieldname, TABLE.marc TD.indicator, TABLE.marc TR.header TD.content, TABLE.marc SPAN
{
     font-family: Courier New, Tahoma, Arial, Helvetica, sans-serif;
     font-weight: bold;
}
TABLE.marc TD.indicator
{
    padding-left: 4px;
    padding-right: 4px;
    
    border: 0px;
    border-left: 1px;
    border-right: 1px;
    border-style: dotted;
    border-color: #eeeeee;
}
TABLE.marc SPAN.subfield
{
    margin: 2px;
    margin-left: 0px;
    line-height: 140%;
        
    border: 1px;
    border-style: solid;
    border-color: #cccccc;
    
    padding-top: 1px;
    padding-bottom: 1px;
    padding-left: 3px;
    padding-right: 3px;
    font-weight: bold;
    color: Blue;
    background-color: Yellow;
}
TABLE.marc SPAN.fieldend
{
    margin: 2px;
    margin-left: 4px;
    
    border: 1px;
    border-style: solid;
    border-color: #cccccc;
    
    padding-top: 1px;
    padding-bottom: 1px;
    padding-left: 3px;
    padding-right: 3px;
    font-weight: bold;
    color: White;
    background-color: #cccccc;
}
</style>
</head>";

            string strHtml = "<html>" +
    strHead +
    "<body>" +
    MarcUtil.GetHtmlOfMarc(strMARC, false) +
    "</body></html>";

            AppendHtml(this.webBrowser1, strHtml, true);

            //if (this.IsOutput == false && this.Visible == true)
            //{
            //    HideMessageTip();
            //    ShowMessageTip();
            //}
            return;
        ERROR1:
            ClearHtml();
        }


        // 清空记录
        void ClearHtml()
        {

            this.webBrowser1.DocumentText = "<html><body></body></html>";
        }

        /*public*/
        static void AppendHtml(WebBrowser webBrowser,
 string strHtml,
 bool bClear = false)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            if (bClear == true)
                doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }


        // 从 ISO2709 文件中读出第一条记录。返回的是机内格式
        // return:
        //      -1  出错
        //      0   正常
        /*public*/
        static int LoadFirstRecord(string strMarcFileName,
 Encoding encoding,
 out string strMARC,
 out string strError)
        {
            strError = "";
            strMARC = "";

            try
            {
                using (Stream stream = File.Open(strMarcFileName,
    FileMode.Open,
    FileAccess.Read,
    FileShare.ReadWrite))
                {
                    // 从ISO2709文件中读入一条MARC记录
                    // return:
                    //	-2	MARC格式错
                    //	-1	出错
                    //	0	正确
                    //	1	结束(当前返回的记录有效)
                    //	2	结束(当前返回的记录无效)
                    int nRet = MarcUtil.ReadMarcRecord(stream,
                        encoding,
                        true,	// bRemoveEndCrLf,
                        true,	// bForce,
                        out strMARC,
                        out strError);
                    if (nRet == 0 || nRet == 1)
                        return 0;   // 正常

                    strError = "读入MARC记录时出错: " + strError;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "异常: " + ex.Message;
                return -1;
            }
        }

        #endregion

        // 用户输入的馆藏地
        public string Location
        {
            get
            {
                return this.textBox_location.Text.Trim();
            }
        }

        // 用户输入的馆藏地
        public string BookType
        {
            get
            {
                return this.textBox_bookType.Text.Trim();
            }
        }

        // 开始转bdf
        private void button_tobdf_Click(object sender, EventArgs e)
        {
            if (textBox_isofilename.Text.Trim() == "")
            {
                MessageBox.Show(this, "尚未选择iso数据文件");
                return;
            }

            if (comboBox_source.Text.Trim() == "")
            {
                MessageBox.Show(this, "尚未选择数据来源系统");
                return;
            }

            if (this.comboBox_source.Text.ToUpper() == "DT1000")
            {
                if (this.Location=="")
                {
                    MessageBox.Show(this, "请输入馆藏地");
                    return;
                }

                if (this.BookType == "")
                {
                    MessageBox.Show(this, "请输入图书类型");
                    return;
                }
            }



            // 选择保存的bdf文件
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Title = "书目转储文件名",
                Filter = "书目转储文件(*.bdf)|*.bdf",
                RestoreDirectory = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // bdf文件是一个xml结构
            string bdfFile = dlg.FileName;

            XmlTextWriter _writer = null;
            _writer = new XmlTextWriter(bdfFile, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 4
            };
            _writer.WriteStartDocument();
            _writer.WriteStartElement("dprms", "collection", DpNs.dprms);
            _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

            // 输出一个错误信息文件
            FileInfo fileInfo = new FileInfo(bdfFile);
            string strSourceDir = fileInfo.DirectoryName;
            string errorFilename = strSourceDir + "\\~error.txt";

            long errorCount1 = 0;
            StreamWriter sw_error = new StreamWriter(errorFilename,
                        false,  // append
                        Encoding.UTF8);

            // 源iso文件
            string isoFileName = this.textBox_isofilename.Text.Trim();

            // 用当前日期作为批次号
            string strBatchNo = DateTime.Now.ToString("yyyyMMdd");
            int nIndex = 0;
            string strError = "";

            MarcLoader loader = new MarcLoader(isoFileName, this.Encoding, "marc", null);
            foreach (string marc in loader)
            {
                MarcRecord record = new MarcRecord(marc);

                string strISBN = record.select("field[@name='010']/subfield[@name='a']").FirstContent;
                strISBN = string.IsNullOrEmpty(strISBN) ? "" : strISBN;

                string strTitle = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                strTitle = string.IsNullOrEmpty(strTitle) ? "" : strTitle;

                string strSummary = strISBN + "\t" + strTitle;


                // 转换回XML
                XmlDocument domMarc = null;
                int nRet = MarcUtil.Marc2Xml(marc,
                    this.MarcSyntax,//this.m_strBiblioSyntax,
                    out domMarc,
                    out strError);
                if (nRet == -1)
                {
                    errorCount1++;
                    sw_error.WriteLine("!!!异常Marc2Xml()" + strError);
                    return;
                }

                // 写<record>
                _writer.WriteStartElement("dprms", "record", DpNs.dprms);

                // 写<biblio>
                _writer.WriteStartElement("dprms", "biblio", DpNs.dprms);

                // 把marc xml写入<biblio>下级
                domMarc.WriteTo(_writer);
                // </<biblio>>
                _writer.WriteEndElement();

                #region 关于价格

                bool bPriceError = false;

                // *** 从010$d中取得价格
                string strErrorPrice = "";
                string strCataPrice = record.select("field[@name='010']/subfield[@name='d']").FirstContent;
                string strOldPrice = strCataPrice;

                // marc中不存在010$d价格
                if (string.IsNullOrEmpty(strCataPrice)==true)
                {
                    strCataPrice = "CNY0";
                    sw_error.WriteLine(nIndex + "\t" + strSummary + "\t不存在010$d价格子字段。");

                    bPriceError = true;
                }
                else
                {
                    // 如果不在合法的价格格式
                    if (!Regex.IsMatch(strCataPrice, @"^(CNY)?\d+\.?\d{0,2}$"))
                    {
                        //将价格中含有的汉字型数值替换为数字
                        strCataPrice = ParsePrice(strCataPrice);
                        if (strCataPrice != strOldPrice)
                        {
                            strErrorPrice = strOldPrice;
                            sw_error.WriteLine(nIndex + "\t" + strSummary + "\t价格字符串 '" + strOldPrice + "' 被自动修改为 '" + strCataPrice + "'");
                            bPriceError = true;

                            
                        }

                        //已知格式价格内容转换
                        string temp1 = strCataPrice;
                        strCataPrice = CorrectPrice(strCataPrice);
                        if (strCataPrice != temp1)
                        {
                            strErrorPrice = strOldPrice;
                            sw_error.WriteLine(nIndex + "\t" + strSummary + "\t价格字符串 '" + strOldPrice + "' 被自动修改为 '" + strCataPrice + "'");
                            bPriceError = true;
                        }
                    }

                    List<string> temp = VerifyPrice(strCataPrice);
                    if (temp.Count > 0)
                    {
                        string temp1 = strCataPrice;
                        CorrectPrice(ref strCataPrice);
                        if (temp1 != strCataPrice)
                        {
                            if (IsPriceCorrect(strCataPrice) == true)
                            {
                                sw_error.WriteLine(nIndex + "\t" + strSummary + "\t价格字符串 '" + strOldPrice + "' 被自动修改为 '" + strCataPrice + "'");
                                bPriceError = true;
                            }
                            else
                            {
                                strCataPrice = "CNY0";
                                strErrorPrice = strOldPrice;
                                sw_error.WriteLine(nIndex + "\t" + strSummary + "\t价格字符串 '" + strOldPrice + "' 无法被自动修改，默认为 CNY0");
                                bPriceError = true;
                            }
                        }
                        else
                        {
                            strCataPrice = "CNY0";
                            strErrorPrice = strOldPrice;
                            sw_error.WriteLine(nIndex + "\t" + strSummary + "\t价格字符串 '" + strOldPrice + "' 无法被自动修改，默认为 CNY0");
                            bPriceError = true;
                        }
                    }

                    // 如果不是CNY开头的，加CNY
                    if (!strCataPrice.StartsWith("CNY"))
                    {
                        strCataPrice = "CNY" + strCataPrice;
                        sw_error.WriteLine(nIndex + ":" + strSummary + "*** 价格字符串 '" + strCataPrice + "' 不含有币种前缀，自动添加为'CNY'\r\n");
                        bPriceError = true;
                    }
                }

                // 如果价格经过修复，错误记录数增加1
                if (bPriceError == true)
                {
                    errorCount1++;
                }

                #endregion

                #region 从905$d$e取得索书号

                // 905$d 中图法大类
                string str905d = record.select("field[@name='905']/subfield[@name='d']").FirstContent;
                if (String.IsNullOrEmpty(str905d) == true)
                {
                    strError = nIndex + "\t" + strSummary + "\t905字段不存在$d子字段";
                    sw_error.WriteLine(strError);
                }

                // 905$e 种次号
                string str905e = record.select("field[@name='905']/subfield[@name='e']").FirstContent;
                if (String.IsNullOrEmpty(str905e) == true)
                {
                    strError = nIndex + "\t" + strSummary + "\t905字段不存在$e子字段";
                    sw_error.WriteLine(strError);
                }

                // 如果缺索取号记一笔
                if (bPriceError == false)
                {
                    if (string.IsNullOrEmpty(str905d)==true || string.IsNullOrEmpty(str905e)==true)
                        errorCount1++;
                }

                #endregion


                // 册条码是放在906$h字段的
                MarcNodeList subfield_690h = record.select("field[@name='906']/subfield[@name='h']");


                int index = 0;
                // <itemCollection>
                _writer.WriteStartElement("dprms", "itemCollection", DpNs.dprms);
                foreach (MarcNode node in subfield_690h)
                {
                    index++;
                    string strBarcode = node.Content;
                    // 册条码号为空，则不创建册信息元素
                    if (string.IsNullOrEmpty(strBarcode))
                    {
                        continue;
                    }

                    _writer.WriteStartElement("dprms", "item", DpNs.dprms);


                    strBarcode = strBarcode.Trim();
                    _writer.WriteElementString("barcode", strBarcode);


                    string strLocation = this.Location;  //DT1000使用界面输入的值
                        _writer.WriteElementString("location", strLocation);


                    if (!string.IsNullOrEmpty(strCataPrice))
                        _writer.WriteElementString("price", strCataPrice);

                    if (!string.IsNullOrEmpty(strErrorPrice))
                        _writer.WriteElementString("comment", "原价格：" + strErrorPrice);

                    string strBookType = this.BookType;//DT1000使用界面输入的值
                    _writer.WriteElementString("bookType", strBookType);

                    if (!string.IsNullOrEmpty(str905d) && !string.IsNullOrEmpty(str905e))
                        _writer.WriteElementString("accessNo", str905d + "/" + str905e);
                    else
                    {
                        //strError = nIndex + "\t" + strSummary + "\t册记录'" + strBarcode + "'不含有 索取号 内容";
                        //sw_error.WriteLine(strError);
                    }

                    _writer.WriteElementString("batchNo", strBatchNo);//

                    _writer.WriteEndElement();
                }
                //</itemCollection>
                _writer.WriteEndElement();

                //</record>
                _writer.WriteEndElement();



                nIndex++;
                this._mainForm.SetStatusMessage(nIndex.ToString() +" "+strSummary);

                Application.DoEvents();
            }



            //</collection>
            _writer.WriteEndElement();
            _writer.WriteEndDocument();
            _writer.Close();
            _writer = null;


            this._mainForm.SetStatusMessage("处理完成，共处理"+nIndex.ToString() + "条。");

            string strMsg = "数据转换处理结束。请到dp2内务使用【批处理】-【从书目转储文件导入】功能将转换的 书目转储文件 导入到目标书目库。";

            MessageBox.Show(this, strMsg);

            if (sw_error != null)
            {
                 strMsg = "共处理'" + nIndex + "'条MARC记录，其中有 " +errorCount1.ToString() + " 条记录中有错误信息。";

                sw_error.WriteLine(strMsg);
                sw_error.Close();
               sw_error = null;
            }
        }


        public void setProgress(long totalLength, long current)
        {
            this._mainForm.SetStatusMessage(current.ToString() + "/" + totalLength.ToString());
        }


        #region 价格处理


        static string VerifyPricePrefix(string prefix)
        {
            foreach (var ch in prefix)
            {
                if (char.IsLetter(ch) == false)
                    return $"货币名称 '{prefix}' 中出现了非字母的字符";
            }

            return null;
        }

        // 去掉价格字符串中的 "(...)" 部分
        // TODO: 检查小数点后的位数，多于 2 位的要删除
        // return:
        //      false   没有发生修改
        //      true    发生了修改
        public static bool CorrectPrice(ref string strText)
        {
            strText = strText.Trim();

            //2017/6/17
            strText = strText.Replace("￥", "CNY");

            // 2017/6/17
            strText = strText.Replace("精装", "").Replace("平装", "").Replace("每册", "");

            strText = StringUtil.ToDBC(strText);

            // 截掉逗号右侧的部分
            List<string> parts = StringUtil.ParseTwoPart(strText, ",");
            strText = parts[0];

            int nStart = strText.IndexOf("(");
            if (nStart == -1)
                return false;

            // 右边剩余部分
            string strRight = strText.Substring(nStart + 1);

            strText = strText.Substring(0, nStart).Trim();
            int nEnd = strRight.IndexOf(")");
            if (nEnd == -1)
                return true;

            string strFragment = strRight.Substring(0, nEnd).Trim();
            strText += strRight.Substring(nEnd + 1).Trim();

            // 判断是否为 全5册 情况
            if (string.IsNullOrEmpty(strFragment) == false)
            {
                bool bChanged = false;

                if (strFragment == "上下册")
                {
                    strText += "/2";
                    bChanged = true;
                }
                else if (strFragment == "上中下册")
                {
                    strText += "/3";
                    bChanged = true;
                }
                else if (strFragment.EndsWith("册"))
                {
                    // 数字+册
                    string strNumber = strFragment.Substring(0, strFragment.Length - 1).Trim();
                    int v = 0;
                    if (StringUtil.IsPureNumber(strNumber) && Int32.TryParse(strNumber, out v))
                    {
                        strText += "/" + strNumber;
                        bChanged = true;
                    }
                }

                if (bChanged == false)
                {
                    string strNumber = StringUtil.Unquote(strFragment, "全册共册全卷共卷");
                    if (strNumber != strFragment)
                    {
                        int v = 0;
                        if (StringUtil.IsPureNumber(strNumber) && Int32.TryParse(strNumber, out v))
                        {
                            strText += "/" + strNumber;
                            // strError = "被变换为每册平均价格形态";
                        }
                    }
                }
            }

            // 2017/7/1
            if (strText.EndsWith(".00.00"))
                strText = strText.Substring(0, strText.Length - 3);

            return true;
        }

        // 校验价格
        public static List<string> VerifyPrice(string strPrice)
        {
            List<string> errors = new List<string>();

            // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out CurrencyItem item,
                out string strError);
            if (nRet == -1)
                errors.Add(strError);

            // 2020/7/8
            // 检查货币字符串中是否出现了字母以外的字符
            if (string.IsNullOrEmpty(item.Postfix) == false)
                errors.Add($"金额字符串 '{strPrice}' 中出现了后缀 '{item.Postfix}' ，这很不常见，一般意味着错误");

            string error1 = VerifyPricePrefix(item.Prefix);
            if (error1 != null)
                errors.Add(error1);

            string new_value = StringUtil.ToDBC(strPrice);
            if (new_value.IndexOfAny(new char[] { '(', ')' }) != -1)
            {
                errors.Add("价格字符串中不允许出现括号 '" + strPrice + "'");
            }

            if (new_value.IndexOf(',') != -1)
            {
                errors.Add("价格字符串中不允许出现逗号 '" + strPrice + "'");
            }

            return errors;
        }



        // 将价格中含有的汉字型数值替换为数字
        static string[] chinese = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        public static string ParsePrice(string price)
        {
            for (int i = 0, length = price.Length; i < length && i < chinese.Length; i++)
            {
                if (price.IndexOf(chinese[i]) != -1)
                    price = price.Replace(chinese[i], i.ToString());
            }

            price = price.Replace("两", "2");

            return price;
        }


        // 已知格式价格内容转换
        // 无 = CNY0
        // 五 = CNY0
        // CN8.00 = CNY8.00
        // cny22.60 = CNY22.60
        // &&&&&&&&&&&&&&&&&&&1.10x6 = CNY1.10
        // 1180.00? = CNY1180.00
        // 4.45 . = CNY4.45
        // 12.50精 = CNY12.50
        // CNBY45.00 = CNY45.00
        // CNYCNBY45.00 = CNY45.00
        // CNY64.00 全 = CNY64.00
        // CNY8.00 共  = CNY8.00
        // CNY无2 = CNY2
        // CNYZ180.00 = CNY180.00
        // $14 = CNY14
        // CN￥33.00 = CNY33.00
        // RMB25.00 = CNY25.00
        // CNY精装 128.00 = CNY128.00
        // 精装17.20 = CNY17.20
        // 12.00 套 = CNY12.00
        // 9.00。80 = CNY9.00
        // 9，.50 = CNY9.50
        // 9,.50 = CNY9.50
        //  CNY51,000元(旧币)  = CNY51000
        // CNY68.00（含光盘） = CNY68.00
        // ￥120.00 = CNY120.00
        // CNY 34.50 = CNY34.50
        // ?.65 = CNY0.65
        // .45 = CNY0.45
        // CNY120.00(上下) = CNY120.00(上下册) = CNY120.00/2
        // CNY2650.00(全套42册) = CNY2650.00(全42册) = CNY2650.00/42
        // ￥980.00(全2卷) = CNY980.00(全2册) = CNY980.00/2
        string CorrectPrice(string price)
        {
            string result = price;

            if (!string.IsNullOrEmpty(price))
            {
                int nIndex = price.IndexOf('。');
                if (nIndex != -1)
                    price = price.Substring(0, nIndex);

                if (price.StartsWith("?."))
                    price = price.Replace("?.", "0.");
                else if (price.StartsWith("."))
                    price = "0" + price;
                else if (price.StartsWith("精装"))
                    price = price.Replace("精装", "");
                // else if (price.StartsWith("cny"))
                // price = price.ToLower();
                else if (price.StartsWith("CNBY") || price.StartsWith("CNYCNBY"))
                    price = price.Replace("CNYCNBY", "CNY").Replace("CNBY", "CNY");
                else if (price.StartsWith("CNY无"))
                    price = price.Replace("CNY无", "CNY");
                else if (price.StartsWith("CNYZ"))
                    price = price.Replace("CNYZ", "CNY");
                else if (price == "CNY")
                    price = "CNY0";

                if (price.EndsWith("共") || price.EndsWith("全") || price.EndsWith("精"))
                    price = price.Replace("共", "").Replace("全", "").Replace("精", "");
                else if (price.EndsWith("."))
                    price = price.Remove(price.Length - 1);

                // string pattern = @"\d{0,2}(,|，)\d{3}\.(\d){0,2}";
                string pattern = @"^(CNY)?(\d{1,2})(\,\d{3})+\.?\d{0,2}";// @"^(CNY){0,1}(\d{1,2})(\,\d{3}){1,}\.{0,1}\d{0,2}";
                if (Regex.IsMatch(price, pattern))
                {
                    price = price.Replace(",", "");
                }

                pattern = @"^CN\d";
                if (Regex.IsMatch(price, pattern))
                {
                    price = price.Replace("CN", "CNY");
                }

                price = price.Replace("套", "").Replace("上下", "上下册");
                price = price.Replace("上下册册", "上下册");
                price = price.Replace("，.", ".").Replace(",.", ".").Replace("?", "");
                price = price.Trim().Replace("元", "").Replace(" ", "").Replace("$", "CNY");
                price = price.Replace("CN￥", "￥").Replace("￥", "CNY");
                price = price.Replace("RMB", "CNY").Replace("CNY精装", "CNY");

                result = price.Trim();
            }
            return result;
        }

        public static bool IsPriceCorrect(string strPrice)
        {
            if (string.IsNullOrEmpty(strPrice))
                return false;

#if NO
            string strError = "";
            CurrencyItem item = null;
            // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out item,
                out strError);
            if (nRet == -1)
                return false;

            return true;
#endif
            if (VerifyPrice(strPrice).Count > 0)
                return false;
            return true;
        }

        #endregion

        private void textBox_isofilename_TextChanged(object sender, EventArgs e)
        {
            // 自动显示文件的第一条
            DisplayFirstRecord(this.FileName, this.Encoding);
        }

        /// <summary>
        /// 获取或设置文件名全路径
        /// </summary>
        public string FileName
        {
            get
            {
                return this.textBox_isofilename.Text;
            }
            set
            {
                this.textBox_isofilename.Text = value;

                // 自动显示文件的第一条
                DisplayFirstRecord(value, this.Encoding);
            }
        }

        private void comboBox_encoding_TextChanged(object sender, EventArgs e)
        {
            // 自动显示文件的第一条
            try
            {
                DisplayFirstRecord(this.FileName, this.Encoding);
            }
            catch
            {
                // TODO: 最好把 combobox 显示为特殊颜色，表示输入的编码方式名称不合法
            }
        }

        private void comboBox_source_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_source.Text.ToUpper() == "DT1000")
            {
                
            }
        }
    }



}

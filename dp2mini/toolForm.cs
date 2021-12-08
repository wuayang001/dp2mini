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
using System.Linq;
using System.Text.RegularExpressions;
using DigitalPlatform.Text;
using ClosedXML.Excel;

namespace dp2mini
{
    public partial class toolForm : Form
    {
        // mid父窗口
        MainForm _mainForm = null;

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public EntityDbManager _dbManager = null;

        /// <summary>
        ///  构造函数
        /// </summary>
        public toolForm()
        {
            InitializeComponent();
        }

        // excel
        XLWorkbook _workbook= new XLWorkbook();

        /// <summary>
        /// 窗体装载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolForm_Load(object sender, EventArgs e)
        {
            this._mainForm = this.MdiParent as MainForm;

        }





        #region 界面操作按钮

        public string Dir
        {
            get
            {
                string dir= this.textBox_dir.Text.Trim();
                if (string.IsNullOrEmpty(dir) == true)
                {
                    //先选择输出目录
                    button_selectDir_Click(null, null);
                    dir = this.textBox_dir.Text.Trim();


                }
                return dir;
            }
        }

        StreamWriter _sw = null;
        public StreamWriter Sw
        {
            get
            {
                if (this._sw == null)
                {
                    string fileName = this.Dir + "/"+this._mainForm.LibraryName+"-巡检结果.txt";
                    if (File.Exists(fileName) == true)
                        File.Delete(fileName);
                    _sw = new StreamWriter(fileName, false, Encoding.UTF8);
                }

                return this._sw;
            }
        }

        // 一键巡检
        private void button_oneKey_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 先清一下文件
                if (this._sw != null)
                {
                    _sw.Close();
                    _sw = null;
                }
                this.textBox_info.Text = "";


                // 获取数据库信息
                this.button_listdb_Click(sender, e);

                // 获取册
                SearchItem(this._cancel.Token);

                // 馆藏地
                this.button_checkLocation_Click(sender, e);

                // 图书类型
                this.button_checkBookType_Click(sender, e);

                // 排架体系
                this.button_paijia_Click(sender, e);
                // 索取号
                this.button_checkAccessNo_Click(sender, e);



                // 册价格
                this.button_checkPrice_Click(sender, e);


                // 获取流通权限
                this.button_circulationRight_Click(sender, e);

                // 统计读者
                this.checkDepartment();

                // 检查权限
                this.CheckRights();

                // 需放在最后，获取校验函数
                this.button_script_Click(sender, e);
                this.button_verifyBarcode_Click(sender, e);
            }
            finally
            {
                EnableControls(true);


            }

        }

        // 停止
        private void button_stop_Click(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }

        // 列出书目库信息
        private void button_listdb_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                this.OutputInfo("==开始获取书目信息==");
                this.GetDbInfos();
                this.OutputInfo("==结束获取书目信息，详见excel==");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 获取流通权限
        private void button_circulationRight_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);
                this.OutputInfo("==开始获取流通权限==");
                this.GetCirculationRight();
                this.OutputInfo("==结束获取流通权限，详见右侧==");
            }
            finally
            {
                EnableControls(true);
            }
        }



        // 统计读者
        private void button_patron_Click(object sender, EventArgs e)
        {

        }

        // 检查权限
        private void button_right_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {

            }
            finally
            {
                EnableControls(true);
            }
        }

        #endregion


        #region 通用函数

        // 清信息
        private void button_clear_Click(object sender, EventArgs e)
        {
            this.textBox_info.Text = "";
        }

        public void OutputInfo(string info,bool isTextBox=true,bool isState=true)
        {
            this.Invoke((Action)(() =>
            {
                if (isTextBox == true)
                {
                    this.textBox_info.Text += info + "\r\n";


                    this.textBox_info.Focus();//获取焦点
                    this.textBox_info.Select(this.textBox_info.TextLength, 0);//光标定位到文本最后
                    this.textBox_info.ScrollToCaret();//滚动到光标处

                    // 写到文件
                    this.Sw.WriteLine(info);
                    this.Sw.Flush();
                }

                if (isState == true)
                {
                    //设置父窗口状态栏参数
                    this._mainForm.SetStatusMessage(info);
                }
            }
            ));
        }

        public void OnlyOutput2File(string info)
        {
            // 写到文件
            this.Sw.WriteLine(info);
            this.Sw.Flush();
        }

        /// <summary>
        /// 设置控件是否可用
        /// </summary>
        /// <param name="bEnable"></param>
        void EnableControls(bool bEnable)
        {
            this.button_oneKey.Enabled = bEnable;
            this.button_stop.Enabled = bEnable?false:true;

            this.button_listdb.Enabled = bEnable;  // 书目
            this.button_entity.Enabled = bEnable; //获取册
            this.button_checkLocation.Enabled = bEnable; // 馆藏地
            this.button_checkBookType.Enabled = bEnable; //图书类型
            this.button_paijia.Enabled = bEnable;  //排架
            this.button_checkAccessNo.Enabled = bEnable;  //索取号
            this.button_script.Enabled = bEnable; //校验函数
            this.button_verifyBarcode.Enabled = bEnable;  //校验册条码
            this.button_checkPrice.Enabled = bEnable;  //册价格

            this.button_circulationRight.Enabled = bEnable;  //流通权限

            this.button_patron.Enabled = bEnable;  //校验读者
            this.button_right.Enabled = bEnable;  //权限
            this.button_loc.Enabled = bEnable; //单馆藏地

            this.button_clear.Enabled = bEnable;     //清信息
        }


        #endregion


        #region 获取数据库信息

        // 获取数据库信息
        public void GetDbInfos()
        {
            string strError = "";

            // sheet名：书目库
            IXLWorksheet ws = null;
            bool bExist=this._workbook.Worksheets.TryGetWorksheet("书目库",out ws);
            if (bExist == true)
                this._workbook.Worksheets.Delete("书目库");

           ws = this._workbook.Worksheets.Add("书目库");
            // 设置标题
            string[] titles = {
            "序号",
            "书目库名",
            "数据量",
            "是否流通",
            "数据格式",
            "文献类型",
            "角色",
            "下级子库"
            };
            int i = 1;
            foreach (string s in titles)
            {
                ws.Cell(1, i++).Value = s;
            }


            // 已输出excel，则不输出的文本了
            //this.OutputInfo("序号\t书目库名\t 数据量\t是否流通\t数据格式\t文献类型\t角色\t下级子库",
            //    true,false);


            // 获取书目库
            string strValue = this.GetSystemParameter("system", "biblioDbGroup");
            //<database biblioDbName="中文图书" syntax="unimarc"
            // orderDbName="中文图书订购" commentDbName="中文图书评注"
            // inCirculation="true" role="catalogWork" itemDbName="中文图书实体"  issueDbName="中文期刊期" >
            string xml = strValue;
            xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNodeList list = dom.DocumentElement.SelectNodes("database");
            List<db> dbs = new List<db>();
            foreach (XmlNode node in list)
            {
                Application.DoEvents();

                db one = new db();
                one.biblioDbName = DomUtil.GetAttr(node, "biblioDbName");
                one.itemDbName = DomUtil.GetAttr(node, "itemDbName");
                one.orderDbName = DomUtil.GetAttr(node, "orderDbName");
                one.commentDbName = DomUtil.GetAttr(node, "commentDbName");
                one.issueDbName = DomUtil.GetAttr(node, "issueDbName");
                one.syntax = DomUtil.GetAttr(node, "syntax");
                one.inCirculation = DomUtil.GetAttr(node, "inCirculation");
                one.role = DomUtil.GetAttr(node, "role");
                dbs.Add(one);
            }

            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                int index = 0;
                long lTotalCount = 0;  //总数量
                long lCirCount = 0; //流通库数据总量
                int cirDbCount = 0; //流通库个数
                foreach (db one in dbs)
                {
                    Application.DoEvents();
                    index++;

                    string type = "Book-图书";
                    if (string.IsNullOrEmpty(one.issueDbName) == false)
                        type = "Series-期刊";
                    string output = "";
                    long lRet = channel.SearchBiblio(one.biblioDbName,
                       "",
                       -1,
                       "recid",
                       "left",
                       "test",
                       "",
                       out output,
                       out strError);
                    if (lRet == -1)
                    {
                        this.OutputInfo("获取数据库[" + one.biblioDbName + "]的书目记录数量出错：" + strError,
                            true,false);
                        continue;
                    }

                    List<string> children = new List<string>();
                    if (string.IsNullOrEmpty(one.itemDbName) == false)
                        children.Add(one.itemDbName);
                    if (string.IsNullOrEmpty(one.orderDbName) == false)
                        children.Add(one.orderDbName);
                    if (string.IsNullOrEmpty(one.commentDbName) == false)
                        children.Add(one.commentDbName);
                    if (string.IsNullOrEmpty(one.issueDbName) == false)
                        children.Add(one.issueDbName);
                    


                    string[] row = {
            index.ToString(),
             one.biblioDbName,
            lRet.ToString(),
           one.inCirculation.ToString(),
            one.syntax,
           type,
            one.role,
           string.Join("\r\n",children) //one.itemDbName + "\r\n" + one.orderDbName + "\r\n" + one.commentDbName + "\r\n" + one.issueDbName
                    };


                    // 已输出excel，则不输出的文本了
                    //string info = string.Join("\t", row).Replace("\r\n", ",");
                    //this.OutputInfo(info,true,false);

                    // 写excel
                    i = 1;
                    foreach (string s in row)
                    {
                        ws.Cell(index+1, i++).Value = s;
                    }


                    //流通库数量和记录数
                    if (one.inCirculation == "true")
                    {
                        cirDbCount++;
                        lCirCount += lRet;
                    }

                    //总数量
                    lTotalCount += lRet;
                }

                this.OutputInfo("\r\n图书馆系统中总共有" + index + "个书目库，书目记录总数量为" + lTotalCount
                    + "，其中有" + cirDbCount + "个库可流通，流通的书目种数为" + lCirCount + "。" + "\r\n",
                    true,false);


                // 设置excel格式
                this.SetExcelStyle(ws, index + 1,titles.Length);

                //保存excel
                this._workbook.SaveAs(this.ExcelFileName);

                return;
            }
            finally
            {
                this._mainForm.ReturnChannel(channel);
            }
        }

        #endregion

        #region 册统计

        // 内存中的册记录
        List<Entity> _items = new List<Entity>();


        public void SearchItem(CancellationToken token)
        {
            // 空2行
            this.OutputEmprty(2);

            this.OutputInfo("==开始获取册信息==");
            Application.DoEvents();

            string strError = "";
            long lRet = 0;

            // 记下原来的光标
            Cursor oldCursor = Cursor.Current;

            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                // 设置按钮状态
                EnableControls(false);

                // 输出信息框清空,不要清空
                // this.textBox_info.Text = "";

                //设置父窗口状态栏参数
                this._mainForm.SetStatusMessage("");

                // 鼠标设为等待状态
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
            }
            ));
            try
            {
                // 获取所有册记录，保存到本地数组
                //List<Entity> items = new List<Entity>();
                RestChannel channel = this._mainForm.GetChannel();
                try
                {
                    token.ThrowIfCancellationRequested();

                    // 检查全部册
                    lRet = channel.SearchItem(//stop,
                       "<all>",
                       "", // 
                       -1,
                       "__id",
                       "left",
                       "myresult",
                       "id,xml",
                       out strError);
                    if (lRet == -1)
                    {
                        this.OutputInfo("检索册记录出错：" + strError,true,false);
                        return;
                    }

                    // 输出命中信息
                    this.OutputInfo("系统中册数量总计" + this._items.Count.ToString() + "册。", true, false);

                    //this.OutputInfo("共命中" + lRet.ToString() + "册");


                    long lHitCount = lRet;
                    long lStart = 0;
                    long lCount = lHitCount;

                    // 从结果集中取出册记录
                    for (; ; )
                    {
                        //Application.DoEvents(); // 出让界面控制权

                        token.ThrowIfCancellationRequested();


                        Record[] searchresults = null;
                        lRet = channel.GetSearchResult(
                            //stop,
                            "myresult",   // strResultSetName
                            lStart,
                            lCount,
                            "id,xml",
                            "zh",
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                        {
                            this.OutputInfo("已获取" + lStart.ToString() + "条，获取结果集出错：" + strError,
                                true,false);
                            return;
                        }

                        // 说明结果集里的记录获取完了。
                        if (lRet == 0)
                        {
                            break;
                        }


                        // 处理本批取到的册记录
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            Record record = searchresults[i];

                            Entity item = new Entity();
                            item.path = record.Path;
                            this._items.Add(item);

                            /*
    <barcode>B001</barcode> 
    <location>图书馆</location> 
    <accessNo>K825.6=76/T259</accessNo> 
    <bookType>普通</bookType> 
    <price>a34.80b</price> 
     */
                            string xml = record.RecordBody.Xml;
                            XmlDocument dom = new XmlDocument();
                            dom.LoadXml(xml);
                            XmlNode root = dom.DocumentElement;
                            item.barcode = DomUtil.GetElementInnerText(root, "barcode");
                            item.location = DomUtil.GetElementInnerText(root, "location");
                            item.accessNo = DomUtil.GetElementInnerText(root, "accessNo");
                            item.bookType = DomUtil.GetElementInnerText(root, "bookType");
                            item.price = DomUtil.GetElementInnerText(root, "price");


                            // info
                            this.OutputInfo("获取第" + (lStart + i).ToString(),false,true);
                        }

                        this.OutputInfo("已获取" + (lStart + searchresults.Length).ToString() + "条",
                            true,false);


                        lStart += searchresults.Length;
                        lCount -= searchresults.Length;

                        if (lStart >= lHitCount || lCount <= 0)
                            break;
                    }
                }
                finally
                {
                    this._mainForm.ReturnChannel(channel);
                }
            }
            finally
            {


                // 用Invoke线程安全的方式来调
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                    this.Cursor = oldCursor;
                }
                ));

                //完成提示
                this.OutputInfo("==结束获取册信息==");
            }
        }

        // 校验册条码

        public void VerifyBarcode(List<Entity> items, CancellationToken token)
        {
            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                EnableControls(false);
            }
            ));

            // 空2行
            this.OutputEmprty(2);

            this.OutputInfo("==开始校验册条码==");

            try
            {

                string strError = "";

                long errorCount = 0;
                RestChannel channel = this._mainForm.GetChannel();
                try
                {
                    int index = 0;
                    foreach (Entity item in items)
                    {
                        // 当外部让停止时，停止循环
                        token.ThrowIfCancellationRequested();
                        //Application.DoEvents();

                        index++;
                        this.OutputInfo("校验册条码第" + index.ToString(),false,true);

                        /// <param name="strLibraryCode">馆代码</param>
                        /// <param name="strBarcode">条码号</param>
                        /// <param name="strError">返回出错信息</param>
                        /// <returns>
                        /// <para>-1:   出错</para>
                        /// <para>0/1/2:    分别对应“不合法的标码号”/“合法的读者证条码号”/“合法的册条码号”</para>
                        // 校验册条码
                        long lRet = channel.VerifyBarcode("",
                            item.barcode,
                            out strError);
                        if (lRet == -1)
                        {
                            this.OutputInfo("校验册条码出错:" + strError,true,false);
                            return;
                        }
                        // 不合法
                        if (lRet == 0 || lRet == 1)
                        {
                            errorCount++;
                            if (errorCount == 1)
                            {
                                // this.OutputInfo("以下册条码不符合规则");
                                // 仅输出到文件
                                this.OnlyOutput2File("以下册条码不符合规则");
                            }

                            if (lRet == 0)
                            {
                                //仅输出到文件
                                this.OnlyOutput2File(item.path + "\t" + item.barcode);// + "\t" + strError);
                                //this.OutputInfo(item.path + "\t" + item.barcode + "\t" + strError, true, false);
                            }
                            else if (lRet == 1)
                            {
                                //仅输出到文件
                                this.OnlyOutput2File(item.path + "\t" + item.barcode);// + "\t" + "与读者证规则冲突");
                                //this.OutputInfo(item.path + "\t" + item.barcode + "\t" + "与读者证规则冲突", true, false);
                            }
                        }

                        token.ThrowIfCancellationRequested();

                    }

                    if (errorCount > 0)
                    {
                        this.OutputEmprty();
                        this.OutputInfo("不符合规则的册条码共有" + errorCount + "条。");
                    }
                }
                finally
                {
                    this._mainForm.ReturnChannel(channel);
                }
            }
            finally
            {
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                }
                ));


            }

            this.OutputInfo("==结束校验册条码，详见txt文件==");
        }

        // 校验价格
        private void CheckPrice(List<Entity> items, CancellationToken token)
        {
            string match = "^[0-9]*([.][0-9]*)$";// this.txtMatch.Text.Trim();

            //空价格的
            List<string> emptyList = new List<string>();
            //高于500价格
            List<string> largeList = new List<string>();
            //括号
            List<string> bracketList = new List<string>();
            //其它
            List<string> otherList = new List<string>();

            int index = 0;
            foreach (Entity item in items)//string line in lines)
            {
                // 当外部让停止时，停止循环
                token.ThrowIfCancellationRequested();

                index++;
                this.OutputInfo("校验册价格第" + index.ToString(),false,true);

                string path = item.path;
                string price = item.price;
                if (price == null)
                    price = "";

                Application.DoEvents();

                string retLine = path + "\t" + price;

                if (price == "")
                {
                    emptyList.Add(retLine);
                    continue;
                }

                if (price.Length > 3 &&
                    (price.Substring(0, 3) == "CNY"
                    || price.Substring(0, 3) == "USD"
                    || price.Substring(0, 3) == "KWR"
                    || price.Substring(0, 3) == "TWD"
                    || price.Substring(0, 3) == "HKD"
                    || price.Substring(0, 3) == "JPY"
                    || price.Substring(0, 3) == "EUR")
                    )
                {
                    string right = price.Substring(3);

                    // 未定义
                    if (right == "")
                    {
                        emptyList.Add(retLine);
                        continue;
                    }

                    //大于200
                    try
                    {
                        double dPrice = Convert.ToDouble(right);
                        if (dPrice > 200)
                        {
                            largeList.Add(retLine);
                        }
                        continue;
                    }
                    catch
                    {

                        int nTemp = right.IndexOf('/');
                        if (nTemp > 0)
                        {
                            string r1 = right.Substring(0, nTemp);
                            string r2 = right.Substring(nTemp + 1);
                            try
                            {
                                double dR1 = Convert.ToDouble(r1);
                                double dR2 = Convert.ToDouble(r2);
                                continue;
                            }
                            catch
                            {

                            }
                        }
                    }

                    // 正常的
                    bool bRet = Regex.IsMatch(right, match);// "^[0-9]*(.[0-9])$");
                    if (bRet == true)
                        continue;

                    //带括号的
                    if (right.IndexOf("(") != -1 || right.IndexOf("（") != -1)
                    {
                        bracketList.Add(retLine);
                        continue;
                    }
                }

                otherList.Add(retLine);
            }



            //输出
            StringBuilder result = new StringBuilder();

            if (emptyList.Count > 0)
            {
                result.AppendLine("\r\n下面是册价格为空的，有" + emptyList.Count + "条。");
                foreach (string li in emptyList)
                {
                    result.AppendLine(li);
                }
            }

            //
            if (largeList.Count > 0)
            {
                result.AppendLine("\r\n下面是册价格很高超过CNY200异常的，有" + largeList.Count + "条。");
                foreach (string li in largeList)
                {
                    result.AppendLine(li);
                }
            }
            //

            if (bracketList.Count > 0)
            {
                result.AppendLine("\r\n下面是册价格带括号异常的，有" + bracketList.Count + "条。");
                foreach (string li in bracketList)
                {
                    result.AppendLine(li);
                }
            }

            //
            if (otherList.Count > 0)
            {
                result.AppendLine("\r\n下面是其它册价格不合规则的，有" + otherList.Count + "条。");
                foreach (string li in otherList)
                {
                    result.AppendLine(li);
                }
            }

            // 输出到界面上
            //this.OutputInfo(result.ToString(),true,false);

            // 仅输出到文件
            this.OnlyOutput2File(result.ToString());


        }

        // 获取聚合的location
        private LocGroup GetLocGroup(List<LocGroup> list, string name)
        {
            foreach (LocGroup group in list)
            {
                if (group.location == name)
                {
                    return group;
                }
            }
            return null;
        }

        // 获取聚合的类型
        private TypeGroup GetTypeGroup(List<TypeGroup> list, string name)
        {
            foreach (TypeGroup group in list)
            {
                if (group.bookType == name)
                {
                    return group;
                }
            }
            return null;
        }

        // 获取馆藏地
        public List<simpleLoc> GetLocation()
        {
            List<simpleLoc> locList = new List<simpleLoc>();

            string strValue = this.GetSystemParameter("circulation", "locationTypes");
            /*
            <root><item canborrow="yes" canreturn="" itemBarcodeNullable="yes">流通库</item>
            <item canborrow="yes" canreturn="" itemBarcodeNullable="yes">走廊</item>
            <item canborrow="yes" canreturn="" itemBarcodeNullable="no">共享书柜</item>
            <item canborrow="yes" canreturn="" itemBarcodeNullable="no">一楼阅览室</item>
            <item canborrow="no" canreturn="" itemBarcodeNullable="yes">二楼阅览室</item>
            <library code="第三中学">
            <item canborrow="no" canreturn="" itemBarcodeNullable="no">大阅读室</item>
            <item canborrow="yes" canreturn="" itemBarcodeNullable="no">图书馆</item>
            <item canborrow="no" canreturn="" itemBarcodeNullable="yes">一年级</item>
            </library>
            */
            string xml = "<root>" + strValue + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNodeList nodelist = dom.DocumentElement.SelectNodes("item");
            foreach (XmlNode node in nodelist)
            {
                string location = DomUtil.GetNodeText(node);
                string canBorrow = DomUtil.GetAttr(node, "canborrow");
                simpleLoc simpleLoc = new simpleLoc();
                simpleLoc.name = location;
                simpleLoc.canBorrow = canBorrow;
                locList.Add(simpleLoc);
            }

            // 分馆的馆藏地
            nodelist = dom.DocumentElement.SelectNodes("library/item");
            foreach (XmlNode node in nodelist)
            {
                string location = DomUtil.GetAttr(node.ParentNode, "code") + "/" + DomUtil.GetNodeText(node);
                string canBorrow = DomUtil.GetAttr(node, "canborrow");
                simpleLoc simpleLoc = new simpleLoc();
                simpleLoc.name = location;
                simpleLoc.canBorrow = canBorrow;
                locList.Add(simpleLoc);
            }

            return locList;
        }


        // 对馆藏地进行聚合
        public void GroupLocation(List<Entity> items, CancellationToken token)
        {

            // 按馆藏地统计册数
            var list = items.GroupBy(
                x => new { x.location },
                (key, item_list) => new LocGroup
                {
                    location = key.location,
                    Items = new List<Entity>(item_list),
                    count = item_list.Count()
                }).OrderByDescending(o => o.Items.Count);

            List<LocGroup> locs = new List<LocGroup>();
            foreach (LocGroup group in list)
            {
                locs.Add(group);
            }


            // sheet名：馆藏地
            IXLWorksheet ws = this.CreateSheet("馆藏地");

            // 设置标题
            string[] titles = {
            "馆藏地名称",
            "允许外借",
            "册数量"
            };
            int i = 1;
            foreach (string s in titles)
            {
                ws.Cell(1, i++).Value = s;
            }

            // 已输出excel，则不输出的文本了
            // 输出表头
            //this.OutputInfo("馆藏地名称\t允许外借\t册数量",true,false);

            // 系统里配置的馆藏地
            int index = 0;
            List<simpleLoc> locList = this.GetLocation();
            foreach (simpleLoc one in locList)
            {
                index++;

                LocGroup group = this.GetLocGroup(locs, one.name);
                int count = 0;
                if (group != null)
                {
                    count = group.count;
                    group.isConfig = true; //表示在系统已定义过
                }

                // 已输出excel，则不输出的文本了
                //this.OutputInfo(one.name + "\t" + one.canBorrow + "\t" + count,true,false);
                
                // 写excel
                ws.Cell(index+1, 1).Value = one.name;
                ws.Cell(index + 1,2).Value = one.canBorrow;
                ws.Cell(index + 1, 3).Value = count.ToString();
            }



            // 把没有数据的馆藏地显示在下方
            foreach (LocGroup loc in locs)
            {
                if (loc.isConfig == false)
                {
                    index++;
                    
                    // 输出excel
                    //this.OutputInfo(loc.location + "\t" + "未定义" + "\t" + loc.count,true,false);

                    // 写excel
                    ws.Cell(index + 1, 1).Value = loc.location;
                    ws.Cell(index + 1, 2).Value = "未定义";
                    ws.Cell(index + 1, 3).Value = loc.count;
                }
            }

            // 设置excel格式
            this.SetExcelStyle(ws,  index + 1,titles.Length);

            //保存excel
            this._workbook.SaveAs(this.ExcelFileName);

            return;
        }

        public string ExcelFileName
        {
            get
            {
                return this.Dir + "/" + this._mainForm.LibraryName + "-巡检结果.xlsx";
                //return this.Dir + "/巡检结果.xlsx"; 
            }
        }

        private IXLWorksheet CreateSheet(string sheetName)
        {
            IXLWorksheet ws = null;
            bool bExist = this._workbook.Worksheets.TryGetWorksheet("馆藏地", out ws);
            if (bExist == true)
                this._workbook.Worksheets.Delete("馆藏地");
            ws = this._workbook.Worksheets.Add("馆藏地");

            return ws;
        }

        public void SetExcelStyle(IXLWorksheet ws,int rowCount, int colCount)
        {
            // 第一行加粗，背景发灰
            var range = ws.Range(1, 1, 1, colCount);
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.LightGray;

            //第三列为文本格式
            //ws.Column(3).Style.NumberFormat.Format = "@";

            range = ws.Range(1, 1, rowCount, colCount);
            range.Style.Font.SetFontName("微软雅黑");
            range.Style.Font.SetFontSize(8);
            ws.Columns().AdjustToContents();  // Adjust column width
            ws.Rows().AdjustToContents();     // Adjust row heights
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }


        // 对图书类型进行聚合
        public void GroupType(List<Entity> items, CancellationToken token)
        {
            // 让用户选择需要统计的范围。根据批次号、目标位置来进行选择
            var list = items.GroupBy(
                x => new { x.bookType },
                (key, item_list) => new TypeGroup
                {
                    bookType = key.bookType,
                    Items = new List<Entity>(item_list),
                    count = item_list.Count()
                }).OrderByDescending(o => o.Items.Count);



            List<TypeGroup> types = new List<TypeGroup>();

            foreach (TypeGroup group in list)
            {
                if (String.IsNullOrEmpty(group.bookType) == true)
                    group.bookType = "[空]";

                types.Add(group);
            }


            // sheet名：馆藏地
            IXLWorksheet ws = this.CreateSheet("图书类型");

            // 设置标题
            string[] titles = {
            "图书类型",
            "册数量",
            "备注"
            };
            int i = 1;
            foreach (string s in titles)
            {
                ws.Cell(1, i++).Value = s;
            }

            // 已输出excel，则不输出的文本了
            // 输出表头
            //this.OutputInfo("图书类型	\t册数量\t备注",true,false);

            // 获取配置的图书类型
            int index = 0;
            List<string> bookTypes = this.GetCirTypes("bookTypes");
            foreach (string one in bookTypes)
            {
                index++; 

                TypeGroup group = this.GetTypeGroup(types, one);
                int count = 0;
                if (group != null)
                {
                    count = group.count;
                    group.isConfig = true; //表示在系统已定义过
                }

                // 已输出excel，则不输出的文本了
                //this.OutputInfo(one +  "\t" + count, true, false);

                // 写excel
                ws.Cell(index + 1, 1).Value = one;
                ws.Cell(index + 1, 2).Value = count.ToString();
                ws.Cell(index + 1, 3).Value = "";
            }

            // 把没有数据的馆藏地显示在下方
            foreach (TypeGroup one in types)
            {
                if (one.isConfig == false)
                {
                    index++;

                    // 已输出excel，则不输出的文本了
                    //this.OutputInfo(one.bookType+  "\t" +one.count+"\t" + "未定义", true, false);

                    // 写excel
                    ws.Cell(index + 1, 1).Value = one.bookType;
                    ws.Cell(index + 1, 2).Value = one.count.ToString();
                    ws.Cell(index + 1, 3).Value = "未定义";
                }
            }

            // 设置excel格式
            this.SetExcelStyle(ws, index + 1, titles.Length);

            //保存excel
            this._workbook.SaveAs(this.ExcelFileName);
        }

        public List<string> GetCirTypes(string type)
        {
            List<string> types = new List<string>();

            /*
<rightsTable>  
    <readerTypes>
        <item>读者</item>
        <item>test</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
    <library code="星洲学校">
        <readerTypes>
            <item>教师</item>
            <item>志愿者</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
            <item>教材</item>
            <item>[空]</item>
        </bookTypes>
    </library>
</rightsTable>
             */

            // 流通权限定义的图书类型
            string strValue = this.GetSystemParameter("circulation", "rightsTable");
            if (string.IsNullOrEmpty(strValue) == false)
            {
                strValue = "<root>" + strValue + "</root>";
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strValue);
                XmlNodeList nodeList = dom.DocumentElement.SelectNodes(type+"/item");
                foreach (XmlNode node in nodeList)
                {
                    string value = node.InnerText.Trim();
                    if (string.IsNullOrEmpty(value)==false)
                    {
                        types.Add(value);
                    }
                }

                nodeList = dom.DocumentElement.SelectNodes("library");
                foreach (XmlNode node in nodeList)
                {
                    string library = DomUtil.GetAttr(node, "code").Trim();
                    XmlNodeList list1 = node.SelectNodes(type + "/item");
                    foreach (XmlNode one in list1)
                    {
                        string value = one.InnerText.Trim();
                        if (string.IsNullOrEmpty(value) == false)
                        {
                            types.Add(library+"/"+value);
                        }
                    }
                }
            }

            return types;
        }

        #region 检查索取号

        // 检查索取号
        private void CheckAccessNo(List<Entity> items, CancellationToken token)
        {

            //空索取号的
            List<string> emptyList = new List<string>();

            //没有斜撇
            List<string> noXpList = new List<string>();

            //有斜撇，但左或右没有值
            List<string> hasXpNoValueList = new List<string>();

            //左右有空格的
            List<string> hasKgList = new List<string>();

            List<string> leftWrongList = new List<string>();
            List<string> rightWrongList = new List<string>();

            // 其它
            List<string> otherList = new List<string>();
            int index = 0;
            foreach (Entity item in items)//string line in lines)
            {
                // 当外部让停止时，停止循环
                token.ThrowIfCancellationRequested();

                index++;
                this.OutputInfo("校验索取号第" + index.ToString(),false,true);

                //if (line == "")
                //    continue;

                string path = item.path;
                string accessNo = item.accessNo;
                if (accessNo == null)
                    accessNo = "";

                string retLine = path + "\t" + accessNo;

                // 无索取号的
                if (accessNo == "")
                {
                    emptyList.Add(retLine);
                    continue;
                }

                int nTemp = accessNo.IndexOf('/');
                if (nTemp == -1)
                {
                    //索取号不包括/
                    noXpList.Add(retLine);
                    continue;
                }

                string left = accessNo.Substring(0, nTemp);
                string right = accessNo.Substring(nTemp + 1);
                if (left == "" || right == "")
                {
                    //有斜撇，但左或右没有值
                    hasXpNoValueList.Add(retLine);
                    continue;
                }

                string left1 = left.Trim();
                string right1 = right.Trim();
                if (left1 != left || right1 != right)
                {
                    //左右有空格的
                    hasKgList.Add(retLine);
                    continue;
                }

                // 左侧不是字母的
                string firstLeft = left.Substring(0, 1);
                if (StringUtil.Between(firstLeft, "A", "Z") == false)
                {
                    leftWrongList.Add(retLine);
                    continue;
                }

                // 右边不合适
                try
                {
                    double d = Convert.ToDouble(right);
                    continue;
                }
                catch
                {
                    string firstRight = right.Substring(0, 1);
                    if (StringUtil.Between(firstRight, "A", "Z") == true)
                        continue;

                    if (StringUtil.Between(firstRight, "A", "Z") == false)
                    {
                        nTemp = right.IndexOf(":");
                        if (nTemp == -1)
                            nTemp = right.IndexOf("：");
                        if (nTemp == -1)
                            nTemp = right.IndexOf("=");
                        if (nTemp == -1)
                            nTemp = right.IndexOf(";");
                        if (nTemp == -1)
                            nTemp = right.IndexOf("；");

                        if (nTemp > 0)
                        {
                            string strFirst = right.Substring(0, nTemp);
                            //string strEnd = right.Substring(nTemp + 1);
                            try
                            {
                                int n = Convert.ToInt32(strFirst);
                                continue;
                            }
                            catch
                            { }
                        }

                        rightWrongList.Add(retLine);
                        continue;
                    }
                }

                //
                otherList.Add(retLine);

            }

            StringBuilder result = new StringBuilder ();


            //空索取号的
            if (emptyList.Count > 0)
            {
                result.AppendLine("\r\n下面是索取号为空的册记录，有" + emptyList.Count + "条。");
                foreach (string li in emptyList)
                {
                    result.AppendLine(li);
                }
            }
            //没有斜
            if (noXpList.Count > 0)
            {
                result.AppendLine("\r\n下面是索取号中没有区分号/，有" + noXpList.Count + "条。");
                foreach (string li in noXpList)
                {
                    result.AppendLine(li);
                }
            }

            //有斜，但左或右没值
            if (hasXpNoValueList.Count > 0)
            {
                result.AppendLine("\r\n下面是分类号或区分号没值的，有" + hasXpNoValueList.Count + "条。");
                foreach (string li in hasXpNoValueList)
                {
                    result.AppendLine(li);
                }
            }

            if (hasKgList.Count > 0)
            {
                result.AppendLine("\r\n下面分类号或区分号中有空格的，有" + hasXpNoValueList.Count + "条。");
                foreach (string li in hasXpNoValueList)
                {
                    result.AppendLine(li);
                }
            }

            //左边错误
            if (leftWrongList.Count > 0)
            {
                result.AppendLine("\r\n下面是索取号中分类号错误的，有" + leftWrongList.Count + "条。");
                foreach (string li in leftWrongList)
                {
                    result.AppendLine(li);
                }
            }


            //右边错误
            if (rightWrongList.Count > 0)
            {
                result.AppendLine("\r\n下面是索取号中区分号错误的，有" + rightWrongList.Count + "条。");

                foreach (string li in rightWrongList)
                {
                    result.AppendLine(li);
                }
            }

            //其它
            if (otherList.Count > 0)
            {
                result.AppendLine("\r\n下面是其它索取号错误的，有" + otherList.Count + "条。");
                foreach (string li in otherList)
                {
                    result.AppendLine(li);
                }
            }

            // 整体输出，仅输出到文件
            this.OnlyOutput2File(result.ToString());    
            //this.OutputInfo(result.ToString(),true,false);

        }

        // 检查非种次号的索取号
        private void checkNoZch(object sender, EventArgs e)
        {
            //this.txtResult.Text = "";
            //this.textBox1_result2.Text = "";
            StringBuilder zcResult = new StringBuilder();
            StringBuilder otherResult = new StringBuilder();

            string[] lines = null;// this.GetLines();
            int i = 0;
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] subs = line.Split(new char[] { '\t' });

                string itemPath = subs[0];

                string accessNo = subs[1];

                // 如果输入的记录中有非种次号的，提示重新处理
                if (IsZch(accessNo) == true)
                {
                    zcResult.AppendLine(line);
                }
                else
                {
                    otherResult.AppendLine(line);
                }



                //this.toolStripStatusLabel_info.Text = (++i).ToString();
                Application.DoEvents();
            }



            //this.txtResult.Text = zcResult.ToString();
            //this.textBox1_result2.Text = otherResult.ToString();
        }

        /// <summary>
        /// 检查是否是种次号
        /// </summary>
        /// <param name="accessNo"></param>
        /// <returns></returns>
        bool IsZch(string accessNo)
        {
            if (string.IsNullOrEmpty(accessNo) == true)
                return false;

            int n = accessNo.LastIndexOf("/");
            if (n == -1) // 只有第一部分
                return false;

            if (n != -1)
            {
                string last = accessNo.Substring(n + 1);
                if (last.Length > 0 && (last[0] > '9' || last[0] < '0'))
                {
                    return false;
                }

                int nIndex = last.IndexOf(":");
                if (nIndex != -1)
                    last = last.Substring(0, nIndex);

                try
                {
                    int acc = Convert.ToInt32(last);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }

            }
            return false;
        }

        #endregion

        #endregion

        #region 统计读者

        // 统计读者单位
        private void checkDepartment()
        {
            this.OutputEmprty(2);
            this.OutputInfo("读者统计功能尚未实现",true,false);
            return;

            Hashtable keyvalues = new Hashtable();
            Hashtable pathList = new Hashtable();

            string[] lines = null;// this.GetLines();

            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string path = line;
                string key = "";

                int nIndex = line.IndexOf('\t');
                if (nIndex > 0)
                {
                    path = line.Substring(0, nIndex);
                    key = line.Substring(nIndex + 1);
                }

                int value = 0;
                if (keyvalues.ContainsKey(key) == true)
                {
                    value = (int)keyvalues[key];
                }

                value++;

                keyvalues[key] = value;
                pathList[key] = path;

            }

            string result = "";
            foreach (string key in keyvalues.Keys)
            {
                if (result != "")
                    result += "\r\n";

                result += key + "\t" + keyvalues[key] + "\t" + pathList[key];
            }
            //this.txtResult.Text = result;

            MessageBox.Show("ok");

        }

        #endregion

        #region 检查权限

        public static string[] danger_rights = new string[] {
        "batchtask",
        "clearalldbs",
        "devolvereaderinfo",
        "changeuser",
        "newuser",
        "deleteuser",
        "changeuserpassword",
        "simulatereader",
        "simulateworker",
        "setsystemparameter",
        "urgentrecover",
        "repairborrowinfo",
        "settlement",
        "undosettlement",
        "deletesettlement",
        "writerecord",
        "managedatabase",
        "restore",
        "managecache",
        "managechannel",
        "upload",
        "bindpatron",
        };

        // 检查帐号权限
        private void CheckRights()
        {
            this.OutputEmprty(2);
            this.OutputInfo("检查权限功能尚未实现",true,false);
            return;

            string result = "";
            string[] lines = null;// this.GetLines();
            foreach (string acc in lines)
            {
                if (acc == "")
                    continue;

                int nIndex = acc.IndexOf("~");
                if (nIndex == -1)
                    continue;

                string userName = acc.Substring(0, nIndex);
                string account = acc.Substring(nIndex + 1);


                string[] rightList = account.Split(',');

                string hasRights = "";
                int count = 0;

                foreach (string danger in danger_rights)
                {
                    if (rightList.Contains(danger) == true)
                    {
                        if (hasRights != "")
                        {
                            hasRights += ",";
                        }

                        hasRights += danger;
                        count++;
                    }
                }

                result += userName + "包含下列" + count + "个危险权限\n" + hasRights + "\r\n";

            }

            // this.txtResult.Text = result;

            MessageBox.Show("ok");
        }

        #endregion

        #region 获取流通权限
        public void GetCirculationRight()
        {
            string strValue = this.GetSystemParameter("circulation", "rightsTableHtml");

            // 显示在浏览器控件中
            WriteHtml(this.webBrowser1, strValue);
        }

        static void WriteHtml(WebBrowser webBrowser,
            string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
#if NO
                webBrowser.DocumentText = "<h1>hello</h1>";
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "");
#endif
            }

            // doc = doc.OpenNew(true);
            doc.Write("<br/>======<br/>");
            doc.Write(strHtml);
        }



        #endregion

        // 选择本地目录
        private void button_selectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult ret = dlg.ShowDialog();
            if (ret == DialogResult.OK)
            {
                this.textBox_dir.Text = dlg.SelectedPath;
            }
        }


        #region 统计册

        // 获取册
        private void button_entity_Click(object sender, EventArgs e)
        {
            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                SearchItem(this._cancel.Token);
            });
        }

        // 检查馆藏地
        private void button_checkLocation_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();

                // 空2行
                this.OutputEmprty(2);

                this.OutputInfo("==开始统计馆藏地==");
                this.GroupLocation(this._items, this._cancel.Token);
                this.OutputInfo("==结束统计馆藏地，详见excel==");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 校验图书类型
        private void button_checkBookType_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();

                // 空2行
                this.OutputEmprty(2);

                this.OutputInfo("==开始统计图书类型==");
                this.GroupType(this._items, this._cancel.Token);
                this.OutputInfo("==结束统计图书类型，详见excel==");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 校验索取号
        private void button_checkAccessNo_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();

                // 空2行
                this.OutputEmprty(2);

                // 校验索取号
                this.OutputInfo("==开始校验索取号==");
                Application.DoEvents();
                this.CheckAccessNo(this._items, this._cancel.Token);
                this.OutputInfo("==结束校验索取号，详见txt文件==");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 校验册价格
        private void button_checkPrice_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();

                // 空2行
                this.OutputEmprty(2);

                // 校验册价格
                this.OutputInfo("==开始校验册价格==");

                this.CheckPrice(this._items, this._cancel.Token);
                this.OutputInfo("==结束校验册价格，详见txt文件==");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 校验册条码
        private void button_verifyBarcode_Click(object sender, EventArgs e)
        {

            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                this.VerifyBarcode(this._items, this._cancel.Token);
            });
        }

        #endregion

        // 获取馆藏地
        private void button_getLocation_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                this.OutputInfo("==开始获取流通权限==");

                List<simpleLoc> locs = this.GetLocation();

                foreach (simpleLoc one in locs)
                {
                    this.OutputInfo(one.name+"\t"+one.canBorrow,true,false);
                }

                this.OutputInfo("==结束获取流通权限==");

                return;
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 检查排架体系
        private void button_paijia_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                this.OutputInfo("==开始检查排架体系==");

                // 调服务器接口
                string strValue = this.GetSystemParameter("circulation","callNumber");

                // 已配置了排系体系的馆藏地
                List<string> paijiaLocs = new List<string>();
                /*
                <group name="著者号排架" classType="中图法" qufenhaoType="GCAT" zhongcihaodb="" callNumberStyle="索取类号+区分号">
                <location name="星洲学校/图书馆" />
                <location name="星洲学校/阅览室" />
                <location name="星洲学校/班级书架" /><location name="星洲学校/走廊" /><location name="流通库" /></group><group name="种" classType="中图法" qufenhaoType="种次号" zhongcihaodb="" callNumberStyle=""><location name="第三中学/一年级" /><location name="第三中学/大阅读室" /><location name="第三中学/图书馆" />
                </group>
                 */
                string xml = "<root>" + strValue + "</root>";
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);
                XmlNodeList list = dom.DocumentElement.SelectNodes("group");
                foreach (XmlNode node in list)
                {
                    string name = DomUtil.GetAttr(node, "name");
                    string classType = DomUtil.GetAttr(node, "classType");
                    string qufenhaoType = DomUtil.GetAttr(node, "qufenhaoType");

                    this.OutputInfo("\r\n排架体系:"+ qufenhaoType + "  类号=" + classType+"  区分号="+qufenhaoType,
                        true,false);

                    XmlNodeList locs = node.SelectNodes("location");
                    foreach (XmlNode loc in locs)
                    { 
                        string locName=DomUtil.GetAttr(loc, "name");
                        this.OutputInfo(locName,true,false);
                        paijiaLocs.Add(locName);
                    }
                }


                // 把配置的馆藏地列出来，看看哪些没有定义排架
                bool bFirst = true;
                List<simpleLoc> simpleLocs = this.GetLocation();
                foreach(simpleLoc one in simpleLocs)
                {
                    if (paijiaLocs.IndexOf(one.name) == -1)
                    {
                        if (bFirst == true)
                        {
                            this.OutputInfo("\r\n以下馆藏地未配置排架方式，请图书馆老师确认是否需要配置。",
                                true,false);
                            bFirst = false;
                        }
                        this.OutputInfo(one.name,true,false);
                    }
                }

                this.OutputEmprty();
                this.OutputInfo("==结束检查排架体系==");

                return;
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 输出空行
        private void OutputEmprty(int count=1)
        {
            for (int i = 0; i < count; i++)
            {
                // 输出空格
                this.OutputInfo("", true, false);
            }
        }

        // 获取脚本
        private void button_script_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                this.OutputInfo("==开始获取条码校验规则==");

                //barcodeValidation
                string strValue = this.GetSystemParameter("circulation", "barcodeValidation");
                strValue = DomUtil.GetIndentXml("<barcodeValidation>"+strValue+ "</barcodeValidation>");

                if (string.IsNullOrEmpty(strValue) == false)
                {
                    this.OutputInfo("已配置新版本条码校验规则，如下:");
                    this.OutputInfo(strValue, true, false);
                }
                else
                {
                    this.OutputInfo("已配置新版本条码校验规则，如下:");
                }



                //script
                strValue = this.GetSystemParameter("circulation", "script");

                bool bHasFun = false;
                if (strValue.IndexOf("VerifyBarcode") != -1)
                {
                    this.OutputInfo("存在C#条码格式校验函数");
                    bHasFun = true;
                }

                if (strValue.IndexOf("ItemCanBorrow") != -1)
                {
                    this.OutputInfo("存在ItemCanBorrow函数");
                    bHasFun = true;
                }

                if (strValue.IndexOf("ItemCanReturn") != -1)
                {
                    this.OutputInfo("存在ItemCanReturn函数");
                    bHasFun = true;
                }

                /*
VerifyBarcode
ItemCanBorrow
ItemCanReturn
                 */
                if (bHasFun)
                {
                    // 输出空格
                    this.OutputEmprty();

                    this.OutputInfo(strValue, true, false);
                }

                this.OutputInfo("==结束获取条码校验规则==");

                return;
            }
            finally
            {
                EnableControls(true);
            }
        }


        // 获取系统参数
        public string GetSystemParameter(string strCategory,string strName)
        {
            RestChannel channel = this._mainForm.GetChannel();

            try
            {
                GetSystemParameterResponse response = channel.GetSystemParameter(strCategory,
                    strName);
                long lRet = response.GetSystemParameterResult.Value;
                string strError = response.GetSystemParameterResult.ErrorInfo;
                if (lRet == -1)
                {
                    throw new Exception("针对服务器 " + channel.Url + " 获得系统参数Category=["+strCategory+"]Name=["+strName+"]发生错误：" + strError);
                }

                return response.strValue;

            }
            finally
            {
                this._mainForm.ReturnChannel(channel);
            }
        }

        private void toolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._sw != null)
            {
                _sw.Close();
                _sw = null;
            }
        }
    }

    public class TypeGroup
    {
        // 册类型
        public string bookType { get; set; }

        public List<Entity> Items { get; set; }

        public int count { get; set; }

        public string Dump()
        {
            return bookType + "\t"
                + count;
        }

        // 是否在系统里定义过
        public bool isConfig { get; set; }
    }

    public class LocGroup
    {
        // 册类型
        public string location { get; set; }


        public List<Entity> Items { get; set; }

        public int count { get; set; }

        public string Dump()
        {
            return location + "\t"
                + count;
        }

        // 是否在系统里定义过
        public bool isConfig { get; set; }

    }

    // 册记录
    public class Entity
    {
        // 路径
        public string path { get; set; }

        // 册条码
        public string barcode { get; set; }

        // 馆藏地
        public string location { get; set; }

        // 索取号
        public string accessNo { get; set; }

        // 册类型
        public string bookType { get; set; }

        // 价格
        public string price { get; set; }

        public string errorInfo { get; set; }



    }

    public class db
    {
        //<database biblioDbName="中文图书" syntax="unimarc"
        // orderDbName="中文图书订购" commentDbName="中文图书评注"
        // inCirculation="true" role="catalogWork" itemDbName="中文图书实体"  issueDbName="中文期刊期" >
        public string biblioDbName { get; set; }

        public string itemDbName { get; set; }
        public string orderDbName { get; set; }
        public string commentDbName { get; set; }
        public string issueDbName { get; set; }
        public string syntax { get; set; }

        public string inCirculation { get; set; }
        public string role { get; set; }
    }

    public class simpleLoc
    {
        public string name { get; set; }

        public string canBorrow { get; set; }
    }

    }

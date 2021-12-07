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

        // 一键巡检
        private void button_oneKey_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 获取数据库信息
                this.button_listdb_Click(sender, e);

                // 获取流通权限
                this.button_circulationRight_Click(sender, e);
                //this.GetCirculationRight();

                // 册统计
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();
                //// 开一个新线程
                //Task.Run(() =>
                //{
                SearchItem(this._cancel.Token);
                //});

                // 校验册记录各种信息
                this.button_checkBookType_Click(sender, e);
                this.button_checkLocation_Click(sender, e);
                this.button_verifyBarcode_Click(sender, e);
                this.button_checkPrice_Click(sender, e);
                this.button_checkAccessNo_Click(sender, e);


                // 统计读者
                this.checkDepartment();

                // 检查权限
                this.CheckRights();
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
                this.OutputInfo("===\r\n开始获取书目信息:\r\n");
                this.GetDbInfos();
                this.OutputInfo("结束获取书目信息");
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
                this.OutputInfo("===\r\n开始获取流通权限:\r\n");
                this.GetCirculationRight();
                this.OutputInfo("结束获取流通权限");
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

        }

        #endregion


        #region 通用函数

        // 清信息
        private void button_clear_Click(object sender, EventArgs e)
        {
            this.textBox_info.Text = "";
        }

        public void OutputInfo(string info)
        {
            this.Invoke((Action)(() =>
            {
                this.textBox_info.Text += info + "\r\n";
            }
            ));
        }

        public void SetStateInfo(string info)
        {
            this.Invoke((Action)(() =>
            {
                //设置父窗口状态栏参数
                this._mainForm.SetStatusMessage(info);
            }
            ));
        }

        /// <summary>
        /// 设置控件是否可用
        /// </summary>
        /// <param name="bEnable"></param>
        void EnableControls(bool bEnable)
        {
            this.button_oneKey.Enabled = bEnable;
            this.button_stop.Enabled = bEnable?false:true;

            this.button_listdb.Enabled = bEnable;
            this.button_circulationRight.Enabled = bEnable;
            this.button_entity.Enabled = bEnable;
            this.button_patron.Enabled = bEnable;
            this.button_right.Enabled = bEnable;

            this.button_checkAccessNo.Enabled = bEnable;
            this.button_checkPrice.Enabled = bEnable;
            this.button_verifyBarcode.Enabled = bEnable;
            this.button_checkBookType.Enabled = bEnable;
            this.button_checkLocation.Enabled = bEnable;


            this.button_clear.Enabled = bEnable;   

        }


        #endregion


        #region 获取数据库信息

        // 获取数据库信息
        public void GetDbInfos()
        {
            this.OutputInfo("序号\t书目库名\t 数据量\t是否流通\t数据格式\t文献类型\t角色\t下级子库" + "\r\n");

            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                GetSystemParameterResponse response = channel.GetSystemParameter("system",
                    "biblioDbGroup");
                long lRet = response.GetSystemParameterResult.Value;
                string strError = response.GetSystemParameterResult.ErrorInfo;
                if (lRet == -1)
                {
                    this.OutputInfo("针对服务器 " + channel.Url + " 获得图书馆名称发生错误：" + strError);
                    return;
                }

                List<db> dbs = new List<db>();

                //<database biblioDbName="中文图书" syntax="unimarc"
                // orderDbName="中文图书订购" commentDbName="中文图书评注"
                // inCirculation="true" role="catalogWork" itemDbName="中文图书实体"  issueDbName="中文期刊期" >
                string xml = response.strValue;
                xml = "<root>" + xml + "</root>";
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);
                XmlNodeList list = dom.DocumentElement.SelectNodes("database");
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

                //string info = "";
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
                    lRet = channel.SearchBiblio(one.biblioDbName,
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
                        this.OutputInfo("获取数据库[" + one.biblioDbName + "]的书目记录数量进出错：" + strError);
                        continue;
                    }

                    string info = index + "\t"
                        + one.biblioDbName + "\t"
                        + lRet + "\t"
                        + one.inCirculation + "\t"
                        + one.syntax + "\t"
                        + type + "\t"
                        + one.role + "\t"
                        + one.itemDbName + "," + one.orderDbName + "," + one.commentDbName + "," + one.issueDbName
                        + "\r\n";

                    this.OutputInfo(info);

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
                    + "，其中有" + cirDbCount + "个库可流通，流通的书目种数为" + lCirCount + "。" + "\r\n");


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
                        this.OutputInfo("检索册记录出错：" + strError);
                        return;
                    }

                    // 输出命中信息
                    this.OutputInfo("共命中" + lRet.ToString() + "册");
                    this.SetStateInfo("共命中" + lRet.ToString() + "册");


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
                            this.OutputInfo("已获取" + lStart.ToString() + "条，获取结果集出错：" + strError);
                            return;
                        }

                        // 说明结果集里的记录获取完了。
                        if (lRet == 0)
                        {
                            break;
                        }

                        this.OutputInfo("已获取" + (lStart + searchresults.Length).ToString() + "条");

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
                            this.SetStateInfo("正在获取第" + (lStart + i).ToString());
                        }

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



                //完成提示
                this.SetStateInfo("获取数据完成");
                this.OutputInfo("获取数据完成");

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
            }
        }

        // 校验册条码

        public void VerifyBarcode(List<Entity> items, CancellationToken token)
        {
            string strError = "";

            List<Entity> barcodeErrorItems = new List<Entity>();
            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                int index = 0;
                foreach (Entity item in items)
                {
                    // 当外部让停止时，停止循环
                    token.ThrowIfCancellationRequested();

                    index++;
                    this.SetStateInfo("正在校验册价格" + index.ToString());

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
                        this.OutputInfo("校验册条码出错:" + strError);
                        return;
                    }
                    // 不合法
                    if (lRet == 0)
                    {
                        item.errorInfo = strError;
                        barcodeErrorItems.Add(item);
                    }
                    if (lRet == 1)
                    {
                        item.errorInfo = "与读者证规则冲突";
                        barcodeErrorItems.Add(item);
                    }

                }

            }
            finally
            {
                this._mainForm.ReturnChannel(channel);
            }

            // 输入册条码不合格的记录
            if (barcodeErrorItems.Count > 0)
            {
                this.OutputInfo("共有" + barcodeErrorItems.Count.ToString() + "条记录不符合册条码规则");

                foreach (Entity item in barcodeErrorItems)
                {
                    this.OutputInfo(item.path + "\t" + item.barcode + "\t" + item.errorInfo);
                }
            }

        }

        // 校验价格
        private void CheckPrice(List<Entity> items, CancellationToken token)
        {
            //this.toolStripStatusLabel_info.Text = "开始";

            //string[] lines = this.GetLines();
            string result = "";

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
                this.SetStateInfo("正在校验册价格" + index.ToString());



                //if (line == "")
                //    continue;

                string path = item.path;
                string price = item.price;
                if (price == null)
                    price = "";

                //this.toolStripStatusLabel_info.Text = "处理 " + line;
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

                    //大于500
                    try
                    {
                        double dPrice = Convert.ToDouble(right);
                        if (dPrice > 100)
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
            result += "==价格为空" + emptyList.Count + "条==";
            foreach (string li in emptyList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //
            result += "\r\n\r\n==价格超过500的" + largeList.Count + "条==";
            foreach (string li in largeList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //
            result += "\r\n\r\n==价格带括号的" + bracketList.Count + "条==";
            foreach (string li in bracketList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //
            result += "\r\n\r\n==其它不合规则的" + otherList.Count + "条==";
            foreach (string li in otherList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            this.OutputInfo(result);

            //MessageBox.Show("ok");
            //this.toolStripStatusLabel_info.Text = "结束";

        }


        // 对馆藏地进行聚合
        public void GroupLocation(List<Entity> items, CancellationToken token)
        {
            // 让用户选择需要统计的范围。根据批次号、目标位置来进行选择
            var list = items.GroupBy(
                x => new { x.location },
                (key, item_list) => new LocGroup
                {
                    location = key.location,
                    Items = new List<Entity>(item_list),
                    count = item_list.Count()
                }).OrderByDescending(o => o.Items.Count);


            foreach (LocGroup group in list)
            {
                this.OutputInfo(group.Dump());
            }
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


            foreach (TypeGroup group in list)
            {
                this.OutputInfo(group.Dump());
            }
        }

        #region 检查索取号

        // 检查索取号
        private void CheckAccessNo(List<Entity> items, CancellationToken token)
        {
            //this.toolStripStatusLabel_info.Text = "开始";

            //string[] lines = this.GetLines();
            string result = "";


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
                this.SetStateInfo("正在校验索取号" + index.ToString());

                //if (line == "")
                //    continue;

                string path = item.path;
                string accessNo = item.accessNo;
                if (accessNo == null)
                    accessNo = "";



                ////this.toolStripStatusLabel_info.Text = "处理 " + line;
                Application.DoEvents();


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
                    else
                    {

                        continue;
                    }

                }

                //
                otherList.Add(retLine);

            }



            //空索取号的
            result += "##索取号为空的" + emptyList.Count + "条";
            foreach (string li in emptyList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //没有斜
            result += "\r\n##没有斜的" + noXpList.Count + "条";
            foreach (string li in noXpList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //有斜，但左或右没值
            result += "\r\n##有斜，但左或右没值的" + hasXpNoValueList.Count + "条";
            foreach (string li in hasXpNoValueList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            //左边错误
            result += "\r\n##左边错误的" + leftWrongList.Count + "条";
            foreach (string li in leftWrongList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }


            //右边错误
            result += "\r\n##右边错误" + rightWrongList.Count + "条";
            foreach (string li in rightWrongList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            //其它
            result += "\r\n##其它" + otherList.Count + "条";
            foreach (string li in otherList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            this.OutputInfo(result);

            //MessageBox.Show("ok");
            //this.toolStripStatusLabel_info.Text = "结束";

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
            this.OutputInfo("读者统计功能尚未实现");
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
            this.OutputInfo("检查权限功能尚未实现");
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

            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                GetSystemParameterResponse response = channel.GetSystemParameter("circulation",
                    "rightsTableHtml");
                long lRet = response.GetSystemParameterResult.Value;
                string strError = response.GetSystemParameterResult.ErrorInfo;
                if (lRet == -1)
                {
                    MessageBox.Show(this, "针对服务器 " + channel.Url + " 获得图书馆名称发生错误：" + strError);
                    return;
                }

                // 显示在浏览器控件中
                string xml = response.strValue;
                WriteHtml(this.webBrowser1,xml);

                return;
            }
            finally
            {
                this._mainForm.ReturnChannel(channel);

            }
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

            //// 开一个新线程
            //Task.Run(() =>
            //{
                SearchItem(this._cancel.Token);
            //});
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

                // 聚合馆藏地
                this.SetStateInfo("开始统计馆藏地");

                this.OutputInfo("===\r\n开始统计册记录的馆藏地:\r\n");
                this.GroupLocation(this._items, this._cancel.Token);
                this.OutputInfo("结束统计馆藏地");
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

                // 聚合图书类型
                this.SetStateInfo("开始统计图书类型");

                this.OutputInfo("\r\n***\r\n开始统计图书类型:\r\n");
                this.GroupType(this._items, this._cancel.Token);
                this.OutputInfo("结束统计图书类型");
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

                // 校验索取号
                this.SetStateInfo("开始校验索取号");

                this.OutputInfo("\r\n***\r\n开始校验索取号:\r\n");
                this.CheckAccessNo(this._items, this._cancel.Token);
                this.OutputInfo("结束校验索取号");
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

                // 校验册价格
                this.SetStateInfo("正在校验册价格");

                this.OutputInfo("\r\n***\r\n开始校验册价格:\r\n");
                this.CheckPrice(this._items, this._cancel.Token);
                this.OutputInfo("结束校验册价格");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 校验册条码
        private void button_verifyBarcode_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();

                // 校验册条码
                this.SetStateInfo("正在校验索取号");

                this.OutputInfo("\r\n***\r\n开始校验索取号:");
                this.VerifyBarcode(this._items,this._cancel.Token);
                this.OutputInfo("结束校验索取号");
            }
            finally
            {
                EnableControls(true);
            }
        }

        #endregion
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



}

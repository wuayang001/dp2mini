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
        XLWorkbook _workbook = new XLWorkbook();

        /// <summary>
        /// 窗体装载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolForm_Load(object sender, EventArgs e)
        {
            this._mainForm = this.MdiParent as MainForm;

        }

        private void toolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._sw != null)
            {
                _sw.Close();
                _sw = null;
            }
        }



        #region 界面操作按钮

        public string Dir
        {
            get
            {
                string dir = this.textBox_dir.Text.Trim();
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
                    string fileName = this.Dir + "/" + this._mainForm.LibraryName + "-巡检结果.txt";
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

                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();


                // 获取数据库信息
                this.button_listdb_Click(sender, e);

                // 获取册,注意是直接调了函数
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

                // 获取校验函数
                this.button_script_Click(sender, e);

                // 校验册条码，注意是直接调了函数
                this.VerifyBarcode(this._items, this._cancel.Token);





                //==以下是读者相关

                // 读者库
                button_patron_Click(sender, e);

                // 获取读者，注意是直接调函数
                SearchReader(this._cancel.Token);

                // 读者类型
                button_readerType_Click(sender, e);

                // 读者单位
                button_department_Click(sender, e);

                // 校验读者条码
                button_patronBarcode_Click(sender, e);


                // 以下是流通相关====
                // 开馆日历
                this.button_calendar_Click(sender, e);

                // 获取流通权限
                this.button_circulationRight_Click(sender, e);


                // ==其它====

                // 下library
                this.button_downloadLibrary_Click(sender, e);

                // 查配置
                this.button_checkConfig_Click(sender, e);


                // 检查权限
                this.button_right_Click(sender, e);

                // 安全
                this.button_security_Click(sender, e);


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

        private string GetTimeString(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string GetInfoAddTime(string info, DateTime time)
        {
            return this.GetTimeString(time) + info;
        }

        // 列出书目库信息
        private void button_listdb_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始获取书目信息==", start));

                // 干活
                this.GetBiblioDbInfos();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束获取书目信息,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        public string GetSeconds(DateTime start, DateTime end)
        {
            double m = (end - start).TotalSeconds;
            return m.ToString("f2");
        }

        // 获取流通权限
        private void button_circulationRight_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始获取流通权限==", start));

                this.GetCirculationRight();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束获取流通权限,详见右侧,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 统计读者
        private void button_patron_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {

                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始获取读者库信息==", start));

                this.GetReaderDbInfos();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束获取读者库信息,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 检查权限
        private void button_right_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始检查权限==", start));



                // sheet名
                IXLWorksheet ws = this.CreateSheet("权限检查");
                // 设置标题
                string[] titles = {
                    "帐户",
                    "危险权限",
                    "备注"
                    };
                int i = 1;
                foreach (string s in titles)
                {
                    ws.Cell(1, i++).Value = s;
                }


                int index = 1;
                /*
    <accounts>
        <account name="reader" type="reader" rights="borrow,renew,reservation,order,changereaderpassword,getbibliosummary,searchcharging,listdbfroms,searchbiblio,getbiblioinfo,searchitem,getiteminfo,search,getsystemparameter,getres,searchissue,getissueinfo,searchorder,getorderinfo,getcommentinfo,setcommentinfo,searchcomment,getpatrontempid,getreaderinfo:info|borrows|overdues|reservations|outofReservations|refID|oi|readerType|cardNumber|state|gender|?name|comment|?tel|?barcode|?department,_wx_debug" libraryCode="" access="" comment="" binding="" />
                 */
                XmlNodeList list = this.LibraryDom.DocumentElement.SelectNodes("accounts/account");
                foreach (XmlNode node in list)
                {
                    string name = DomUtil.GetAttr(node, "name");
                    string rights = DomUtil.GetAttr(node, "rights");

                    // 这几个内部帐号过滤掉
                    if (name == "supervisor"
                        || name == "opac"
                        || name == "capo")
                    {
                        continue;
                    }

                    // 危险权限
                    Hashtable dangerRights = GetDangerRights();

                    bool bFirst = true;


                    // 检查本身配置的权限中有没有
                    string[] rightList = rights.Split(new char[] { ',' });
                    foreach (string right in rightList)
                    {
                        if (dangerRights.ContainsKey(right))
                        {
                            index++;

                            // 如果是第一个危险权限
                            if (bFirst == true)
                            {
                                ws.Cell(index, 1).Value = name;
                                bFirst = false;
                            }

                            ws.Cell(index, 2).Value = right;
                            ws.Cell(index, 3).Value = dangerRights[right];
                        }

                    }

                }

                // 设置excel格式
                this.SetExcelStyle(ws, index, titles.Length);

                //保存excel
                this._workbook.SaveAs(this.ExcelFileName);



                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束检查权限,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));

            }
            finally
            {
                EnableControls(true);
            }
        }

        Hashtable _dangerRights = null;

        public Hashtable GetDangerRights()
        {
            if (this._dangerRights == null)
            {
                this._dangerRights = new Hashtable();
                this._dangerRights.Add("batchtask", "批处理任务");
                this._dangerRights.Add("clearalldbs", "清除所有数据库");
                this._dangerRights.Add("devolvereaderinfo", "转移读者信息");
                this._dangerRights.Add("changeuser", "修改用户信息");
                this._dangerRights.Add("newuser", "创建新用户");
                this._dangerRights.Add("deleteuser", "删除用户");
                this._dangerRights.Add("changeuserpassword", "(强制)修改(其他)用户密码");
                this._dangerRights.Add("simulatereader", "允许模拟读者登录");
                this._dangerRights.Add("simulateworker", "允许模拟工作人员登录");
                this._dangerRights.Add("setsystemparameter", "设置系统参数");
                this._dangerRights.Add("urgentrecover", "应急恢复");
                this._dangerRights.Add("repairborrowinfo", "修复借阅信息");

                this._dangerRights.Add("settlement", "(费用)结算");
                this._dangerRights.Add("undosettlement", "撤销(费用)结算");
                this._dangerRights.Add("deletesettlement", "删除(费用)的结算记录");
                this._dangerRights.Add("writerecord", "直接写入数据库记录");
                this._dangerRights.Add("managedatabase", "管理数据库");

                this._dangerRights.Add("restore", "完整还原数据记录");
                this._dangerRights.Add("managecache", "管理缓存");
                this._dangerRights.Add("managechannel", "管理服务器通道");
                this._dangerRights.Add("upload", "上传文件");
                this._dangerRights.Add("bindpatron", "为读者绑定号码");

                this._dangerRights.Add("client_uimodifyorderrecord", "前端允许界面直接修改订购记录");
                this._dangerRights.Add("changereaderbarcode", "强制修改读者信息中的证条码号");
                this._dangerRights.Add("client_forceverifydata", "前端保存记录时是否强制校验数据。校验时如果发现数据有错，会拒绝保存");
                this._dangerRights.Add("client_deletebibliosubrecords", "前端删除书目记录时是否强制删除下属的实体、期、订购、评注记录。如果不具备此权限，则需要下属记录全部删除以后才能删除书目记录");


                this._dangerRights.Add("settailnumber", "设置种次号尾号");
                this._dangerRights.Add("setutilinfo", "设置实用库记录信息");

                this._dangerRights.Add("client_simulateborrow", "前端允许进行模拟借还操作");
                this._dangerRights.Add("client_multiplecharging", "前端允许进行复选借还操作");

            }

            return this._dangerRights;
        }

        #endregion


        #region 通用函数

        // 清信息
        private void button_clear_Click(object sender, EventArgs e)
        {
            this.textBox_info.Text = "";
        }

        public void OutputInfo(string info, bool isTextBox = true, bool isState = true)
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
            this.Invoke((Action)(() =>
            {
                this.button_oneKey.Enabled = bEnable;
                this.button_stop.Enabled = bEnable ? false : true;

                this.button_listdb.Enabled = bEnable;  // 书目
                this.button_entity.Enabled = bEnable; //获取册
                this.button_checkLocation.Enabled = bEnable; // 馆藏地
                this.button_checkBookType.Enabled = bEnable; //图书类型
                this.button_paijia.Enabled = bEnable;  //排架
                this.button_checkAccessNo.Enabled = bEnable;  //索取号
                this.button_script.Enabled = bEnable; //校验函数
                this.button_verifyBarcode.Enabled = bEnable;  //校验册条码
                this.button_checkPrice.Enabled = bEnable;  //册价格



                this.button_patron.Enabled = bEnable;  //读者库
                this.button_getReader.Enabled = bEnable;//获取读者
                this.button_readerType.Enabled = bEnable;//读者类型
                this.button_department.Enabled = bEnable;//读者部门
                this.button_patronBarcode.Enabled = bEnable;//读者条码

                this.button_calendar.Enabled = bEnable;// 开馆日历
                this.button_circulationRight.Enabled = bEnable;  //流通权限


                this.button_downloadLibrary.Enabled = bEnable;
                this.button_checkConfig.Enabled = bEnable;  //查配置
                this.button_right.Enabled = bEnable;  //权限
                this.button_security.Enabled = bEnable;//安全


                this.button_loc.Enabled = bEnable; //单馆藏地


                this.button_clear.Enabled = bEnable;     //清信息
            }
            ));
        }

        // 输出空行
        private void OutputEmprty(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                // 输出空格
                this.OutputInfo("", true, false);
            }
        }

        // 获取系统参数
        public string GetSystemParameter(string strCategory, string strName)
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
                    throw new Exception("针对服务器 " + channel.Url + " 获得系统参数Category=[" + strCategory + "]Name=[" + strName + "]发生错误：" + strError);
                }

                return response.strValue;

            }
            finally
            {
                this._mainForm.ReturnChannel(channel);
            }
        }




        #endregion


        #region 获取数据库信息

        // 获取数据库信息
        public void GetBiblioDbInfos()
        {
            string strError = "";

            // sheet名：书目库
            IXLWorksheet ws = this.CreateSheet("书目库");
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
                            true, false);
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
                        ws.Cell(index + 1, i++).Value = s;
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
                    true, false);


                // 设置excel格式
                this.SetExcelStyle(ws, index + 1, titles.Length);

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
            this._items.Clear(); // 先清空本地缓存的册记录


            // 空2行
            this.OutputEmprty(2);

            // 开始时间
            DateTime start = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==开始获取册信息==", start));
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
                        this.OutputInfo("检索册记录出错：" + strError, true, false);
                        return;
                    }

                    // 输出命中信息
                    this.OutputInfo("dp2系统中册数量总计" + lRet.ToString() + "册。", true, false);
                    Application.DoEvents();
                    //this.OutputInfo("共命中" + lRet.ToString() + "册");


                    long lHitCount = lRet;
                    long lStart = 0;
                    long lCount = lHitCount;

                    // 从结果集中取出册记录
                    for (; ; )
                    {
                        Application.DoEvents(); // 出让界面控制权

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
                                true, false);
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
                            Application.DoEvents(); // 出让界面控制权
                            token.ThrowIfCancellationRequested();


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
                            this.OutputInfo("获取册记录第" + (lStart + i).ToString(), false, true);
                        }

                        this.OutputInfo("已获取" + (lStart + searchresults.Length).ToString() + "条");


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


            }


            // 结束时间
            DateTime end = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==结束获取册信息,用时" + this.GetSeconds(start, end) + "秒==", end));
        }

        // 校验册条码

        public void VerifyBarcode(List<Entity> items, CancellationToken token)
        {
            // 空2行
            this.OutputEmprty(2);

            // 开始时间
            DateTime start = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==开始校验册条码==", start));

            EnableControls(false);
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
                        Application.DoEvents();

                        index++;
                        this.OutputInfo("校验册条码第" + index.ToString(), false, true);

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
                            this.OutputInfo("校验册条码出错:" + strError, true, false);
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

                EnableControls(true);
            }

            // 结束时间
            DateTime end = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==结束校验册条码,详见txt文件,用时" + this.GetSeconds(start, end) + "秒==", end));
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
                this.OutputInfo("校验册价格第" + index.ToString(), false, true);

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
        private BookTypeGroup GetTypeGroup(List<BookTypeGroup> list, string name)
        {
            foreach (BookTypeGroup group in list)
            {
                if (group.bookType == name)
                {
                    return group;
                }
            }
            return null;
        }

        private PatronGroup GetPatronGroup(List<PatronGroup> list, string name)
        {
            foreach (PatronGroup group in list)
            {
                if (group.name == name)
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
                ws.Cell(index + 1, 1).Value = one.name;
                ws.Cell(index + 1, 2).Value = one.canBorrow;
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
            this.SetExcelStyle(ws, index + 1, titles.Length);

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
            bool bExist = this._workbook.Worksheets.TryGetWorksheet(sheetName, out ws);
            if (bExist == true)
                this._workbook.Worksheets.Delete(sheetName);
            ws = this._workbook.Worksheets.Add(sheetName);

            return ws;
        }

        public void SetExcelStyle(IXLWorksheet ws, int rowCount, int colCount)
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
                (key, item_list) => new BookTypeGroup
                {
                    bookType = key.bookType,
                    Items = new List<Entity>(item_list),
                    count = item_list.Count()
                }).OrderByDescending(o => o.Items.Count);



            List<BookTypeGroup> types = new List<BookTypeGroup>();
            foreach (BookTypeGroup group in list)
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

                BookTypeGroup group = this.GetTypeGroup(types, one);
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
            foreach (BookTypeGroup one in types)
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
                XmlNodeList nodeList = dom.DocumentElement.SelectNodes(type + "/item");
                foreach (XmlNode node in nodeList)
                {
                    string value = node.InnerText.Trim();
                    if (string.IsNullOrEmpty(value) == false)
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
                            types.Add(library + "/" + value);
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
                this.OutputInfo("校验索取号第" + index.ToString(), false, true);

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

            StringBuilder result = new StringBuilder();


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

        public List<db> GetReaderDbs()
        {
            this._cirReaderDbs = "";

            // 获取读者库
            string strValue = this.GetSystemParameter("system", "readerDbGroup");
            //<database name="读者" inCirculation="true" libraryCode="" />

            string xml = strValue;
            xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNodeList list = dom.DocumentElement.SelectNodes("database");
            List<db> dbs = new List<db>(); //未定义新类，直接使用的bibliodb这个类
            foreach (XmlNode node in list)
            {
                Application.DoEvents();

                db one = new db();
                one.biblioDbName = DomUtil.GetAttr(node, "name");
                one.inCirculation = DomUtil.GetAttr(node, "inCirculation");
                one.libraryCode = DomUtil.GetAttr(node, "libraryCode");
                dbs.Add(one);

                // 把流通库单独放置
                if (one.inCirculation == "true")
                {
                    if (string.IsNullOrEmpty(this._cirReaderDbs) == false)
                    {
                        this._cirReaderDbs += ",";
                    }
                    this._cirReaderDbs += one.biblioDbName;
                }
            }

            return dbs;
        }

        // 获取读者数据库信息
        public void GetReaderDbInfos()
        {
            string strError = "";

            // sheet名：读者库
            IXLWorksheet ws = this.CreateSheet("读者库");
            // 设置标题
            string[] titles = {
            "读者库名",
            "参考流通",
            "馆代码",
            "数据量"
            };
            int i = 1;
            foreach (string s in titles)
            {
                ws.Cell(1, i++).Value = s;
            }


            // 获取全部读者库
            List<db> dbs = this.GetReaderDbs();


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

                    //string output = "";
                    long lRet = channel.SearchReader(one.biblioDbName,
                       "",
                       -1,
                       "__id",
                       "left",
                       "zh",
                       "test",
                       "",
                       out strError);
                    if (lRet == -1)
                    {
                        this.OutputInfo("获取读者库[" + one.biblioDbName + "]的记录数量出错：" + strError,
                            true, false);
                        continue;
                    }

                    // 每行内容
                    string[] row = {
             one.biblioDbName,
                        one.inCirculation.ToString(),
             one.libraryCode,
                       lRet.ToString() };


                    // 写excel
                    i = 1;
                    foreach (string s in row)
                    {
                        ws.Cell(index + 1, i++).Value = s;
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

                this.OutputInfo("\r\n图书馆系统中总共有" + index + "个读者库，读者记录总数量为" + lTotalCount
                    + "，其中有" + cirDbCount + "个库参与流通，参与流通的读者记录数量为" + lCirCount + "。" + "\r\n",
                    true, false);


                // 设置excel格式
                this.SetExcelStyle(ws, index + 1, titles.Length);

                //保存excel
                this._workbook.SaveAs(this.ExcelFileName);

                return;
            }
            finally
            {
                this._mainForm.ReturnChannel(channel);
            }

        }

        List<Patron> _patrons = new List<Patron>();
        public string _cirReaderDbs = "";
        public void SearchReader(CancellationToken token)
        {
            this._patrons.Clear(); // 先清空本地缓存的读者记录

            // 空2行
            this.OutputEmprty(2);

            // 开始时间
            DateTime start = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==开始获取读者记录==", start));
            Application.DoEvents();

            string strError = "";
            long lRet = 0;


            // 设置按钮状态
            EnableControls(false);



            // 检查读者信息，只检查流通的读者
            if (string.IsNullOrEmpty(this._cirReaderDbs) == true)
            {
                // 如果流通库没有，则自动从服务器找一下
                this.GetReaderDbs();
            }

            // 如果没有参与流通的数据库，则直接退出
            if (string.IsNullOrEmpty(this._cirReaderDbs) == true)
            {
                this.OutputInfo("没有参与流通的读者库", true, false);
                return;
            }


            // 获取所有册记录，保存到本地数组
            //List<Entity> items = new List<Entity>();
            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                token.ThrowIfCancellationRequested();

                // 检查全部册
                lRet = channel.SearchReader(//stop,
                   this._cirReaderDbs,
                   "", // 
                   -1,
                   "__id",
                   "left",
                   "zh",
                   "myresult",
                   "id,xml",
                   out strError);
                if (lRet == -1)
                {
                    this.OutputInfo("检索读者记录出错：" + strError, true, false);
                    return;
                }

                // 输出命中信息
                this.OutputInfo("系统中参与流通的读者数量为" + lRet.ToString() + "条。", true, false);
                Application.DoEvents();
                //this.OutputInfo("共命中" + lRet.ToString() + "册");


                long lHitCount = lRet;
                long lStart = 0;
                long lCount = lHitCount;

                // 从结果集中取出册记录
                for (; ; )
                {
                    Application.DoEvents(); // 出让界面控制权

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
                            true, false);
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
                        Application.DoEvents(); // 出让界面控制权
                        token.ThrowIfCancellationRequested();

                        Record record = searchresults[i];

                        Patron patron = new Patron();
                        patron.path = record.Path;
                        this._patrons.Add(patron);

                        /*
- <root>
<barcode>XZP00002</barcode> 
<name>朱婧媛</name> 
<readerType>学生</readerType> 
<department>2018级初中一班</department> 
<refID>34baad2b-53cc-4e7d-8e77-5078eccb03dd</refID> 
<libraryCode>星洲学校</libraryCode> 
<hire expireDate="" period="" /> 
<borrows>
  <borrow barcode="DPB000001" oi="CN-320506-C-XZXX" recPath="中文图书实体/24" biblioRecPath="中文图书/51" location="星洲学校/图书馆" borrowDate="Thu, 04 Nov 2021 13:54:07 +0800" borrowPeriod="10day" borrowID="e1af938d-d35d-4742-ba01-f639f7181131" returningDate="Sun, 14 Nov 2021 12:00:00 +0800" operator="supervisor" type="教材" volume="123" price="CNY28.80" notifyHistory="ynyyny" /> 
</borrows>
 */
                        string xml = record.RecordBody.Xml;
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(xml);
                        XmlNode root = dom.DocumentElement;
                        patron.libraryCode = DomUtil.GetElementInnerText(root, "libraryCode");
                        patron.barcode = DomUtil.GetElementInnerText(root, "barcode");
                        patron.name = DomUtil.GetElementInnerText(root, "name");
                        patron.readerType = DomUtil.GetElementInnerText(root, "readerType");
                        if (string.IsNullOrEmpty(patron.libraryCode) == false)
                        {
                            patron.readerType = patron.libraryCode + "/" + patron.readerType;
                        }


                        patron.department = DomUtil.GetElementInnerText(root, "department");
                        // 统计一下空格
                        if (patron.department == null)
                            patron.department = "";
                        patron.department = patron.department.Trim();


                        XmlNodeList borrowList = root.SelectNodes("borrows/borrow");
                        patron.borrowCount = borrowList.Count;

                        // info
                        this.OutputInfo("获取读者第" + (lStart + i).ToString(), false, true);
                    }

                    this.OutputInfo("已获取" + (lStart + searchresults.Length).ToString() + "条");


                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }


                // 输出有在借册的读者
                // sheet名：书目库
                IXLWorksheet ws = this.CreateSheet("在借书读者");
                // 设置标题
                string[] titles = {
            "读者证条码号",
            "读者姓名",
            "在借数量"
            };
                int j = 1;
                foreach (string s in titles)
                {
                    ws.Cell(1, j++).Value = s;
                }

                // 循环读者
                int index = 0;
                foreach (Patron one in this._patrons)
                {
                    if (one.borrowCount > 0)
                    {
                        index++;

                        // 写excel
                        ws.Cell(index + 1, 1).Value = one.barcode;
                        ws.Cell(index + 1, 2).Value = one.name;
                        ws.Cell(index + 1, 3).Value = one.borrowCount;
                    }
                }

                // 输出一句
                this.OutputInfo("有在借图书的读者数量为" + index + "条。", true, false);

                // 设置excel格式
                this.SetExcelStyle(ws, index + 1, titles.Length);

                //将第1列读者证条码设为文本格式
                ws.Column(1).Style.NumberFormat.Format = "@";

                //保存excel
                this._workbook.SaveAs(this.ExcelFileName);

            }
            finally
            {
                this._mainForm.ReturnChannel(channel);

                EnableControls(true);
            }


            // 结束时间
            DateTime end = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==结束获取读者记录,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));


            // todo，以后可以改进，保存下来
        }

        // 获取读者
        private void button_getReader_Click(object sender, EventArgs e)
        {
            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                SearchReader(this._cancel.Token);
            });
        }

        // 统计读者类型
        private void button_readerType_Click(object sender, EventArgs e)
        {

            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始统计读者类型==", start));

                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();
                this.GroupReaderType(this._patrons, this._cancel.Token);


                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束统计读者类型,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }

        }

        // 对读者类型进行聚合
        public void GroupReaderType(List<Patron> patrons, CancellationToken token)
        {
            // 让用户选择需要统计的范围。根据批次号、目标位置来进行选择
            var list = patrons.GroupBy(
                x => new { x.readerType },
                (key, item_list) => new PatronGroup
                {
                    name = key.readerType,
                    Items = new List<Patron>(item_list),
                    count = item_list.Count()
                }).OrderByDescending(o => o.count);



            List<PatronGroup> types = new List<PatronGroup>();
            foreach (PatronGroup group in list)
            {
                if (String.IsNullOrEmpty(group.name) == true)
                    group.name = "[空]";

                types.Add(group);
            }


            // sheet名：馆藏地
            IXLWorksheet ws = this.CreateSheet("读者类型");

            // 设置标题
            string[] titles = {
            "读者类型",
            "读者数量",
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

            // 获取配置的读者类型
            int index = 0;
            List<string> bookTypes = this.GetCirTypes("readerTypes");
            foreach (string one in bookTypes)
            {
                index++;

                PatronGroup group = this.GetPatronGroup(types, one);
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
            foreach (PatronGroup one in types)
            {
                if (one.isConfig == false)
                {
                    index++;

                    // 已输出excel，则不输出的文本了
                    //this.OutputInfo(one.bookType+  "\t" +one.count+"\t" + "未定义", true, false);

                    // 写excel
                    ws.Cell(index + 1, 1).Value = one.name;
                    ws.Cell(index + 1, 2).Value = one.count.ToString();
                    ws.Cell(index + 1, 3).Value = "未定义";
                }
            }

            // 设置excel格式
            this.SetExcelStyle(ws, index + 1, titles.Length);

            //保存excel
            this._workbook.SaveAs(this.ExcelFileName);
        }

        // 对读者部门进行聚合
        public void GroupReaderDept(List<Patron> patrons, CancellationToken token)
        {
            // 让用户选择需要统计的范围。根据批次号、目标位置来进行选择
            var list = patrons.GroupBy(
                x => new { x.department },
                (key, item_list) => new PatronGroup
                {
                    name = key.department,
                    Items = new List<Patron>(item_list),
                    count = item_list.Count()
                }).OrderByDescending(o => o.count);



            List<PatronGroup> types = new List<PatronGroup>();

            foreach (PatronGroup group in list)
            {
                if (group.name == "")
                    group.name = "[空]";
                types.Add(group);
            }


            // sheet名：馆藏地
            IXLWorksheet ws = this.CreateSheet("读者部门");
            // 设置标题
            string[] titles = {
            "读者部门",
            "读者数量",
            "备注"
            };
            int i = 1;
            foreach (string s in titles)
            {
                ws.Cell(1, i++).Value = s;
            }

            bool bHasEmpty = false;
            // 输出读者部门到excel
            int index = 0;
            // 把没有数据的馆藏地显示在下方
            foreach (PatronGroup one in types)
            {
                index++;

                // 写excel
                ws.Cell(index + 1, 1).Value = one.name;
                ws.Cell(index + 1, 2).Value = one.count.ToString();

                // 如果部门为空，将读者路径和证条码输出到txt
                if (one.name == "[空]")
                {
                    ws.Cell(index + 1, 3).Value = "详见下方excel";
                    bHasEmpty = true;
                }

            }

            // 设置excel格式
            this.SetExcelStyle(ws, index + 1, titles.Length);

            //保存excel
            this._workbook.SaveAs(this.ExcelFileName);


            // 将读者单位的为空的另行输出excel
            if (bHasEmpty == true)
            {
                // sheet名：馆藏地
                ws = this.CreateSheet("部门为空的读者");
                // 设置标题
                string[] titles1 = {
            "路径",
            "证条码",
            "姓名"
            };
                i = 1;
                foreach (string s in titles1)
                {
                    ws.Cell(1, i++).Value = s;
                }
                index = 0;
                // 把没有数据的馆藏地显示在下方
                foreach (PatronGroup one in types)
                {
                    if (one.name != "[空]")
                        continue;
                    this.OutputInfo("读者部门为[空]的，有" + one.Items.Count.ToString() + "条。");
                    foreach (Patron a in one.Items)
                    {
                        index++;
                        ws.Cell(index + 1, 1).Value = a.path;
                        ws.Cell(index + 1, 2).Value = a.barcode;
                        ws.Cell(index + 1, 3).Value = a.name;
                    }
                }

                // 设置excel格式
                this.SetExcelStyle(ws, index + 1, titles.Length);

                //将第1列读者证条码设为文本格式
                ws.Column(2).Style.NumberFormat.Format = "@";

                //保存excel
                this._workbook.SaveAs(this.ExcelFileName);
            }

        }

        /*
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
        */

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
                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始统计馆藏地==", start));

                // 干活
                this._cancel.Dispose(); // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel = new CancellationTokenSource();
                this.GroupLocation(this._items, this._cancel.Token);

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束统计馆藏地,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
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
                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始统计图书类型==", start));

                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();
                this.GroupType(this._items, this._cancel.Token);


                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束统计图书类型,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
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


                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始校验索取号==", start));
                Application.DoEvents();


                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();
                this.CheckAccessNo(this._items, this._cancel.Token);


                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束校验索取号,详见txt文件,用时" + this.GetSeconds(start, end) + "秒==", end));
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

                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始校验册价格==", start));

                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();
                this.CheckPrice(this._items, this._cancel.Token);

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束校验册价格,详见txt文件,用时" + this.GetSeconds(start, end) + "秒==", end));
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
                    this.OutputInfo(one.name + "\t" + one.canBorrow, true, false);
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


                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始检查排架体系==", start));

                // 调服务器接口
                string strValue = this.GetSystemParameter("circulation", "callNumber");

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

                    this.OutputInfo("\r\n排架体系:" + qufenhaoType + "  类号=" + classType + "  区分号=" + qufenhaoType,
                        true, false);

                    XmlNodeList locs = node.SelectNodes("location");
                    foreach (XmlNode loc in locs)
                    {
                        string locName = DomUtil.GetAttr(loc, "name");
                        this.OutputInfo(locName, true, false);
                        paijiaLocs.Add(locName);
                    }
                }


                // 把配置的馆藏地列出来，看看哪些没有定义排架
                bool bFirst = true;
                List<simpleLoc> simpleLocs = this.GetLocation();
                foreach (simpleLoc one in simpleLocs)
                {
                    if (paijiaLocs.IndexOf(one.name) == -1)
                    {
                        if (bFirst == true)
                        {
                            this.OutputInfo("\r\n以下馆藏地未配置排架方式，请图书馆老师确认是否需要配置。",
                                true, false);
                            bFirst = false;
                        }
                        this.OutputInfo(one.name, true, false);
                    }
                }

                // 输出空行
                this.OutputEmprty();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束检查排架体系,用时" + this.GetSeconds(start, end) + "秒==", end));

                return;
            }
            finally
            {
                EnableControls(true);
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

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始获取条码校验规则==", start));

                //barcodeValidation
                string strValue = this.GetSystemParameter("circulation", "barcodeValidation");
                strValue = DomUtil.GetIndentXml("<barcodeValidation>" + strValue + "</barcodeValidation>");

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
                if (strValue.IndexOf("VerifyBarcode") != -1)
                {
                    this.OutputInfo("存在C#条码校验函数，建议改为新的配置方式");
                    // 输出空格
                    this.OutputEmprty();
                    this.OutputInfo(strValue, true, false);
                }



                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束获取条码校验规则,用时" + this.GetSeconds(start, end) + "秒==", end));

                return;
            }
            finally
            {
                EnableControls(true);
            }
        }








        #endregion

        private void button_checkConfig_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始检查配置==", start));

                // 干活
                this.GetConfig();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束检查配置,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        public void GetConfig()
        {

            XmlNode root = this.LibraryDom.DocumentElement;

            // 脚本函数
            //ItemCanBorrow 无
            //ItemCanReturn 无
            // 说明：这两个函数是dp2老版本采用的方式，dp2 V3版本一般在馆藏地设置是否允许借还
            /*
    <script><![CDATA[
using System;
using System.Xml;
             */
            this.OutputEmprty();
            this.OutputInfo("==脚本函数==");
            string script = DomUtil.GetElementInnerText(root, "script");
            bool hasItemCanBorrow = false;
            bool hasItemCanReturn = false;
            if (string.IsNullOrEmpty(script) == false)
            {
                if (script.IndexOf("ItemCanBorrow") != -1)
                {
                    this.OutputInfo("存在ItemCanBorrow函数");
                    hasItemCanBorrow = true;
                }
                else
                {
                    this.OutputInfo("不存在ItemCanBorrow函数");
                }

                if (script.IndexOf("ItemCanReturn") != -1)
                {
                    this.OutputInfo("存在ItemCanReturn函数");
                    hasItemCanReturn = true;
                }
                else
                {
                    this.OutputInfo("存在ItemCanReturn函数");
                }
            }
            this.OutputInfo("\r\n功能说明：这两个函数是dp2老版本采用的C#函数方式，用来管理什么情况下允许借还。dp2 V3版本一般在馆藏地设置是否允许借还。如果存在这两个函数，建议改为新的配置方式。");

            /*
检查结果：
不存在ItemCanBorrow和ItemCanReturn函数，配置良好。
存在ItemCanBorrow或者ItemCanReturn函数，需改为新的配置方式。
             */
            if (hasItemCanBorrow == true || hasItemCanReturn == true)
            {
                this.OutputInfo("\r\n检查结果：存在ItemCanBorrow或者ItemCanReturn函数，需改为新的配置方式。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：不存在ItemCanBorrow和ItemCanReturn函数，配置良好。");
            }


            //值列表
            //内容
            //说明：检查在这里设置了馆藏地
            /*
<valueTables>
        <table name="location" dbname="">基藏库,流通库,阅览室</table>
        <table name="state" dbname="">挂失,修补,装订,剔旧,其他</table>
        <table name="bookType" dbname="">普通图书,文学,科技,社科</table>
        <table name="readerType" dbname="">普通读者,本科生,硕士生,教师,博士生,教授</table>
        <table name="readerState" dbname="">挂失,暂停,违章,注销(退证),其他</table>
        <table name="hirePeriod" dbname="">一月,半年,一年</table>
        <table name="issueState" dbname="">预测,记到</table>
        <table name="orderSeller" dbname="">新华书店,中图公司</table>
        <table name="orderSource" dbname="">本馆经费,世行贷款</table>
        <table name="orderState" dbname="">已订购,已验收</table>
        <table name="orderClass" dbname="">社科,自科</table>
</valueTables>

             */
            // 值列表
            this.OutputEmprty();
            this.OutputInfo("==值列表==");
            string valueTables = "";
            XmlNode node = root.SelectSingleNode("valueTables");
            bool hasVt = false;
            if (node != null)
            {
                valueTables = node.OuterXml;

                //<valueTables>里的<table name=”readerType”>和<table name=”bookType”>也定义了一个读者类型和册类型的值列表

                // 检查有没有读者类型
                XmlNodeList nodeList = node.SelectNodes("table[@name='readerType']");
                if (nodeList.Count > 0)
                    hasVt = true;

                // 检查有没有图书类型
                nodeList = node.SelectNodes("table[@name='bookType']");
                if (nodeList.Count > 0)
                    hasVt = true;

                // 检查有没有馆藏地
                nodeList = node.SelectNodes("table[@name='location']");
                if (nodeList.Count > 0)
                    hasVt = true;

            }
            if (string.IsNullOrEmpty(valueTables) == false)
            {
                valueTables = DomUtil.GetIndentXml(valueTables);
                this.OutputInfo(valueTables);
            }
            else
            {
                this.OutputInfo("未配置值列表");
            }


            /*
功能说明：值列表主要用来定义订购的'渠道'、'经费来源'、'类别'、数据状态信息，但不要在值列表中定义'馆藏地'、'图书类型'、'读者类型'，如果存在，则需改为在对应的界面进行配置。

\r\n检查结果：
值列表中不存在'馆藏地'、'图书类型'、'读者类型'的配置，配置良好。
值列表中存在'馆藏地'、'图书类型'、'读者类型'其中一项或多项配置，需改进，请在对应的界面进行配置。
            尚未配置值列表，建议配置一些选择信息，方便前端选择。
             */
            this.OutputInfo("\r\n功能说明：值列表主要用来定义订购的'渠道'、'经费来源'、'类别'，数据状态信息，但不要在值列表中定义'馆藏地'、'图书类型'、'读者类型'，如果存在，则需改为在对应的界面进行配置。");
            //
            if (string.IsNullOrEmpty(valueTables) == true)
            {
                this.OutputInfo("\r\n检查结果：尚未配置值列表，建议配置一些选择信息，方便前端选择。");
            }
            else
            {
                if (hasVt == false)
                {
                    this.OutputInfo("\r\n检查结果：值列表配置良好，不存在'馆藏地'、'图书类型'、'读者类型'的配置。");
                }
                else
                {
                    this.OutputInfo("\r\n检查结果：值列表中存在'馆藏地'、'图书类型'、'读者类型'其中一项或多项配置，需改进，请在对应的界面进行配置。");
                }
            }


            // 查重空间
            // 内容
            // 说明：需配置查重空间
            /*
     <unique>
         <space dbnames="中文图书,英文图书" />
     </unique>
             */
            this.OutputEmprty();
            this.OutputInfo("==查重空间==");
            string unique = "";
            XmlNode space = null;
            bool hasUnique = false;
            node = root.SelectSingleNode("unique");
            if (node != null)
            {
                unique = node.OuterXml;

                space = node.SelectSingleNode("space");
            }
            if (string.IsNullOrEmpty(unique) == false)
            {
                unique = DomUtil.GetIndentXml(unique).Trim();
                this.OutputInfo(unique);

                // 检查有没有实际的spac
                if (space != null)
                {
                    hasUnique = true;
                }
            }
            else
            {
                this.OutputInfo("未配置查重空间");
            }
            /*
功能说明：查重空间用来定义多个库的书目记录不重复，如果没有配置，需与馆员老师沟通，进行配置。

检查结果：
已配置查重空间，配置良好。
未配置查看空间，需改进，进行配置。
             */
            this.OutputInfo("\r\n功能说明：查重空间用来定义多个库的书目记录不重复，如果没有配置，需与馆员老师沟通，进行配置。");
            if (hasUnique == true)
            {
                this.OutputInfo("\r\n检查结果：已配置查重空间，配置良好。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：未配置查看空间，需改进，进行配置。");
            }


            // RFID机构代码
            // 内容
            // 说明：如果已上线RFID，但没配置机构代码，属异常
            /*
    <rfid>
        <ownerInstitution>
            <item map="/" isil="CN-120103-C-SYZX" />
        </ownerInstitution>
    </rfid>
             */
            //string rfid = this.GetSystemParameter("system","rfid");
            //rfid=DomUtil.GetIndentXml(rfid);
            //this.OutputInfo(rfid);
            this.OutputEmprty();
            this.OutputInfo("==RFID机构代码==");
            XmlNode itemNode = null;
            bool hasRfid = false;
            string rfid = "";
            node = root.SelectSingleNode("rfid");
            if (node != null)
            {
                rfid = node.OuterXml;
                itemNode = node.SelectSingleNode("ownerInstitution/item");
            }
            if (string.IsNullOrEmpty(rfid) == false)
            {
                rfid = DomUtil.GetIndentXml(rfid);
                this.OutputInfo(rfid);
                if (itemNode != null)
                    hasRfid = true;
            }
            else
            {
                this.OutputInfo("未配置RFID机构代码");
            }
            /*
             * 功能说明：机构代码表示图书馆图书资产和读者所属的机构，如果已上线RFID智能图书馆功能，需配置机构代码，如未配置属异常情况。

            检查结果：
            已配置RFID机构代码，配置良好。
            未配置RFID机构代码，需和用户单位确认，是否已使用RFID功能，如已使用，一定要配置上；如未使用RFID，也建议尽早配置上。
             */
            this.OutputInfo("\r\n功能说明：机构代码表示图书馆图书资产和读者所属的机构，如果已上线RFID智能图书馆功能，需配置机构代码，如未配置属异常情况。");
            if (hasRfid == true)
            {
                this.OutputInfo("\r\n检查结果：已配置RFID机构代码，需确认是否符合RFID规范。");
            }
            else
            {
                this.OutputInfo("\r\n未配置RFID机构代码，需和用户单位确认，是否已使用RFID功能，如已使用，一定要配置上；如未使用RFID，也建议尽早配置上。");
            }

            //mongoDB数据库配置
            // 说明：如果不配置这项，没有4个库
            /*
<mongoDB connectionString="mongodb://127.0.0.1" instancePrefix="demo" />
             */
            this.OutputEmprty();
            this.OutputInfo("==MongoDB数据库配置==");
            string mongoDB = "";
            node = root.SelectSingleNode("mongoDB");
            if (node != null)
            {
                mongoDB = node.OuterXml;
            }
            if (string.IsNullOrEmpty(mongoDB) == false)
            {
                this.OutputInfo(mongoDB);
            }
            else
            {
                this.OutputInfo("未配置MongoDB数据库参数");
            }
            /*
             * 功能说明：mongoDB数据库参数，是dp2系统扩展的功能，用来存储'全量借阅历史'、'书目摘要'、'访问计数'、'访问日志'这4个库，如果未配置，不影响系统使用，但没法查早前的借阅历史，“书目摘要”用来提高显示书目摘要的速度，'访问计数'用来存储OPAC访问计数和数字资源浏览次数，'访问日志'用来存储数字资源访问日志记录。

检查结果：
已配置mongoDB数据库参数，配置良好。
未配置mongoDB数据库参数，缺少'出纳历史'、'书目摘要'、'访问计数'、'访问日志'这4个库，建议配置上。

             */
            this.OutputInfo("\r\n功能说明：MongoDB数据库参数，是dp2系统扩展的功能，用来存储'全量借阅历史'、'书目摘要'、'访问计数'、'访问日志'这4个库，如果未配置，不影响系统使用，但没法查早前的借阅历史，“书目摘要”用来提高显示书目摘要的速度，'访问计数'用来存储OPAC访问计数和数字资源浏览次数，'访问日志'用来存储数字资源访问日志记录。");
            if (string.IsNullOrEmpty(mongoDB) == false)
            {
                this.OutputInfo("\r\n检查结果：已配置MongoDB数据库参数，配置良好。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：未配置MongoDB数据库参数，缺少'出纳历史'、'书目摘要'、'访问计数'、'访问日志'这4个库，建议配置上。");
            }

            //预约到书配置
            //说明:检查预约保留期
            /*
<arrived dbname="预约到书" reserveTimeSpan="1day" outofReservationThreshold="10" canReserveOnshelf="true" notifyTypes="dpmail,email,mq" />
             */
            this.OutputEmprty();
            this.OutputInfo("==预约到书配置==");
            string arrived = "";
            string days = "";
            string canReserveOnshelf = "";
            node = root.SelectSingleNode("arrived");
            string partInfo = "";
            if (node != null)
            {
                arrived = node.OuterXml;

                days = DomUtil.GetAttr(node, "reserveTimeSpan");
                canReserveOnshelf = DomUtil.GetAttr(node, "canReserveOnshelf");
                if (canReserveOnshelf.ToLower() == "true")
                {
                    partInfo = "支持在架预约，需和老师沟通确认实际是否支持在架预约。";
                }
                else
                {
                    partInfo = "不支持在架预约。";
                }
            }
            if (string.IsNullOrEmpty(arrived) == false)
            {
                // rfid = DomUtil.GetIndentXml(rfid);
                this.OutputInfo(arrived);
            }
            else
            {
                this.OutputInfo("未配置预约到书参数");
            }
            /*
             * 功能说明：reserveTimeSpan表示预约到书保留天数，outofReservationThreshold表示预约到书未取的次数限制，超过这个次数后面不能预约；canReserveOnshelf表示是否支持在架预约；notifyTypes表示预约到书通知方式，其中mq表示微信通知。

检查结果：
已配置预约到书参数，保留天数为？,支持在架预约，需和老师沟通确认实际是否支持在架预约。
已配置预约到书参数，保留天数为？,不支持在架预约。
未配置预约到书参数，建议配置上。
             */
            this.OutputInfo("\r\n功能说明：reserveTimeSpan表示预约到书保留天数，outofReservationThreshold表示预约到书未取的次数限制，超过这个次数后面不能预约；canReserveOnshelf表示是否支持在架预约；notifyTypes表示预约到书通知方式，其中mq表示微信通知。");
            if (string.IsNullOrEmpty(arrived) == false)
            {
                this.OutputInfo("\r\n检查结果：已配置预约到书参数，保留天数为" + days + "，" + partInfo);
            }
            else
            {
                this.OutputInfo("未配置预约到书参数，建议配置上。");
            }

            //消息配置
            /*
<message dbname="消息" reserveTimeSpan="365day" defaultQueue=".\private$\dp2library_demo" />
             */
            this.OutputEmprty();
            this.OutputInfo("==消息配置==");
            string message = "";
            node = root.SelectSingleNode("message");
            string defaultQueue = "";
            if (node != null)
            {
                message = node.OuterXml;
                defaultQueue = DomUtil.GetAttr(node, "defaultQueue");
            }
            if (string.IsNullOrEmpty(message) == false)
            {
                // rfid = DomUtil.GetIndentXml(rfid);
                this.OutputInfo(message);
            }
            else
            {
                this.OutputInfo("未配置消息参数");
            }
            /*
             * 功能说明：defaultQueue表示是否启动MSMQ消息队列，用于给微信发通知，如果缺少该参数，则不能给微信发通知。

检查结果：
已配置消息队列，请确认公众号功能是否正常开通，如未开通公众号，建议去掉消息队列参数，防止MSMQ消息堆积太多。
未配置消息队列，请确认公众号功能是否正常开通，如已开通公众号，需配置上消息队列，这样读者微信才能收到通知。
             */
            this.OutputInfo("\r\n功能说明：defaultQueue表示是否启动MSMQ消息队列，用于给微信发通知，如果缺少该参数，则不能给微信发通知。");
            if (string.IsNullOrEmpty(message) == false)
            {
                if (string.IsNullOrEmpty(defaultQueue) == false)
                    this.OutputInfo("\r\n检查结果：已配置消息队列defaultQueue，请确认公众号功能是否正常开通，如未开通公众号，建议去掉消息队列参数，防止MSMQ消息堆积太多。");
                else
                    this.OutputInfo("\r\n检查结果：未配置消息队列defaultQueue，请确认公众号功能是否正常开通，如已开通公众号，需配置上消息队列，这样读者微信才能收到通知。");
            }
            else
            {
                this.OutputInfo("未配置消息参数，建议配置上。");
            }

            // smtpServer
            //   < smtpServer address = "128.0.0.8686" managerEmail = "fjb@163.net" />
            this.OutputEmprty();
            this.OutputInfo("==邮件服务器配置==");
            string smtpServer = "";
            node = root.SelectSingleNode("smtpServer");
            if (node != null)
            {
                smtpServer = node.OuterXml;
            }
            if (string.IsNullOrEmpty(smtpServer) == false)
            {
                // rfid = DomUtil.GetIndentXml(rfid);
                this.OutputInfo(smtpServer);
            }
            else
            {
                this.OutputInfo("未配置邮件服务器");
            }
            /*
             * 功能说明：邮件服务器用来把一些超期通知，给读者发邮件。目前一般不配置这项参数，因为邮箱服务器本身会有一些限制，不一定能发送成功。

检查结果：
未配置邮件服务器，属正常情况。
配置了邮件服务器，需与用户单位确认，邮箱服务器是否能正常转发邮件，读者是否能正常收到邮件。

             */
            this.OutputInfo("\r\n功能说明：邮件服务器用来把一些超期通知，给读者发邮件。目前一般不配置这项参数，因为邮箱服务器本身可能有一些限制，不一定能发送成功。");
            if (string.IsNullOrEmpty(smtpServer) == true)
            {
                this.OutputInfo("\r\n检查结果：未配置邮件服务器，属正常情况。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：配置了邮件服务器，需与用户单位确认，邮箱服务器是否能正常转发邮件，读者是否能正常收到邮件。");
            }


            // 读者同步
            /*
<patronReplication 
interfaceUrl="ipc://CardCenterChannel/CardCenterServer" 
patronDbName="读者"
idElementName="barcode"
/>
             */
            this.OutputEmprty();
            this.OutputInfo("==读者同步配置==");
            string patronReplication = "";
            node = root.SelectSingleNode("patronReplication");
            if (node != null)
            {
                patronReplication = node.OuterXml;
            }
            if (string.IsNullOrEmpty(patronReplication) == false)
            {
                this.OutputInfo(patronReplication);
            }
            else
            {
                this.OutputInfo("未配置读者同步功能");
            }
            /*
功能说明：读者同步功能是指从第三方系统或者一卡通中心自动同步读者数据到dp2系统，如果配置了读者同步参数，需检查读者同步功能是否正常。

检查结果：
未配置读者同步，属正常情况，但需了解一下用户有没有使用其它方式进行读者同步。
配置了读者同步功能，需与用户单位确认读者同步功能是否正常。
             */
            this.OutputInfo("\r\n功能说明：读者同步功能是指从第三方系统或者一卡通中心自动同步读者数据到dp2系统，如果配置了读者同步参数，需检查读者同步功能是否正常。");
            if (string.IsNullOrEmpty(patronReplication) == true)
            {
                this.OutputInfo("\r\n检查结果：未配置读者同步，属正常情况，但需了解一下用户有没有使用其它方式进行读者同步。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：配置了读者同步功能，需与用户单位确认读者同步功能是否正常。");
            }

            //后台任务monitors
            /*
<monitors>
        <readersMonitor notifyDef="-3day" startTime="16:10" types="dpmail,email,mq" />
        <arriveMonitor startTime="23:00" />
        <messageMonitor startTime="23:00" />
    </monitors>
             */
            this.OutputEmprty();
            this.OutputInfo("==后台任务运行时间==");
            string monitors = "";
            string notifyDef = "";
            node = root.SelectSingleNode("monitors");
            if (node != null)
            {
                monitors = node.OuterXml;
                XmlNode readersMonitor = node.SelectSingleNode("readersMonitor");
                if (readersMonitor != null)
                {
                    notifyDef = DomUtil.GetAttr(readersMonitor, "notifyDef").Trim();
                }
            }
            if (string.IsNullOrEmpty(monitors) == false)
            {
                monitors = DomUtil.GetIndentXml(monitors);
                this.OutputInfo(monitors);
            }
            else
            {
                this.OutputInfo("未配置后台任务运行时间");
            }
            /*
             * 功能说明：readersMonitor负责运行读者超期任务，其中notifyDef表示到期前提醒；arriveMonitor负责运行预约到书；messageMonitor负责消息清扫任务。

检查结果：
后台任务配置正常，有图书即将到期提醒。
后台任务配置正常，但无图书即将到期提醒。需和用户单位确认是否开通这项功能。
未配置后台任务运行时间，建议配置上。
             */
            this.OutputInfo("\r\n功能说明：readersMonitor负责运行读者超期任务，其中notifyDef表示到期前提醒；arriveMonitor负责运行预约到书；messageMonitor负责消息清扫任务。");
            if (string.IsNullOrEmpty(monitors) == false)
            {
                if (string.IsNullOrEmpty(notifyDef) == false)
                {
                    this.OutputInfo("\r\n检查结果：后台任务配置正常，有图书即将到期提醒。");
                }
                else
                {
                    this.OutputInfo("\r\n检查结果：后台任务配置正常，但无图书即将到期提醒。需和用户单位确认是否开通这项功能。");
                }
            }
            else
            {
                this.OutputInfo("\r\n检查结果：未配置后台任务运行时间，建议配置上。");
            }

            //实用库
            /*
  <utilDb>
        <database name="出版者" type="publisher" />
        <database name="盘点" type="inventory" />
    </utilDb>
             */
            /*
<utilDb>元素内由若干<database>元素配置定义了系统运作所需要的若干实用库。
<database>元素有两个属性：name属性定义实用库的名字；type属性定义了实用库的具体类型，类型值有“publisher”“zhongcihao”等。
当实用库的类型为“publisher”时，表示这是一个出版者库。出版者库负责存储ISBN号之出版社部分和出版地、出版社名(UNIMARC格式210字段$a$c子字段)之间的对照关系；和ISBN号之出版社部分和出版地代码(UNIMARC格式 102字段$a$b子字段)之间的对照关系。出版者库的结构可参见7.8。
当实用库的类型为“zhongcihao”时，表示这是一个种次号库。种次号库负责存储索书类号和该类当前尾号之间的对照关系。种次号库的结构可参见7.8。
             */
            this.OutputEmprty();
            this.OutputInfo("==实用库配置==");
            string utilDb = "";
            string zhongcihao = "";
            node = root.SelectSingleNode("utilDb");
            if (node != null)
            {
                utilDb = node.OuterXml.Trim();
                XmlNode nodezhongcihao = node.SelectSingleNode("database[@type='zhongcihao']");
                if (nodezhongcihao != null)
                {
                    zhongcihao = DomUtil.GetAttr(nodezhongcihao, "name");  //取出种次号名称
                }
            }
            if (string.IsNullOrEmpty(utilDb) == false)
            {
                utilDb = DomUtil.GetIndentXml(utilDb);
                this.OutputInfo(utilDb);
            }
            else
            {
                this.OutputInfo("未配置实用库");
            }
            /*
功能说明：实用库主要有'出版者库'和'种次号库'，类型为'publisher'表示是出版者库，类型为'zhongcihao'表示种次号库。当存在种次号库需改进，dp2 V3不需要再设置种次号库了，系统会自动按对应分类的尾号取号。

检查结果：
有配置种次号库，需与用户单位沟通，进行改进，去掉种次库，系统会自动取号。
实用库配置正常。
未配置实用库，属正常情况。
             */
            this.OutputInfo("\r\n功能说明：实用库类型为'publisher'表示是出版者库，类型为'zhongcihao'表示种次号库，类型为'inventory'表示盘点库。当存在种次号库需改进，dp2 V3不需要再设置种次号库了，系统会自动按对应分类的尾号取号。");
            if (string.IsNullOrEmpty(utilDb) == false)
            {
                if (string.IsNullOrEmpty(zhongcihao) == false)
                {
                    this.OutputInfo("\r\n检查结果：有配置种次号库，需与用户单位沟通，进行改进，去掉种次库，系统会自动取号。");
                }
                else
                {
                    this.OutputInfo("\r\n检查结果：实用库配置正常。");
                }
            }
            else
            {
                this.OutputInfo("\r\n检查结果：未配置实用库，属正常情况。");
            }


            // 登录与读者密码设置
            //<login globalAddRights="clientscanvirus" patronPasswordExpireLength="90days" patronPasswordStyle="style-1" />
            // globalAddRights
            this.OutputEmprty();
            this.OutputInfo("==登录与读者密码设置==");
            string login = "";
            string globalAddRights = "";
            string patronPasswordExpireLength = "";
            string patronPasswordStyle = "";
            node = root.SelectSingleNode("login");
            if (node != null)
            {
                login = node.OuterXml;

                globalAddRights = DomUtil.GetAttr(node, "globalAddRights");
                patronPasswordExpireLength = DomUtil.GetAttr(node, "patronPasswordExpireLength");
                patronPasswordStyle = DomUtil.GetAttr(node, "patronPasswordStyle");
            }
            if (string.IsNullOrEmpty(login) == false)
            {
                login = DomUtil.GetIndentXml(login);
                this.OutputInfo(login);
            }
            else
            {
                this.OutputInfo("未配置登录参数");
            }
            /*
             * 
             * 功能说明：globalAddRights='clientscanvirus'表示内务登录时检查病毒；patronPasswordExpireLength表示读者密码失效期；patronPasswordStyle表示读者密码健壮性格式要求。

检查结果：
未配置内务登录时检查病毒，建议配置。
未配置工作人员密码失效期或密码健壮性格式要求，安全性不高，建立配置上。
登录安全性配置良好。
             */
            this.OutputInfo("\r\n功能说明：globalAddRights='clientscanvirus'表示内务登录时检查病毒；patronPasswordExpireLength表示读者密码失效期；patronPasswordStyle表示读者密码健壮性格式要求。");
            partInfo = "";
            if (string.IsNullOrEmpty(globalAddRights) == true)// != "clientscanvirus")
            {
                partInfo = "未配置内务登录时检查病毒，建议配置。";
            }
            else
            {
                if (globalAddRights != "clientscanvirus")
                    partInfo = "内务登录检查病毒配置,需改为正确的值。";
            }

            if (string.IsNullOrEmpty(patronPasswordExpireLength) == true
                || string.IsNullOrEmpty(patronPasswordStyle) == true)
            {
                partInfo += "未配置工作人员密码失效期或密码健壮性格式要求，安全性不高，建议配置上。";
            }

            if (string.IsNullOrEmpty(partInfo) == false)
            {
                this.OutputInfo("\r\n检查结果：" + partInfo);
            }
            else
            {
                this.OutputInfo("\r\n检查结果：登录安全性配置良好。");
            }



            //工作人员密码设置
            //< accounts passwordExpireLength = "90days" passwordStyle = "style-1,login" >
            this.OutputEmprty();
            this.OutputInfo("==工作人员密码配置==");
            string passwordExpireLength = "";
            string passwordStyle = "";
            bool hasPasswordExpire = false;
            bool hasPasswordStyle = false;
            node = root.SelectSingleNode("accounts");
            if (node != null)
            {
                passwordExpireLength = DomUtil.GetAttr(node, "passwordExpireLength");
                passwordStyle = DomUtil.GetAttr(node, "passwordStyle");
            }
            if (string.IsNullOrEmpty(passwordExpireLength) == false)
            {
                this.OutputInfo("passwordExpireLength=" + passwordExpireLength);
                hasPasswordExpire = true;
            }
            else
            {
                this.OutputInfo("未配置passwordExpireLength");
            }
            if (string.IsNullOrEmpty(passwordStyle) == false)
            {
                this.OutputInfo("passwordStyle=" + passwordStyle);
                hasPasswordStyle = true;
            }
            else
            {
                this.OutputInfo("未配置passwordStyle");
            }
            /*
             * 功能说明：工作人员密码配置passwordExpireLength表示密码失效期，配置passwordStyle表示密码健壮性格式要求。

            检查结果：
            工作人员密码安全性配置良好。
            未配置工作人员密码失效期或密码健壮性格式要求，安全性不高，建议配置上。
             */
            this.OutputInfo("\r\n功能说明：工作人员密码配置passwordExpireLength表示密码失效期，配置passwordStyle表示密码健壮性格式要求。");

            if (hasPasswordExpire == true && hasPasswordStyle == true)
            {
                this.OutputInfo("\r\n检查结果：工作人员密码安全性配置良好。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：未配置工作人员密码失效期或密码健壮性格式要求，安全性不高，建议配置上。");
            }



            // 流通相关配置
            //<circulation verifyBarcode="true" patronAdditionalFroms="证号,姓名" notifyTypes="mq" verifyReaderType="true" borrowCheckOverdue="true" />
            this.OutputEmprty();
            this.OutputInfo("==业务参数==");
            string circulation = "";
            node = root.SelectSingleNode("circulation");
            if (node != null)
            {
                circulation = node.OuterXml;
            }
            if (string.IsNullOrEmpty(circulation) == false)
            {
                //utilDb = DomUtil.GetIndentXml(utilDb);
                this.OutputInfo(circulation);
            }
            else
            {
                this.OutputInfo("未配置circulation参数节点");
            }
            // 进一步检查里面的属性
            bool noVerify = false;
            if (node != null)
            {
                string verifyBarcode = DomUtil.GetAttr(node, "verifyBarcode").Trim();
                if (verifyBarcode.ToLower() != "true")
                {
                    noVerify = true;
                }

                string verifyReaderType = DomUtil.GetAttr(node, "verifyReaderType").Trim();
                if (verifyReaderType.ToLower() != "true")
                {
                    noVerify = true;
                }

                string verifyBookType = DomUtil.GetAttr(node, "verifyBookType").Trim();
                if (verifyBookType.ToLower() != "true")
                {
                    noVerify = true;
                }
            }
            /*
            功能说明：verifyBarcode="true"表示校验条码，verifyBookType="true"表示校验图书类型，verifyReaderType="true"表示校验读者类型。

            检查结果：
            配置良好，已启用'条码校验'、'图书类型校验'、'读者类型校验'。
            未启用'条码校验'、'图书类型校验'、'读者类型校验' 其中一项或多项，需改进，配置上校验功能。
             */

            this.OutputInfo("\r\n功能说明：verifyBarcode='true'表示校验条码，verifyBookType='true'表示校验图书类型，verifyReaderType='true'表示校验读者类型。");


            if (noVerify == true)
            {
                this.OutputInfo("\r\n检查结果：未启用'条码校验'、'图书类型校验'、'读者类型校验' 其中一项或多项，需改进，配置上校验功能。");
            }
            else
            {
                this.OutputInfo("\r\n检查结果：配置良好，已启用'条码校验'、'图书类型校验'、'读者类型校验'。");
            }


        }

        // 读者部门
        private void button_department_Click(object sender, EventArgs e)
        {

            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始统计读者部门==", start));

                // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
                this._cancel.Dispose();
                this._cancel = new CancellationTokenSource();
                this.GroupReaderDept(this._patrons, this._cancel.Token);


                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束统计读者部门,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 下载文件
        void DownloadLibrary()
        {
            // 本地文件，如果原来有，先删除
            string fileName = this.Dir + "/" + this._mainForm.LibraryName + "-library.xml";
            if (File.Exists(fileName) == true)
                File.Delete(fileName);
            FileStream stream = File.Create(fileName);


            byte[] baTotal = null;
            //
            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                string strPath = "!library.xml";//this.ServerFilePath;
                string strStyle = "content,data";//,metadata,timestamp,outputpath,gzip";

                byte[] baContent = null;

                long lStart = 0;
                int nPerLength = -1;

                byte[] timestamp = null;
                long lTotalLength = -1;
                for (; ; )
                {
                    //if (_cancel.IsCancellationRequested)
                    //{
                    //    strError = "中断";
                    //    goto ERROR1;
                    //}

                    string strMessage = "";
                    string strMetadata = "";

                    GetResResponse response = channel.GetRes(strPath,
                        lStart,
                        nPerLength,
                        strStyle);
                    long lRet = response.GetResResult.Value;
                    if (lRet == -1)
                    {
                        this.OutputInfo("获得服务器文件 '" + strPath + "' 时发生错误： " + response.GetResResult.ErrorInfo, true, false);
                        return;
                    }
                    baContent = response.baContent;
                    lTotalLength = lRet;

                    // 写入本地文件
                    if (baContent.Length > 0)
                    {
                        baTotal = ByteArray.Add(baTotal, baContent);

                        stream.Write(baContent, 0, baContent.Length);
                        stream.Flush();
                        lStart += baContent.Length;


                    }
                    if (lStart >= lRet)
                    {
                        break;
                    }

                } // end of for

                //string  strResult = ByteArray.ToString(baTotal, Encoding.UTF8);

                // // 把baTotal加载到dom
                this._libraryDom1 = new XmlDocument();
                stream.Seek(0, SeekOrigin.Begin);
                this._libraryDom1.Load(stream);

            }
            catch (Exception ex)
            {
                this.OutputInfo(ExceptionUtil.GetDebugText(ex));
                return;
            }
            finally
            {
                this._mainForm.ReturnChannel(channel);

                // 关闭文件
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
        }

        XmlDocument _libraryDom1 = null;

        private XmlDocument LibraryDom
        {
            get
            {
                // 如果library.xml还未下载到本地，先下载
                if (this._libraryDom1 == null)
                {
                    this.DownloadLibrary();
                }

                return this._libraryDom1;
            }
        }


        // 下载library.xml
        private void button_downloadLibrary_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始下载library.xml==", start));

                // 干活
                this.DownloadLibrary();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束下载library.xml,详见输出目录,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_calendar_Click(object sender, EventArgs e)
        {

            EnableControls(false);
            try
            {
                // 空2行
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始获取开馆日历==", start));



                // sheet名
                IXLWorksheet ws = this.CreateSheet("开馆日历");
                // 设置标题
                string[] titles = {
                    "日历名称",
                    "日期范围",
                    "说明",
                    "是否设置了闭馆日期"
                    };
                int i = 1;
                foreach (string s in titles)
                {
                    ws.Cell(1, i++).Value = s;
                }

                /*
    <calendars>
        <calendar name="基本日历" range="20080101-20091231" comment="图书馆开馆日历">20080101,20090101</calendar>
    </calendars>
                 */
                int index = 1;
                XmlNodeList calendarList = this.LibraryDom.DocumentElement.SelectNodes("calendars/calendar");
                foreach (XmlNode calendar in calendarList)
                {
                    index++;
                    string name = DomUtil.GetAttr(calendar, "name");
                    string range = DomUtil.GetAttr(calendar, "range");
                    string comment = DomUtil.GetAttr(calendar, "comment").Trim();

                    string text = calendar.InnerText.Trim();

                    // 写excel
                    ws.Cell(index, 1).Value = name;
                    ws.Cell(index, 2).Value = range;
                    ws.Cell(index, 3).Value = comment;
                    if (string.IsNullOrEmpty(text) == true)
                    {
                        ws.Cell(index, 4).Value = "否";
                    }
                    else
                    {
                        ws.Cell(index, 4).Value = "是";
                    }
                }

                // 设置excel格式
                this.SetExcelStyle(ws, index, titles.Length);

                //保存excel
                this._workbook.SaveAs(this.ExcelFileName);

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束获取开馆日历,详见excel,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_security_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                this.OutputEmprty(2);

                // 开始时间
                DateTime start = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==开始安全配置==", start));

                this.OutputEmprty();
                /*
                 * 工作人员密码安全配置：建议为工作人员启用密码失效期和密码健壮性格式。
读者密码安全配置：建议为读者启用密码失效期和密码健壮性格式。

防火墙检查：已启用防火墙。
防火墙禁用端口检查：已禁用MongoDB使用的27017端口。

MySQL配置为单机命名管道访问，不支持外部访问。
dp2kernel仅开通本机访问协议，不支持外部访问。

                 */
                XmlNode root = this.LibraryDom.DocumentElement;

                // 工作人员密码安全配置
                string passwordExpireLength = "";
                string passwordStyle = "";
                XmlNode node = root.SelectSingleNode("accounts");
                if (node != null)
                {
                    passwordExpireLength = DomUtil.GetAttr(node, "passwordExpireLength");
                    passwordStyle = DomUtil.GetAttr(node, "passwordStyle");
                }
                if (string.IsNullOrEmpty(passwordExpireLength) == true ||
                    string.IsNullOrEmpty(passwordStyle) == true)
                {
                    this.OutputInfo("工作人员密码安全配置：建议为工作人员启用密码失效期和密码健壮性格式，提高安全性。");
                }
                else
                {
                    this.OutputInfo("工作人员密码安全配置：已为工作人员启用密码失效期和密码健壮性格式，安全性高。");
                }

                // 读者密码安全配置
                string patronPasswordExpireLength = "";
                string patronPasswordStyle = "";
                node = root.SelectSingleNode("login");
                if (node != null)
                {
                    patronPasswordExpireLength = DomUtil.GetAttr(node, "patronPasswordExpireLength").Trim();
                    patronPasswordStyle = DomUtil.GetAttr(node, "patronPasswordStyle").Trim();
                }
                if (string.IsNullOrEmpty(patronPasswordExpireLength) == true ||
    string.IsNullOrEmpty(patronPasswordStyle) == true)
                {
                    this.OutputInfo("读者密码安全配置：建议为读者启用密码失效期和密码健壮性格式，提高安全性。");
                }
                else
                {
                    this.OutputInfo("读者密码安全配置：已为读者启用密码失效期和密码健壮性格式，安全性高。");
                }

                this.OutputEmprty();
                this.OutputInfo("防火墙检查：已启用防火墙。");
                this.OutputInfo("防火墙禁用端口检查：已禁用MongoDB使用的27017端口。");


                this.OutputEmprty();
                this.OutputInfo("MySQL配置为单机命名管道访问，不支持外部访问。");
                this.OutputInfo("dp2kernel仅开通本机访问协议，不支持外部访问。");

                this.OutputEmprty();

                // 结束时间
                DateTime end = DateTime.Now;
                this.OutputInfo(GetInfoAddTime("==结束安全配置,用时" + this.GetSeconds(start, end) + "秒==", end));
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_patronBarcode_Click(object sender, EventArgs e)
        {
            // 空2行
            this.OutputEmprty(2);

            // 开始时间
            DateTime start = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==开始校验读者条码==", start));

            EnableControls(false);
            try
            {
                string strError = "";

                long errorCount = 0;
                RestChannel channel = this._mainForm.GetChannel();
                try
                {
                    int index = 0;
                    foreach (Patron item in this._patrons)
                    {
                        // 当外部让停止时，停止循环
                        this._cancel.Token.ThrowIfCancellationRequested();
                        Application.DoEvents();

                        index++;
                        this.OutputInfo("校验读者条码第" + index.ToString(), false, true);

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
                            this.OutputInfo("校验读者条码出错:" + strError, true, false);
                            return;
                        }
                        // 不合法
                        if (lRet == 0 || lRet == 1)
                        {
                            errorCount++;
                            if (errorCount == 1)
                            {
                                // 仅输出到文件
                                this.OnlyOutput2File("以下读者条码不符合规则");
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

                        this._cancel.Token.ThrowIfCancellationRequested();

                    }

                    if (errorCount > 0)
                    {
                        this.OutputEmprty();
                        this.OutputInfo("不符合规则的读者条码共有" + errorCount + "条。");
                    }
                }
                finally
                {
                    this._mainForm.ReturnChannel(channel);
                }
            }
            finally
            {

                EnableControls(true);
            }

            // 结束时间
            DateTime end = DateTime.Now;
            this.OutputInfo(GetInfoAddTime("==结束校验读者条码,详见txt文件,用时" + this.GetSeconds(start, end) + "秒==", end));

        }
    }



        public class BookTypeGroup
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

    public class PatronGroup
    {
        // 册类型
        public string name { get; set; }

        public List<Patron> Items { get; set; }

        public int count { get; set; }


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

    // 读者记录
    public class Patron
    {
        // 路径
        public string path { get; set; }

        // 册条码
        public string barcode { get; set; }

        // 姓名
        public string name { get; set; }


        // 读者类型
        public string readerType { get; set; }

        // 部门
        public string department { get; set; }

        // 馆代码
        public string libraryCode { get; set; }

        public string errorInfo { get; set; }

        public int borrowCount { get; set; }
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

        // 读者库有用
        public string libraryCode { get; set; }
    }

    public class simpleLoc
    {
        public string name { get; set; }

        public string canBorrow { get; set; }
    }

    }

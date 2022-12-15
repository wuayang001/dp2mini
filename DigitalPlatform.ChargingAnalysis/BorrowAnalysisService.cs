using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using xml2html;

namespace DigitalPlatform.ChargingAnalysis
{
    public class BorrowAnalysisService
    {
        #region 单一实例

        static BorrowAnalysisService _instance;

        // 构造函数
        private BorrowAnalysisService()
        {
            //this.dp2ServerUrl = Properties.Settings.Default.dp2ServerUrl;
            //this.dp2Username = Properties.Settings.Default.dp2Username;
            //this.dp2Password = Properties.Settings.Default.dp2Password;
            //this._libraryChannelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }
        private static object _lock = new object();
        static public BorrowAnalysisService Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new BorrowAnalysisService();
                    }
                }
                return _instance;
            }
        }

        Hashtable _clcHT = new Hashtable();

        string _commentFile = "";
        Hashtable _commentHT = new Hashtable();

        List<RoleItem> _roles = new List<RoleItem>();
        List<string> _commentTemplates=new List<string> ();

        // todo要改为有返回出错信息
        // chargingAnalysisDataDir：阅读分析的数据目录，里面有html配置文件
        public int Init(string dataDir,
            string serverUrl, string userName, string passowrd, string loginParameters,
            out string strError)
        {
            strError = "";

            // 阅读分析的数据目录
            this._dataDir = dataDir;

            // 类号中英文对照文件
            if (this._clcHT !=null)
                this._clcHT.Clear();

            string clcclassFile = Path.Combine(this._dataDir, "clcclass.txt");
            if (File.Exists(clcclassFile) == false)
            {
                strError = "'" + clcclassFile + "'配置文件不存在，请联系系统管理员。";
                return -1;
            }
            using (StreamReader reader = new StreamReader(clcclassFile))//, Encoding.UTF8))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    string[] a = line.Split(new char[] { '\t' });
                    _clcHT[a[0]]= a[1];
                    //sb.Append("<p>").Append(line).Append("</p>").AppendLine();
                }
            }

            // 馆员评语
            if (this._commentHT != null)
                this._commentHT.Clear();
            this._commentFile = Path.Combine(this._dataDir, "comment.xml");
            if (File.Exists(this._commentFile) == true)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this._commentFile);

                XmlNodeList list = dom.DocumentElement.SelectNodes("comment");
                foreach (XmlNode node in list)
                {
                    string patronBarcode = DomUtil.GetAttr(node, "patronBarcode");
                    string comment = DomUtil.GetNodeText(node);
                    _commentHT[patronBarcode] = comment;
                }
            }

            // 阅读称号 titleRole.xml
            if (this._roles !=null)
                this._roles.Clear();
            string titleRoleFile = Path.Combine(this._dataDir, "titleRole.xml");
            if (File.Exists(titleRoleFile) == true)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(titleRoleFile);

                XmlNodeList list = dom.DocumentElement.SelectNodes("role");
                if (list.Count > 0)
                {
                    foreach (XmlNode node in list)
                    {

                        RoleItem item = new RoleItem();
                        item.title = DomUtil.GetAttr(node, "title");
                        string borrowCount = DomUtil.GetAttr(node, "borrowCount");
                        try
                        {
                            item.borrowCount = Convert.ToInt32(borrowCount);
                        }
                        catch (Exception ex)
                        {
                            strError = "阅读称号配置文件titleRole.xml中borrowCount属性必须是数字。"+ex.Message;
                            return -1;
                        }
                        _roles.Add(item);
                    }

                    // 按借阅量倒序排序。
                    this._roles=this._roles.OrderByDescending(x => x.borrowCount).ToList();
                }
            }

            // 评语模板commentTemplate.xml
            if (this._commentTemplates != null)
                this._commentTemplates.Clear();
            this._commentTemplateFile = Path.Combine(this._dataDir, "commentTemplate.xml");
            if (File.Exists(this._commentTemplateFile) == true)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this._commentTemplateFile);

                XmlNodeList list = dom.DocumentElement.SelectNodes("comment");
                foreach (XmlNode node in list)
                {
                    string comment = node.InnerText.Trim();
                    if (string.IsNullOrEmpty(comment)==false)
                        this._commentTemplates.Add(comment);
                }
            }

            // 访问dp2服务器的地址和帐号
            this.dp2ServerUrl = serverUrl;
            this.dp2Username = userName;
            this.dp2Password = passowrd;
            this.dp2LoginParameters = loginParameters;
            this._channelPool.BeforeLogin -= channelPool_BeforeLogin;
            this._channelPool.BeforeLogin += channelPool_BeforeLogin;

            return 0;
        }

        // 评语模板文件 2022/12/13
        public string _commentTemplateFile = "";

        // 评语模板 2022/12/7
        public List<string> CommentTemplates
        {
            get
            {
                return this._commentTemplates;
            }
        }

        public bool SaveCommentTemplate(string commentT,out string error)
        {
            error = "";

            if (this._commentTemplates == null)
            {
                error = "异常情况：内存中的_commentTemplates不可能为null。";
                return false;
            }

            // 先检查一下模板中是否已经存在
            foreach (string one in this._commentTemplates)
            {
                if (one == commentT)
                {
                    error = "评语模板中已经存在此项。";
                    return false;
                }
            }
            

            // 更新到内存中
            this._commentTemplates.Add(commentT);

            // 保存到评语模板中
            string xml = "<root>";
            foreach (string comm in this._commentTemplates)
            {
                xml += "<comment>"+comm+"</comment>";
            }
            xml += "</root>";
            using (StreamWriter writer = new StreamWriter(this._commentTemplateFile, false, Encoding.UTF8))
            {
                writer.Write(xml);
            }

            return true;
        }

        #endregion

        #region 关于通道
        // 通道池
        RestChannelPool _channelPool = new RestChannelPool();
        private string _dataDir;

        public string dp2ServerUrl { get; set; }
        public string dp2Username { get; set; }
        public string dp2Password { get; set; }

        public string dp2LoginParameters { get; set; }

        void channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (string.IsNullOrEmpty(this.dp2Username))
            {
                e.Cancel = true;
                e.ErrorInfo = "尚未登录";
            }

            e.LibraryServerUrl = this.dp2ServerUrl;
            e.UserName = this.dp2Username;
            e.Parameters = dp2LoginParameters;//"type=worker,client=dp2analysis|0.01";
            e.Password = this.dp2Password;
            //e.SavePasswordLong = true;
        }


        public RestChannel GetChannel()
        {
            if (this.dp2ServerUrl == "" || this.dp2Username == "")
            {
                throw new Exception("尚未设置dp2library的Url或用户名");
            }

            RestChannel channel = this._channelPool.GetChannel(this.dp2ServerUrl, this.dp2Username);

            return channel;
        }

        public void ReturnChannel(RestChannel channel)
        {
            this._channelPool.ReturnChannel(channel);
        }



        #endregion


        // 定义委托协议
        public delegate void ShowInfoDelegate(string text);

        // 设置进度条
        public delegate void SetProcessDelegate(int min,int max,int value);


        // 任务名称
        public const string C_task_createXml = "createXml";
        public const string C_task_paiMing = "paiMing";
        public const string C_task_writePaiMing = "writePaiMing";
        public const string C_task_xml2html = "xml2html";

        // 任务状态
        public const string C_state_close = "close";
        public const string C_state_error = "error";

        // 生成任务plan文件名
        public static string CreatePlan(CancellationToken token, 
            string patronBarcodes, 
            string startDate,
            string endDate,
            string dir,
            ShowInfoDelegate showInfo)
        {
            // 检查参数不能为空
            if (string.IsNullOrEmpty(patronBarcodes) == true
                || string.IsNullOrEmpty(startDate) == true
                || string.IsNullOrEmpty(endDate) == true
                || string.IsNullOrEmpty(dir) == true)
            {
                throw new Exception("传入的参数patronBarcodes/startDate/endDate/dir均不能为空。");
            }


            //this.SetProcessInfo("正在创建报表计划文件...");
            showInfo("正在创建报表计划文件...");  //一个回调函数

            string xml = "<root startDate='" + startDate + "' endDate='" + endDate + "' dir='" + dir + "' state=''>";

            // 2022/12/15 把条码号内容也加到计划里，方便后面继续里显示
            xml += "<barcodes>" + patronBarcodes + "</barcodes>";


            List<string> patronList = new List<string>();
            // 拆分证条码号，每个号码一行
            patronBarcodes = patronBarcodes.Replace("\r\n", "\n");
            string[] tempList = patronBarcodes.Split(new char[] { '\n' });
            // 循环每个证条码，把空行清除
            foreach (string one in tempList)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                string barcode = one.Trim();
                if (string.IsNullOrEmpty(barcode) == false)
                    patronList.Add(barcode);
            }

            // 生成createXml的任务
            foreach (string barcode in patronList)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                xml += "<task name='" + C_task_createXml + "' barcode='" + barcode + "' state=''/>";
            }

            // 总的paiMing任务
            xml += "<task name='" + C_task_paiMing + "' state=''/>";

            // 给读者xml写入排名任务writePaiMing
            foreach (string barcode in patronList)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                xml += "<task name='" + C_task_writePaiMing + "' barcode='" + barcode + "' state=''/>";
            }

            // xml2html任务
            foreach (string barcode in patronList)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                xml += "<task name='" + C_task_xml2html + "' barcode='" + barcode + "' state=''/>";
            }

            xml += "</root>";

            // 把xml文件保存到输出目录里。
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);

            string planFile = dir + "\\plan.txt";
            // StreamWriter当文件不存在时，会自动创建一个新文件。
            using (StreamWriter writer = new StreamWriter(planFile, false, Encoding.UTF8))
            {
                // 停止
                token.ThrowIfCancellationRequested();

                // 写到打印文件
                writer.Write(xml);
            }

            //this.SetProcessInfo("完成创建报表计划文件。");
            showInfo("完成创建报表计划文件。");  //一个回调函数

            return planFile;
        }


        public static void ExecutePlan(CancellationToken token,
            string planFile,
            SetProcessDelegate setProcess,
            ShowInfoDelegate showInfo)
        {
            if (File.Exists(planFile) == false)
            {
                throw new Exception("计划文件['" + planFile + "']不存在。");
            }

            // 打开计划文件，取出参数，任务
            XmlDocument dom = new XmlDocument();
            dom.Load(planFile);  //也有可能抛异常


            //<root startDate='2022/11/12' endDate='2022/12/12' dir='D:\a3' state=''>
            //<task name='createXml' barcode='XZP00002' state=''/>
            //<task name='createXml' barcode='XZP00003' state=''/>

            XmlNode root = dom.DocumentElement;
            string rootState=DomUtil.GetAttr(root, "state");
            if (rootState == C_state_close)
            {
                showInfo("计划的状态是close，表示之前已处理完成。");
                return;
            }

            // 取出参数
            string startDate = DomUtil.GetAttr(root, "startDate");
            string endDate = DomUtil.GetAttr(root, "endDate");
            string dir = DomUtil.GetAttr(root, "dir");

            XmlNodeList taskList = root.SelectNodes("task");
            int max = taskList.Count;

            // 设置进度条
            setProcess(0, max, 0);

            int nRet = 0;
            string error = "";

            // 排名dom
            XmlDocument paiMingDom = null;

            int nIndex = 0;
            foreach (XmlNode task in taskList)
            { 
                nIndex++;
                setProcess(-1, -1, nIndex);  // 设置进度条
                showInfo(task.OuterXml);  //显示文字进展

                // 外部停止
                token.ThrowIfCancellationRequested();

                string taskName=DomUtil.GetAttr(task, "name");
                string patronBarcode = DomUtil.GetAttr(task, "barcode");

                string patronXmlFile = dir + "\\" + patronBarcode + ".xml";

                // 完成或出错的不处理
                string state=DomUtil.GetAttr(task, "state");
                if (state == C_state_close  || state==C_state_error)  // 已处理完
                    continue;

                // 执行任务
                if (taskName == C_task_createXml)
                {
                    // 创建数据
                    BorrowAnalysisReport report = null;
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

                    //string fileName = dir + "\\" + barcode + ".xml";
                    // StreamWriter当文件不存在时，会自动创建一个新文件。
                    using (StreamWriter writer = new StreamWriter(patronXmlFile, false, Encoding.UTF8))
                    {
                        // 写到打印文件
                        writer.Write(xml);
                    }

                }
                else if (taskName == C_task_paiMing)
                {
                    // 进行排名，将排名结果写到排名临时文件
                    nRet = PaiMing(token,
                        dir,
                        out error);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (taskName == C_task_writePaiMing)
                {
                    if (paiMingDom == null)
                    {
                        string paiMingFile = dir + "\\paiMing.txt";
                        paiMingDom = new XmlDocument();
                        paiMingDom.Load(paiMingFile);
                    }

                    // 把名次写到对应xml
                    nRet = WritePaiMing(token,dir, patronBarcode, paiMingDom, out error);
                    if (nRet == -1)
                        goto ERROR1;

                }
                else if (taskName == C_task_xml2html)
                {
                    nRet = ConvertReport2Html(token,patronXmlFile,out error);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    //不认识的任务
                    error = "不认识的任务";
                }


                // 把这一任务设置成完成
                DomUtil.SetAttr(task, "state", C_state_close);
                dom.Save(planFile);// 立即保存一下plan文件。
                continue;



            ERROR1:

                // 把这一任务设置成出错
                DomUtil.SetAttr(task, "state", C_state_error);
                if (string.IsNullOrEmpty(error) == false)  //写错误信息
                { 
                    DomUtil.SetAttr(task,"error",error);
                }
                dom.Save(planFile);// 立即保存一下plan文件。
            }

            //end
            // 把plan设置成完成
            DomUtil.SetAttr(root, "state", C_state_close);
            dom.Save(planFile);// 立即保存一下plan文件。

        }

        // xml转html
        private static int ConvertReport2Html(CancellationToken token, string xmlFile, out string error)
        {
            error = "";

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

                error = xmlFile + "转html出错：" + ex.Message;

                return -1;
            }

            return 0;

        }


        // 排名
        private static int PaiMing(CancellationToken token, string dir,out string error)
        {
            error = "";
            //this.SetProcessInfo("开始排名");

            List<paiMingItem> paiMingList = new List<paiMingItem>();

            string[] fiels = Directory.GetFiles(dir, "*.xml");

            //this.SetProcessInfo("把目录中的所有xml文件中相关值装入内存结构");
            //this.SetProcess(0, fiels.Length);

            //int index = 0;
            foreach (string file in fiels)
            {
                //if (file.IndexOf("plan.txt") != -1)
                //    continue;

                //// 更改进度条
                //index++;
                //this.SetProcessValue(index);

                // 停止
                token.ThrowIfCancellationRequested();

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(file);
                }
                catch (Exception ex)
                {
                    error = "装载["+file+"]文件到dom出错:" + ex.Message;
                    return -1;
                }

                XmlNode root = dom.DocumentElement;
                //patron/barcode取内容
                string barcode = DomUtil.GetElementInnerText(root, "patron/barcode");

                //borrowInfo 取 totalBorrowedCount 属性
                string totalBorrowedCount = DomUtil.GetAttr(root, "borrowInfo", "totalBorrowedCount");
                int totalCount = Convert.ToInt32(totalBorrowedCount);

                paiMingList.Add(new paiMingItem(barcode, totalCount, file, dom));
            }

           // this.SetProcessInfo("按借阅量排名");

            // 按总量倒序排
            List<paiMingItem> sortedList = paiMingList.OrderByDescending(x => x.totalBorrowedCount).ToList();

            //this.SetProcessInfo("写回xml文件");
            string paiMingFile = dir+ "\\paiMing.txt";

            string xml = "<root>";

            // 写到一个排名的临时xml
            for (int i = 0; i < sortedList.Count; i++)
            {
                // 停止
                token.ThrowIfCancellationRequested();

                int paiming = i + 1;

                paiMingItem item = sortedList[i];

                xml += "<item barcode='"+item.PatronBarcode+"' paiMing='"+paiming.ToString()+"'/>";

                //XmlNode borrowInfoNode = item.dom.DocumentElement.SelectSingleNode("borrowInfo");

                //DomUtil.SetAttr(borrowInfoNode, "paiming", paiming.ToString());

                //item.dom.Save(item.fileName);

            }
            xml += "</root>";

            // 写到paiming临时文件
            using (StreamWriter writer = new StreamWriter(paiMingFile, false, Encoding.UTF8))
            {
                // 写到打印文件
                writer.Write(xml);
            }


            //this.SetProcessInfo("结束排名");

            return 0;
        }

        // 把名字写到读者xml里
        private static int WritePaiMing(CancellationToken token, 
            string dir,
            string patronBarcode, 
            XmlDocument paiMingDom,
            out string error)
        {
            error = "";

            // 从paiMingDom中找到对应的排名
            XmlNode node = paiMingDom.DocumentElement.SelectSingleNode("item[@barcode='" + patronBarcode + "']");
            if (node == null)
            {
                error = "从排名xml中未找到["+patronBarcode+"]对应的名次。";
                return -1;
            }
            string paiMing = DomUtil.GetAttr(node, "paiMing");


            // 写到xml文件中
            string patronXmlFile = dir + "\\" + patronBarcode + ".xml";
            if (File.Exists(patronXmlFile) == false)
            {
                error = "读者[" + patronBarcode + "]的报表文件["+patronXmlFile+"]不存在。";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(patronXmlFile);
            }
            catch (Exception ex)
            {
                error = "装载报表文件[" + patronXmlFile + "]到dom出错："+ex.Message;
                return -1;
            }

            // 给xml写排名
            XmlNode borrowInfoNode = dom.DocumentElement.SelectSingleNode("borrowInfo");
            DomUtil.SetAttr(borrowInfoNode, "paiMing", paiMing);

            // 保存到文件
            dom.Save(patronXmlFile);

            return 0;
        }


        public string GetCommentTitle(int borrowCount)
        {
            if (this._roles == null || this._roles.Count == 0)
                return "";

            foreach (RoleItem item in this._roles)
            {
                if (borrowCount >= item.borrowCount)
                    return item.title;
            }

            return "";
        }

        public void SetComment(BorrowAnalysisReport report, string comment)
        { 
            // 设到内存对象
            report.comment = comment;

            // 把评语写到文件里
            this.SetComment2file(report.patron.barcode,comment);
        }

        public void SetComment2file(string barcode, string comment)
        {
            // 设到内存hashtable
            this._commentHT[barcode] = comment;

            // 保存到文件里
            string xml = "";
            foreach (string key in this._commentHT.Keys)
            {
                xml += "<comment patronBarcode='" + key + "'>" + this._commentHT[key] + "</comment>";
            }
            xml = "<root>" + xml + "</root>";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            dom.Save(this._commentFile);
        }

        // 分类统计显示前面的多少行
        int _topCount = 5;
        public const string C_Type_Borrow = "0";
        public const string C_Type_History = "1";


        // 创建内存结构
        // patronBarcode：读者证条码
        // times：操作时间范围 2022/11/01~2022/11/01 11:00
        public int Build(CancellationToken token,
            string patronBarcode,
            //string times,
            string startDate,
            string endDate,
            out BorrowAnalysisReport report,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            report = new BorrowAnalysisReport();

            // 先把本地存储的馆长评语设置到对象上
            if (this._commentHT[patronBarcode] != null)
            {
                report.comment = (string)this._commentHT[patronBarcode];
            }


            // 注意这里范围只用于显示，显示的时候就不带日期了，要不显示的很长。
            // 实际检索时要带了时间。
            report.times = startDate + "~" + endDate;
            string times = startDate + " 00:00" + "~" + endDate + " 23:59";


            // 选将创建完成的变量置为false
            report.built = false;
            report.patron = null;

            // 先清空读者数据集合
            report.borrowedItems = null;
            report.classGroups = null;
            report.yearGroups = null;
            report.quarterGroups = null;
            report.monthGroups = null;

            RestChannel channel = this.GetChannel();
            try
            {
                report.borrowedItems = new List<BorrowedItem>();



                // 获取读者信息
                string[] results = null;
                string strRecPath = "";
                lRet = channel.GetReaderInfo(//null,
                    patronBarcode, //读者卡号,
                    "advancexml",   //xml:noborrowhistory,用advancexml不能去掉noborrowhistory
                    out results,
                    out strRecPath,
                    out strError);
                if (lRet == -1)
                {
                    // todo要不要再丰富一下错误信息
                    return -1;
                }
                string strPatronXml = results[0];

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strPatronXml);

                report.patronXml = strPatronXml;

                // 把读者xml解析后存到内存中
                report.patron = this.ParsePatronXml(dom, strRecPath);

                // 把在借信息存到内存数组中，目前与借阅历史存在一起
                XmlNodeList borrowNodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                foreach (XmlNode borrowNode in borrowNodes)
                {
                    BorrowedItem item = new BorrowedItem(patronBarcode, borrowNode);
                    report.borrowedItems.Add(item);
                }


                //==以下为获取借阅历史==
                long totalCount = 0;  //返回的value即为总共命中条数
                long start = 0;
                long preCount = 2;   // 每次获取几条 //lHitCount;
                ChargingItemWrapper[] itemWarpperList = null;
                // 从结果集中取出册记录
                for (; ; )
                {
                    //Application.DoEvents(); // 出让界面控制权

                    // 外面停止
                    token.ThrowIfCancellationRequested();

                    lRet = channel.SearchCharging(patronBarcode,//this.textBox_SearchCharging_patronBarcode.Text.Trim(),
                       times,
                       "return,lost", //this.textBox_searchCharging_actions.Text.Trim(),
                       "",//this.textBox_searchCharging_order.Text.Trim(),
                       start,
                       preCount,
                       out itemWarpperList,
                       out strError); ;
                    if (lRet == -1)
                    {
                        throw new Exception(strError); //直接抛出异常
                    }

                    // 说明结果集里的记录获取完了。
                    if (lRet == 0)
                    {
                        break;
                    }

                    // 返回值为命中总记录数
                    totalCount = lRet;

                    // 将借阅历史记录装载到内存集合中
                    if (itemWarpperList != null && itemWarpperList.Length > 0)
                    {
                        foreach (ChargingItemWrapper one in itemWarpperList)
                        {
                            // 外面停止
                            token.ThrowIfCancellationRequested();

                            BorrowedItem item = new BorrowedItem(one);
                            report.borrowedItems.Add(item);
                        }
                    }

                    // 开始序号增加，为下一轮获取准备
                    start += itemWarpperList.Length;

                    // 获取完记录时，退出循环
                    if (start >= totalCount)
                        break;
                }


                // 获取册信息与书目信息，用同一根通道即可
                foreach (BorrowedItem item in report.borrowedItems)
                {
                    // 外面停止
                    token.ThrowIfCancellationRequested();


                    lRet = this.GetItemInfo(channel, item.ItemBarcode, item, out strError);
                    if (lRet == -1)
                    {
                        // todo 抛出异常？暂时不处理
                    }
                }


                // 计算阅读称号
                if (report.borrowedItems != null && report.borrowedItems.Count > 0)
                {
                    report.commentTitle = this.GetCommentTitle(report.borrowedItems.Count);
                }

                // 按中图法聚合，todo是按分类排序，还是按数量排序？目前先按分类排序。后面看用户反馈。
                report.classGroups = report.borrowedItems.GroupBy(
                    x => new { x.BigClass },
                    (key, item_list) => new GroupItem
                    {
                        name = key.BigClass,
                        caption = GetClassCaption(key.BigClass),
                        items = new List<BorrowedItem>(item_list)
                    }).OrderBy(o => o.name).ToList();

                // 按年聚合
                report.yearGroups = report.borrowedItems.GroupBy(
                    x => new { x.BorrowDate.Year },
                    (key, item_list) => new GroupItem
                    {
                        name = key.Year,
                        //caption = GetClassCaption(key.BigClass),
                        items = new List<BorrowedItem>(item_list)
                    }).OrderBy(o => o.name).ToList();

                // 按季度聚合
                report.quarterGroups = report.borrowedItems.GroupBy(
                    x => new { x.BorrowDate.Quarter },
                    (key, item_list) => new GroupItem
                    {
                        name = key.Quarter,
                        //caption = GetClassCaption(key.BigClass),
                        items = new List<BorrowedItem>(item_list)
                    }).OrderBy(o => o.name).ToList();

                // 按月聚合
                report.monthGroups = report.borrowedItems.GroupBy(
                    x => new { x.BorrowDate.Month },
                    (key, item_list) => new GroupItem
                    {
                        name = key.Month,
                        //caption = GetClassCaption(key.BigClass),
                        items = new List<BorrowedItem>(item_list)
                    }).OrderBy(o => o.name).ToList();


                //==结束==
                report.built = true;
                return 0;

            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        }

        public string GetClassCaption(string clcclass)
        {
            if (string.IsNullOrEmpty(clcclass) == true)
                return "";

            var o = this._clcHT[clcclass];
            if (o == null)
                return "";  //无对应的中文名称

            return (string)o;
        }


        // 解析读者xml到内存对象
        public Patron ParsePatronXml(XmlDocument dom,
            string recPath)
        {
            // 取出个人信息
            Patron patron = new Patron();
            patron.recPath = recPath;



            // 证条码号
            string refID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            patron.refID = refID;

            // 证条码号
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            patron.barcode = strBarcode;
            if (string.IsNullOrEmpty(patron.barcode) == true)
            {
                patron.barcode = "@refId:" + patron.refID;
            }

            // 显示名
            string strDisplayName = DomUtil.GetElementText(dom.DocumentElement, "displayName");
            patron.displayName = strDisplayName;

            // 姓名
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            patron.name = strName;

            // 性别
            string strGender = DomUtil.GetElementText(dom.DocumentElement, "gender");
            patron.gender = strGender;

            // 出生日期
            string strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement, "dateOfBirth");
            if (string.IsNullOrEmpty(strDateOfBirth) == true)
                strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement, "birthday");
            strDateOfBirth = DateTimeUtil.LocalDate(strDateOfBirth);
            patron.dateOfBirth = strDateOfBirth;

            // 证号 2008/11/11
            string strCardNumber = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
            patron.cardNumber = strCardNumber;

            // 身份证号
            string strIdCardNumber = DomUtil.GetElementText(dom.DocumentElement, "idCardNumber");
            patron.idCardNumber = strIdCardNumber;

            // 单位
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");
            patron.department = strDepartment;

            // 职务
            string strPost = DomUtil.GetElementText(dom.DocumentElement, "post");
            patron.post = strPost;

            // 地址
            string strAddress = DomUtil.GetElementText(dom.DocumentElement, "address");
            patron.address = strAddress;

            // 电话
            string strTel = DomUtil.GetElementText(dom.DocumentElement, "tel");
            patron.tel = strTel;
            // 2020/10/19 公众号上支持修改电话，所以改为不用*号，要不用户不知道以前是什么号
            //if (strTel.Length > 4 && bMaskPhone == true)
            //{
            //    string left = strTel.Substring(0, strTel.Length - 4);
            //    left = "".PadLeft(left.Length, '*');
            //    patron.phone = left + strTel.Substring(strTel.Length - 4);
            //}

            // email
            string strEmail = DomUtil.GetElementText(dom.DocumentElement, "email");
            patron.email = this.RemoveWeiXinId(strEmail);//过滤掉微信id

            // 读者类型
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            patron.readerType = strReaderType;

            // 分馆代码
            string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
            patron.libraryCode = libraryCode;

            // 证状态
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            patron.state = strState;

            // 备注
            string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");
            patron.comment = strComment;

            // 发证日期
            string strCreateDate = DomUtil.GetElementText(dom.DocumentElement, "createDate");
            strCreateDate = DateTimeUtil.LocalDate(strCreateDate);
            patron.createDate = strCreateDate;

            // 证失效期
            string strExpireDate = DomUtil.GetElementText(dom.DocumentElement, "expireDate");
            strExpireDate = DateTimeUtil.LocalDate(strExpireDate);
            patron.expireDate = strExpireDate;

            /*
            // 租金 
            string strHireExpireDate = "";
            string strHirePeriod = "";
            XmlNode nodeHire = dom.DocumentElement.SelectSingleNode("hire");
            string strHire = "";
            if (nodeHire != null)
            {
                strHireExpireDate = DomUtil.GetAttr(nodeHire, "expireDate");
                strHirePeriod = DomUtil.GetAttr(nodeHire, "period");

                strHireExpireDate = DateTimeUtil.LocalDate(strHireExpireDate);
                strHirePeriod = dp2WeiXinService.GetDisplayTimePeriodStringEx(strHirePeriod);

                strHire = "周期" + ": " + strHirePeriod + "; "
                + "失效期" + ": " + strHireExpireDate;
            }
            patron.hire = strHire;

            // 押金 2008/11/11
            string strForegift = DomUtil.GetElementText(dom.DocumentElement,
                "foregift");
            patron.foregift = strForegift;

            // 二维码
            // 2022/4/26注：如果用户实例配置了读者信息脱敏，证条码可能是一个***的情况。如果有***号，索性就不显示了，调用者外面再设一下参数。
            if (patron.barcode.IndexOf("*") == -1)
            {
                string qrcodeUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                    + "&type=pqri"
                    + "&barcode=" + HttpUtility.UrlEncode(patron.barcode);
                patron.qrcodeUrl = qrcodeUrl;
            }

            //头像
            string imageUrl = "";
            if (showPhoto == 1)
            {
                // 检查是不是已经有头像图片对象 usage='cardphoto'
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);  //注意xml有命令空间dprms:file
                XmlNodeList fileNodes = dom.DocumentElement.SelectNodes("//dprms:file[@usage='cardphoto']", nsmgr);
                if (fileNodes.Count > 0)
                {
                    string strPhotoPath = recPath + "/object/" + DomUtil.GetAttr(fileNodes[0], "id");

                    imageUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                    + "&objectPath=" + HttpUtility.UrlEncode(strPhotoPath);
                }
            }
            patron.imageUrl = imageUrl; //给patron对象设置头像url


            // 违约
            List<OverdueInfo> overdueLit = new List<OverdueInfo>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            patron.OverdueCount = nodes.Count;
            patron.OverdueCountHtml = ConvertToString(patron.OverdueCount);

            string strWarningText = "";
            List<OverdueInfo> overdueList = dp2WeiXinService.Instance.GetOverdueInfo(strPatronXml,
                out strWarningText);
            patron.overdueList = overdueList;

            // 在借
            nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            patron.BorrowCount = nodes.Count;
            patron.BorrowCountHtml = ConvertToString(patron.BorrowCount);
            int caoQiCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strIsOverdue = DomUtil.GetAttr(node, "isOverdue");
                if (strIsOverdue == "yes")
                {
                    caoQiCount++;
                }
            }
            patron.CaoQiCount = caoQiCount;

            //string strWarningText = "";
            string maxBorrowCountString = "";
            string curBorrowCountString = "";
            List<BorrowInfo2> borrowList = dp2WeiXinService.Instance.GetBorrowInfo(strPatronXml,
                out strWarningText,
                out maxBorrowCountString,
                out curBorrowCountString);
            patron.borrowList = borrowList;
            patron.maxBorrowCount = maxBorrowCountString;
            patron.curBorrowCount = curBorrowCountString;

            // 预约
            nodes = dom.DocumentElement.SelectNodes("reservations/request");
            patron.ReservationCount = nodes.Count;
            patron.ReservationCountHtml = ConvertToString(patron.ReservationCount);
            int arrivedCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string state = DomUtil.GetAttr(node, "state");
                if (state == "arrived")
                {
                    arrivedCount++;
                }
            }
            patron.ArrivedCount = arrivedCount;

            string strReservationWarningText = "";
            List<ReservationInfo> reservations = dp2WeiXinService.Instance.GetReservations(strPatronXml,
                out strReservationWarningText);
            patron.reservations = reservations;
            */
            return patron;
        }
        public const String C_WeiXinIdPrefix = "weixinid:";
        private string RemoveWeiXinId(string email)
        {
            //<email>test@163.com,123,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg,weixinid:o4xvUvnLTg6NnflbYdcS-sxJCGFo,weixinid:testid</email>
            string[] emailList = email.Split(new char[] { ',' });
            string clearEmail = "";
            for (int i = 0; i < emailList.Length; i++)
            {
                string oneEmail = emailList[i].Trim();
                if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == C_WeiXinIdPrefix)
                {
                    continue;
                }

                if (clearEmail != "")
                    clearEmail += ",";

                clearEmail += oneEmail;
            }

            return clearEmail;
        }


        // 获取册信息
        private int GetItemInfo(RestChannel channel,
            string strItemBarcode,
            BorrowedItem item,
            out string strError)
        {
            strError = "";

            // 取出大类
            string bigClass = "空";
            item.BigClass = bigClass;

            // 获取册记录
            string strItemXml = "";
            string strBiblio = "";
            long lRet = channel.GetItemInfo(//null,
                strItemBarcode,
                "xml",
                "table",  //xml，用table格式取题名更方便
                out strItemXml,
                out strBiblio,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // 装载item到dom
            XmlDocument itemDom = new XmlDocument();
            try
            {
                itemDom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = strItemBarcode + " 加载到dom出错：" + ex.Message;
                goto ERROR1;
            }

            XmlNode itemRoot = itemDom.DocumentElement;

            //获取索取号
            string accessNo = DomUtil.GetElementInnerText(itemRoot, "accessNo");
            item.AccessNo = accessNo;

            // 取出大类
            if (string.IsNullOrEmpty(accessNo) == false)
            {
                bigClass = accessNo.Substring(0, 1);
                item.BigClass = bigClass;
            }

            //获取馆藏地
            string location = DomUtil.GetElementInnerText(itemRoot, "location");
            item.Location = location;


            // 处理书目，取题名信息
            /*
            <?xml version="1.0" encoding="utf-8"?>
            <root>
                <line name="_coverImage" value="https://images-cn.ssl-images-amazon.com/images/I/61Q5HSrVmGL.jpg" type="coverimageurl" />
                <line name="题名与责任者" value="钱塘西溪" type="title_area" />
                <line name="出版发行项" value="杭州 : 浙江古籍出版社, 2017" type="publication_area" />
                <line name="载体形态项" value="30,459页 ; 21cm" type="material_description_area" />
                <line name="附注项" value="浙江历史文化研究中心学术成果&#xA;夏承焘全集/吴蓓主编" type="notes_area,notes" />
                <line name="获得方式项" value="ISBN 978-7-5540-1024-2 (精装) : CNY48.00" type="resource_identifier_area" />
                <line name="提要文摘" value="本书对唐宋5位著名词人的生卒家世、生平、交游及作品加以翔实的考订，并编年排比，得年谱5种。主要内容包括：龙川年谱、放翁年谱、张元干年谱、张于湖年谱、刘后村年谱等。" type="summary" />
                <line name="主题分析" value="词人-年谱-中国-唐宋时期" type="subjects" />
                <line name="分类号" value="K825.6=4" type="classes" />
            </root>
             */
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strBiblio);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            XmlNode root = dom.DocumentElement;
            //XmlNode titleNode = root.SelectSingleNode("line[@type='title_area']");

            string title = DomUtil.GetElementAttr(root, "line[@type='title_area']", "value");
            item.Title = title;

            //// 处理题名等信息
            //string strOutMarcSyntax = "";
            //string strMARC = "";
            //int nRet = MarcUtil.Xml2Marc(strBiblio,
            //    false,
            //    "", // 自动识别 MARC 格式
            //    out strOutMarcSyntax,
            //    out strMARC,
            //    out strError);
            //if (nRet == -1)
            //    return -1;

            //MarcRecord marcRecord = new MarcRecord(strMARC);
            //string title = marcRecord.select("field[@name='200']/subfield[@name='a']").FirstContent;
            //item.Title = title;
            //ISBN = marcRecord.select("field[@name='010']/subfield[@name='a']").FirstContent;
            //reserItem.Author = marcRecord.select("field[@name='200']/subfield[@name='f']").FirstContent;




            return 0;

        ERROR1:
            //return -1;
            item.ErrorInfo = strError;
            return 0;

        }


        // 导出xml
        private int OutputXml(BorrowAnalysisReport report,
            out string xml,
            out string error)
        {
            error = "";
            xml = "";

            // 输出读者信息
            string patronXml = report.patronXml;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(patronXml);

            //为了防止输出的xml文件过大，过滤到了borrows/borrowHistory/outofReservations/reservations/overdues/face/fingerprint

            // 删除borrows节点，后面会输出清洗后在借
            XmlNode borrowsNode = dom.DocumentElement.SelectSingleNode("borrows");
            if (borrowsNode != null)
                dom.DocumentElement.RemoveChild(borrowsNode);

            // 删除borrowHistory节点，后面会输出清洗后在借
            XmlNode borrowHistoryNode = dom.DocumentElement.SelectSingleNode("borrowHistory");
            if (borrowHistoryNode != null)
                dom.DocumentElement.RemoveChild(borrowHistoryNode);

            //outofReservations
            XmlNode outofReservationsNode = dom.DocumentElement.SelectSingleNode("outofReservations");
            if (outofReservationsNode != null)
                dom.DocumentElement.RemoveChild(outofReservationsNode);

            //reservations
            XmlNode tempNode = dom.DocumentElement.SelectSingleNode("reservations");
            if (tempNode != null)
                dom.DocumentElement.RemoveChild(tempNode);

            //overdues
            tempNode = dom.DocumentElement.SelectSingleNode("overdues");
            if (tempNode != null)
                dom.DocumentElement.RemoveChild(tempNode);

            //face
            tempNode = dom.DocumentElement.SelectSingleNode("face");
            if (tempNode != null)
                dom.DocumentElement.RemoveChild(tempNode);

            //fingerprint
            tempNode = dom.DocumentElement.SelectSingleNode("fingerprint");
            if (tempNode != null)
                dom.DocumentElement.RemoveChild(tempNode);

            //palmprint
            tempNode = dom.DocumentElement.SelectSingleNode("palmprint");
            if (tempNode != null)
                dom.DocumentElement.RemoveChild(tempNode);

            // 拼出新的xml
            patronXml = "<patron>" + dom.DocumentElement.InnerXml + "</patron>";

            //======
            // 清洗后的在借
            string borrowsXml = "";
            // 清洗后的借阅历史
            string historyXml = "";
            if (report.borrowedItems != null && report.borrowedItems.Count > 0)
            {
                // 类型，0表示在借未还的，1表示借阅历史中的
                // 筛选在借图书，按借书时间倒序排
                var borrowList = report.borrowedItems.Where(x => x.Type == "0").OrderByDescending(x => x.BorrowDate.Time).ToList();
                if (borrowList.Count > 0)
                {
                    string tempXml = "";
                    foreach (BorrowedItem one in borrowList)
                    {
                        tempXml += one.DumpXml();
                    }
                    borrowsXml = "<borrows count='" + borrowList.Count + "'>" + tempXml + "</borrows>";
                }


                // 筛选借阅历史，按借书时间倒序排
                var historyList = report.borrowedItems.Where(x => x.Type == "1").OrderByDescending(x => x.BorrowDate.Time).ToList();
                if (historyList.Count > 0)
                {
                    string tempXml = "";
                    foreach (BorrowedItem one in historyList)
                    {
                        tempXml += one.DumpXml();
                    }
                    historyXml = "<borrowHistory count='" + historyList.Count + "'>" + tempXml + "</borrowHistory>";
                }
            }

            // 首次借书时间和借书总量（包括在借+历史）
            string firstBorrowDate = "";
            string totalBorrowedCount = "0";
            // 第一次借书时间
            if (report.borrowedItems != null && report.borrowedItems.Count > 0)
            {
                var list = report.borrowedItems.OrderBy(x => x.BorrowDate.Time).ToList();
                firstBorrowDate = list[0].BorrowDate.Date;
                totalBorrowedCount = list.Count.ToString();
            }
            //借阅汇总信息
            string borrowInfo = "<borrowInfo firstBorrowDate='" + firstBorrowDate + "' totalBorrowedCount='" + totalBorrowedCount + "'  timeRange='" + report.times + "'  title='" + report.commentTitle + "'/>";


            //=====
            // 按分类统计数量
            string clcXml = "";
            if (report.classGroups != null && report.classGroups.Count > 0)
            {
                int nCount = 0;

                // 其它的数量，当配置了仅显示前几行时，超过这几行的归到其它里
                int restCount = 0;

                // 循环输出每笔借阅记录
                foreach (GroupItem one in report.classGroups)
                {
                    if (nCount >= this._topCount)
                    {
                        restCount += one.items.Count;
                        continue;
                    }

                    clcXml += "<clcItem name='" + one.name + "' caption='" + one.caption + "' count='" + one.items.Count + "'/>";

                    // 数量加1
                    nCount++;
                }

                // 补一行其它
                if (restCount > 0)
                {
                    clcXml += "<clcItem name='其它' caption='' count='" + restCount + "'/>";
                }

                clcXml = "<clcGroup>" + clcXml + "</clcGroup>";
            }

            //======
            // 按年份统计数量
            string yearXml = "";
            if (report.yearGroups != null && report.yearGroups.Count > 0)
            {
                foreach (GroupItem one in report.yearGroups)
                {
                    yearXml += "<yearItem name='" + one.name + "'  count='" + one.items.Count + "'/>";
                }
                yearXml = "<yearGroup>" + yearXml + "</yearGroup>";
            }

            // 按季度统计数量
            string quarterXml = "";
            if (report.quarterGroups != null && report.quarterGroups.Count > 0)
            {
                foreach (GroupItem one in report.quarterGroups)
                {
                    quarterXml += "<quarterItem name='" + one.name + "'  count='" + one.items.Count + "'/>";
                }
                quarterXml = "<quarterGroup>" + quarterXml + "</quarterGroup>";
            }

            // 按月统计数量
            string monthXml = "";
            if (report.monthGroups != null && report.monthGroups.Count > 0)
            {
                foreach (GroupItem one in report.monthGroups)
                {
                    monthXml += "<monthItem name='" + one.name + "'  count='" + one.items.Count + "'/>";

                }
                monthXml = "<monthGroup>" + monthXml + "</monthGroup>";
            }


            //评语信息
            string commentXml = "<comment>" + report.comment + "</comment>";


            // 汇总xml
            xml = "<root>"
                + patronXml
                + borrowInfo
                + borrowsXml
                + historyXml
                + clcXml
                + yearXml
                + quarterXml
                + monthXml
                + commentXml
                + "</root>";


            // 以缩进格式显示

            string strOutXml = "";
            int nRet = DomUtil.GetIndentXml(xml,
                    false,
                    out strOutXml,
                    out error);
            if (nRet == -1)
                return -1;

            // 返回的xml
            xml = strOutXml;


            return 0;
        }


        // 输出报表
        // style:html/excel/xml
        // fileName:目标文件名
        public int OutputReport(BorrowAnalysisReport report,
            string style,
            out string content,
            out string error)
        {
            error = "";
            content = "";

            if (report == null)
            {
                error = "传入的report参数不能为null";
                return -1;
            }

            if (report.built == false)
            {
                error = "请先创建报表";
                return -1;
            }

            if (report.patron == null)
            {
                error = "读者对象不存在，没有可导出的数据。";
                return -1;
            }

            if (style == "xml")
            {
                return this.OutputXml(report,
                    out content,
                    out error);
            }
            else if (style == "html")
            {
                return this.OutputHtml(report,
                    out content,
                    out error);
            }
            

            return 0;

        }

        // 显示用长
        static string GetTimeString(XmlDocument dom)
        {
            XmlElement time = dom.DocumentElement.SelectSingleNode("time") as XmlElement;
            if (time == null)
                return "";
            return time.GetAttribute("seconds") + " 秒; " + time.GetAttribute("start") + " - " + time.GetAttribute("end");
        }

        private int OutputHtml(BorrowAnalysisReport report,
            out string html,
            out string error)
        {
            error = "";
            html = "";

            // 装载html模板，先layout，再加body。
            if (string.IsNullOrEmpty(this._dataDir) == true)
            {
                error = "在用户目录里缺少阅读分析使用的'ChargingAnalysis'目录，请联系系统管理员。";
                return -1;
            }

            string layoutFile = Path.Combine(this._dataDir, "layout.html");
            if (File.Exists(layoutFile) == false)
            {
                error = "'" + layoutFile + "'配置文件不存在，请联系系统管理员。";
                return -1;
            }


            string bodyHtml = "";
            string bodyFile = Path.Combine(this._dataDir, "body.html");
            if (File.Exists(bodyFile) == true)  // 允许不存在
            {
                //error = "'" + bodyFile + "'配置文件不存在，请联系系统管理员。";
                //goto ERROR1;

                // 把layout装载到内存
                StreamReader sBody = new StreamReader(bodyFile, Encoding.UTF8);
                bodyHtml = sBody.ReadToEnd();
            }

            // 把layout装载到内存
            StreamReader sLayout = new StreamReader(layoutFile, Encoding.UTF8);
            string layoutHtml = sLayout.ReadToEnd();

            // 把layout中的%body%替换为配置的body文件的内容。
            html = layoutHtml.Replace("%body%", bodyHtml);

            // 替换读者信息
            html = html.Replace("%patronBarcode%", report.patron.barcode);
            html = html.Replace("%name%", string.IsNullOrEmpty(report.patron.name) == false ? report.patron.name : "&nbsp;");
            html = html.Replace("%gender%", string.IsNullOrEmpty(report.patron.gender) == false ? report.patron.gender : "&nbsp;");
            html = html.Replace("%state%", string.IsNullOrEmpty(report.patron.state) == false ? report.patron.state : "&nbsp;");
            html = html.Replace("%department%", string.IsNullOrEmpty(report.patron.department) == false ? report.patron.department : "&nbsp;");
            html = html.Replace("%patronType%", string.IsNullOrEmpty(report.patron.readerType) == false ? report.patron.readerType : "&nbsp;");

            // 时间范围
            html = html.Replace("%times%", report.times);


            // todo后面把每一行输出到表格
            //string tableXml = "";
            //string tableXmlFile = Path.Combine(this._dataDir, "table.xml");
            //if (File.Exists(tableXmlFile) == true)  // 允许不存在
            //{
            //    // 装载到dom

            //}


            string onlyBorrowTable = "";
            string onlyHistoryTable = "";
            string allBorrowedTable = "";
            if (report.borrowedItems != null && report.borrowedItems.Count > 0)
            {
                // linq语句排序，先将在借还未的排在前面，再按借书时间倒序
                var borrowedList = report.borrowedItems.OrderBy(x => x.Type).ThenByDescending(x => x.BorrowDate.Time);//.BorrowTime);
                // 循环输出每笔借阅记录
                foreach (BorrowedItem one in borrowedList)
                {
                    string borrowTime = "";
                    if (one.BorrowDate != null)
                        borrowTime = one.BorrowDate.Time;

                    string returnTime = "";
                    if (one.ReturnDate != null)
                        returnTime = one.ReturnDate.Time;


                    string temp = "<tr>"
                        + "<td>" + one.ItemBarcode + "</td>"
                        + "<td>" + one.Title + "</td>"
                        + "<td>" + one.AccessNo + "</td>"
                        + "<td>" + borrowTime + "</td>"
                        + "<td>" + returnTime + "</td>"
                        + "</tr>";

                    allBorrowedTable += temp;

                    if (one.Type == C_Type_Borrow)
                        onlyBorrowTable += temp;
                    else if (one.Type == C_Type_History)
                        onlyHistoryTable += temp;
                }

                string titleTR = "<tr class='title'><td>册条码</td><td>题名和责任者</td><td>索取号</td><td>借书时间</td><td>还书时间</td></tr>";

                if (string.IsNullOrEmpty(onlyBorrowTable) == false)
                    onlyBorrowTable = "<table class='statisTable'>" + titleTR + onlyBorrowTable + "</table>";

                if (string.IsNullOrEmpty(onlyHistoryTable) == false)
                    onlyHistoryTable = "<table class='statisTable'>" + titleTR + onlyHistoryTable + "</table>";

                if (string.IsNullOrEmpty(allBorrowedTable) == false)
                    allBorrowedTable = "<table class='statisTable'>" + titleTR + allBorrowedTable + "</table>";
            }

            html = html.Replace("%onlyBorrowTable%", onlyBorrowTable);
            html = html.Replace("%onlyHistoryTable%", onlyHistoryTable);
            html = html.Replace("%allBorrowedTable%", allBorrowedTable);

            string firstBorrowDate = "";
            string borrowedCount = "0";
            // 第一次借书时间
            if (report.borrowedItems != null && report.borrowedItems.Count > 0)
            {
                var list2 = report.borrowedItems.OrderBy(x => x.BorrowDate.Time).ToList();
                firstBorrowDate = list2[0].BorrowDate.Date;
                borrowedCount = list2.Count.ToString();
            }
            //首次借阅时间为%firstBorrowDate%，到目前共借阅图书%borrowedCount%册。
            html = html.Replace("%firstBorrowDate%", firstBorrowDate);
            html = html.Replace("%borrowedCount%", borrowedCount);

            // 按分类统计数量
            string clcTable = "";
            if (report.classGroups != null && report.classGroups.Count > 0)
            {
                int nCount = 0;

                // 其它
                int restCount = 0;

                // 循环输出每笔借阅记录
                foreach (GroupItem one in report.classGroups)
                {

                    if (nCount >= this._topCount)
                    {
                        restCount += one.items.Count;
                        continue;
                    }

                    string temp = "<tr>"
                        + "<td>" + one.name + "</td>"
                        + "<td>" + one.caption + "</td>"
                        + "<td>" + one.items.Count + "</td>"
                        + "</tr>";

                    clcTable += temp;

                    // 数量加1
                    nCount++;
                }

                // 补一行其它
                if (restCount > 0)
                {
                    clcTable += "<tr>"
                        + "<td colspan='2'>其它</td>"
                        + "<td>" + restCount + "</td>"
                        + "</tr>";
                }

                string titleTR = "<tr class='title'><td>分类</td><td>分类名称</td><td>数量</td></tr>";

                clcTable = "<table class='statisTable'>" + titleTR + clcTable + "</table>";
            }
            html = html.Replace("%clcTable%", clcTable);


            // 按年份统计数量
            string yearTable = "";
            if (report.yearGroups != null && report.yearGroups.Count > 0)
            {
                foreach (GroupItem one in report.yearGroups)
                {
                    string temp = "<tr>"
                        + "<td>" + one.name + "</td>"
                        + "<td>" + one.items.Count + "</td>"
                        + "</tr>";

                    yearTable += temp;
                }
                string titleTR = "<tr class='title'><td class='label'>年份</td><td>借阅量</td></tr>";
                yearTable = "<table class='statisTable'>" + titleTR + yearTable + "</table>";
            }
            html = html.Replace("%yearTable%", yearTable);


            // 按季度统计数量
            string quarterTable = "";
            if (report.quarterGroups != null && report.quarterGroups.Count > 0)
            {
                foreach (GroupItem one in report.quarterGroups)
                {
                    string temp = "<tr>"
                        + "<td>" + one.name + "</td>"
                        + "<td>" + one.items.Count + "</td>"
                        + "</tr>";

                    quarterTable += temp;
                }
                string titleTR = "<tr class='title'><td class='label'>季度</td><td>借阅量</td></tr>";
                quarterTable = "<table class='statisTable'>" + titleTR + quarterTable + "</table>";
            }
            html = html.Replace("%quarterTable%", quarterTable);

            // 按月统计数量
            string monthTable = "";
            if (report.monthGroups != null && report.monthGroups.Count > 0)
            {
                foreach (GroupItem one in report.monthGroups)
                {
                    string temp = "<tr>"
                        + "<td>" + one.name + "</td>"
                        + "<td>" + one.items.Count + "</td>"
                        + "</tr>";

                    monthTable += temp;
                }
                string titleTR = "<tr class='title'><td class='label'>月份</td><td>借阅量</td></tr>";
                monthTable = "<table class='statisTable'>" + titleTR + monthTable + "</table>";
            }
            html = html.Replace("%monthTable%", monthTable);


            // 馆长评语
            //%comment%
            html = html.Replace("%comment%", report.comment);

            return 0;
        }
    }


    public class BorrowAnalysisReport
    {

        // 时间范围
        public string times = "";

        // 读者对象
        public Patron patron = null;

        public string patronXml = "";

        // 是否已经创建好了数据
        public bool built = false;


        // 借书记录集合
        public List<BorrowedItem> borrowedItems = new List<BorrowedItem>();


        public List<GroupItem> classGroups;  // 分类统计
        public List<GroupItem> yearGroups;// 按年统计
        public List<GroupItem> quarterGroups;// 按季度统计
        public List<GroupItem> monthGroups;// 按月统计类统计

        // 根据规则算出来的称号
        internal string commentTitle;

        // 馆长评号
        public string comment { get; set; }

    }

    /// <summary>
    /// 读者对象
    /// </summary>
    public class Patron 
    {
        internal string recPath;
        internal string refID;
        internal string displayName;
        internal string comment;

        // 证条码号
        public string barcode { get; set; }

        // 读者类型
        public string readerType { get; set; }

        // 姓名
        public string name { get; set; }

        // 性别
        public string gender { get; set; }

        // 单位
        public string department { get; set; }

        // 电话
        public string tel { get; set; }

        // 馆代码
        public string libraryCode { get; set; }

        //// 显示名
        //public string displayName { get; set; }

        // 出生日期
        public string dateOfBirth { get; set; }

        // 证号 2008/11/11
        public string cardNumber { get; set; }

        // 身份证号
        public string idCardNumber { get; set; }

        // 职务
        public string post { get; set; }

        // 地址
        public string address { get; set; }


        // email 
        public string email { get; set; }


        // 证状态
        public string state { get; set; }

        // 发证日期
        public string createDate { get; set; }
        // 证失效期
        public string expireDate { get; set; }

        // 租金 2008/11/11
        public string hire { get; set; }

        // 押金 2008/11/11
        public string foregift { get; set; }

        ////违约交费数量
        //public int OverdueCount { get; set; }
        //public string OverdueCountHtml { get; set; }


        ////在借 超期数量
        //public int BorrowCount { get; set; }
        //public int CaoQiCount { get; set; }
        //public string BorrowCountHtml { get; set; }

        ////预约 到书数量
        //public int ReservationCount { get; set; }
        //public int ArrivedCount { get; set; }
        //public string ReservationCountHtml { get; set; }
    }



    public class GroupItem
    {
        public string name { get; set; }

        public string caption { get; set; }
        public int totalCount { get; set; }

        public List<BorrowedItem> items { get; set; }
    }

    public class RoleItem
    {
        internal int borrowCount;

        internal string title { get; set; }

    }
}
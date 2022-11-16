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

namespace DigitalPlatform.ChargingAnalysis
{
    public class ChargingAnalysisService
    {
        #region 单一实例

        static ChargingAnalysisService _instance;

        // 构造函数
        private ChargingAnalysisService()
        {
            //this.dp2ServerUrl = Properties.Settings.Default.dp2ServerUrl;
            //this.dp2Username = Properties.Settings.Default.dp2Username;
            //this.dp2Password = Properties.Settings.Default.dp2Password;
            //this._libraryChannelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }
        private static object _lock = new object();
        static public ChargingAnalysisService Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new ChargingAnalysisService();
                    }
                }
                return _instance;
            }
        }

        Hashtable _clcHT = new Hashtable();

        // todo要改为有返回出错信息
        // chargingAnalysisDataDir：阅读分析的数据目录，里面有html配置文件
        public int Init(string dataDir,
            string serverUrl, string userName, string passowrd, string loginParameters,
            out string strError)
        {
            strError = "";

            // 阅读分析的数据目录
            this._dataDir = dataDir;

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
                    string[] a= line.Split(new char[] { '\t' });
                    _clcHT.Add(a[0],a[1]);
                    //sb.Append("<p>").Append(line).Append("</p>").AppendLine();
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


        // 分类统计显示前面的多少行
        int _topCount = 5;

        // 读者对象
        Patron _patron = null;

        // 借书记录集合
        List<BorrowedItem> _borrowedItems = new List<BorrowedItem>();


        // 创建内存结构
        // patronBarcode：读者证条码
        // times：操作时间范围 2022/11/01~2022/11/01 11:00
        public int Build(CancellationToken token,
            string patronBarcode,
            string times,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            // 选将创建完成的变量置为false
            this._built = false;

            // 先清空读者借阅历史内存集合
            this._borrowedItems.Clear();

            RestChannel channel = this.GetChannel();
            try
            {
                // 获取读者信息
                string[] results = null;
                string strRecPath = "";
                lRet = channel.GetReaderInfo(//null,
                    patronBarcode, //读者卡号,
                    "advancexml",
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

                // 把读者xml解析后存到内存中
                this._patron = this.ParsePatronXml(dom, strRecPath);

                // 把在借信息存到内存数组中，目前与借阅历史存在一起
                XmlNodeList borrowNodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                foreach (XmlNode borrowNode in borrowNodes)
                {
                    BorrowedItem item = new BorrowedItem(patronBarcode, borrowNode);
                    this._borrowedItems.Add(item);
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
                            this._borrowedItems.Add(item);
                        }
                    }

                    // 开始序号增加，为下一轮获取准备
                    start += itemWarpperList.Length;

                    // 获取完记录时，退出循环
                    if (start >= totalCount)
                        break;
                }


                // 获取册信息与书目信息，用同一根通道即可
                foreach (BorrowedItem item in this._borrowedItems)
                {
                    // 外面停止
                    token.ThrowIfCancellationRequested();


                    lRet = this.GetItemInfo(channel, item.ItemBarcode, item, out strError);
                    if (lRet == -1)
                    {
                        // todo 抛出异常？暂时不处理
                    }
                }

                // 按中图法聚合，todo是按分类排序，还是按数量排序？目前先按分类排序。后面看用户反馈。
                this._classList = this._borrowedItems.GroupBy(
                    x => new { x.BigClass },
                    (key, item_list) => new ClassGroup
                    {
                        clcClass = key.BigClass,
                        clcName= GetClassCaption(key.BigClass),
                        Items = new List<BorrowedItem>(item_list)
                    }).OrderBy(o => o.clcClass).ToList();



                //==结束==
                this._built = true;
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

            var o=this._clcHT[clcclass];
            if (o == null)
                return "";  //无对应的中文名称

            return  (string)o;
        }


        public List<ClassGroup> _classList;

        public class ClassGroup
        {
            public string clcClass { get; set; }

            public string clcName { get; set; }
            public int totalCount { get; set; }

            public List<BorrowedItem> Items { get; set; }
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
        public int GetItemInfo(RestChannel channel,
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

        public const string C_Type_Borrow = "0";
        public const string C_Type_History = "1";



        // 是否已经创建好了数据
        bool _built = false;

        // 输出报表
        // style:html/excel/xml
        // fileName:目标文件名
        public int OutputReport(string style,
            out string content,
            out string error)
        {
            error = "";
            content = "";

            if (this._built == false)
            {
                error = "请先创建报表";
                return -1;
            }

            if (this._patron == null)
            {
                error = "读者对象不存在，没有可导出的数据。";
                return -1;
            }

            // 装载html模板，先layout，再加body。
            if (string.IsNullOrEmpty(this._dataDir) == true)
            {
                error = "在用户目录里缺少阅读分析使用的'ChargingAnalysis'目录，请联系系统管理员。";
                goto ERROR1;
            }

            string layoutFile = Path.Combine(this._dataDir, "layout.html");
            if (File.Exists(layoutFile) == false)
            {
                error = "'" + layoutFile + "'配置文件不存在，请联系系统管理员。";
                goto ERROR1;
            }

            string bodyFile = Path.Combine(this._dataDir, "body.html");
            if (File.Exists(bodyFile) == false)
            {
                error = "'" + bodyFile + "'配置文件不存在，请联系系统管理员。";
                goto ERROR1;
            }

            // 把layout装载到内存
            StreamReader sLayout = new StreamReader(layoutFile, Encoding.UTF8);
            string layoutHtml = sLayout.ReadToEnd();

            // 把layout装载到内存
            StreamReader sBody = new StreamReader(bodyFile, Encoding.UTF8);
            string bodyHtml = sBody.ReadToEnd();

            // 把layout中的%body%替换为配置的body文件的内容。
            string html = layoutHtml.Replace("%body%", bodyHtml);

            // 替换读者信息
            html = html.Replace("%patronBarcode%", this._patron.barcode);
            html = html.Replace("%name%", this._patron.name);
            html = html.Replace("%gender%", this._patron.gender);
            html = html.Replace("%department%", this._patron.department);

            // 在借 和 借阅历史
            //if (this._borrowedItems == null || this._borrowedItems.Count == 0)
            //{
            //    error = "选择的日期范围该读者没有借阅记录。";
            //    goto ERROR1;
            //}
            string onlyBorrowTable = "";
            string onlyHistoryTable = "";
            string allBorrowedTable = "";
            if (this._borrowedItems != null && this._borrowedItems.Count > 0)
            {
                // linq语句排序，先将在借还未的排在前面，再按借书时间倒序
                var borrowedList = this._borrowedItems.OrderBy(x => x.Type).ThenByDescending(x => x.BorrowTime);
                // 循环输出每笔借阅记录
                foreach (BorrowedItem one in borrowedList)
                {
                    string temp = "<tr>"
                        + "<td>" + one.ItemBarcode + "</td>"
                        + "<td>" + one.Title + "</td>"
                        + "<td>" + one.AccessNo + "</td>"
                        + "<td>" + one.BorrowTime + "</td>"
                        + "<td>" + one.ReturnTime + "</td>"
                        + "</tr>";

                    allBorrowedTable += temp;

                    if (one.Type == C_Type_Borrow)
                        onlyBorrowTable += temp;
                    else if (one.Type == C_Type_History)
                        onlyHistoryTable += temp;
                }

                onlyBorrowTable = "<table style='border: 1px solid green'>" + onlyBorrowTable + "</table>";
                onlyHistoryTable = "<table style='border: 1px solid yellow'>" + onlyHistoryTable + "</table>";
                allBorrowedTable = "<table  style='border: 1px solid red'>" + allBorrowedTable + "</table>";
            }

            html = html.Replace("%onlyBorrowTable%", onlyBorrowTable);
            html = html.Replace("%onlyHistoryTable%", onlyHistoryTable);
            html = html.Replace("%allBorrowedTable%", allBorrowedTable);

            string firstBorrowDate = "";
            string borrowedCount = "0";
            // 第一次借书时间
            if (this._borrowedItems != null && this._borrowedItems.Count > 0)
            {
                var list2 = this._borrowedItems.OrderBy(x => x.BorrowTime).ToList();
                firstBorrowDate = list2[0].BorrowTime;
                borrowedCount = list2.Count.ToString();
            }
            //首次借阅时间为%firstBorrowDate%，到目前共借阅图书%borrowedCount%册。
            html = html.Replace("%firstBorrowDate%", firstBorrowDate);
            html = html.Replace("%borrowedCount%", borrowedCount);

            //// 按分类统计数量
            string clcTable = "";
            if (this._classList != null && this._classList.Count > 0)
            {

                int nCount = 0;

                int restCount = 0;

                // 循环输出每笔借阅记录
                foreach (ClassGroup one in this._classList)
                {

                    if (nCount >= this._topCount)
                    {
                        restCount += one.Items.Count;
                        continue;
                    }

                    string temp = "<tr>"
                        + "<td>" + one.clcClass + "</td>"
                        + "<td>" + one.clcName + "</td>"
                        + "<td>" + one.Items.Count + "</td>"
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

                clcTable = "<table style='border: 1px solid green'>" + clcTable + "</table>";
            }
            html = html.Replace("%clcTable%", clcTable);


            //// 按年份统计数量
            //html = html.Replace("%yearTable%", patron.yearTable);

            content = html;
            return 0;

        ERROR1:
            return -1;
        }

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
}
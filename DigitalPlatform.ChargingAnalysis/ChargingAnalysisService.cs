using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Init(string serverUrl, string userName, string passowrd, string loginParameters)
        {
            this.dp2ServerUrl = serverUrl;
            this.dp2Username = userName;
            this.dp2Password = passowrd;
            this.dp2LoginParameters = loginParameters;
            this._channelPool.BeforeLogin -= channelPool_BeforeLogin;
            this._channelPool.BeforeLogin += channelPool_BeforeLogin;
        }

        #endregion

        #region 关于通道
        // 通道池
        RestChannelPool _channelPool = new RestChannelPool();

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
                // 把xml装载到内存中
                this._patron=this.ParsePatronXml(strPatronXml, strRecPath);

                

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

                            // 获取索取号，用同一根通道即可
                            lRet = this.GetItemInfo(channel, item.ItemBarcode, item, out strError);
                            if (lRet == -1)
                            {
                                // todo 抛出异常？暂时不处理
                            }
                        }
                    }

                    // 开始序号增加，为下一轮获取准备
                    start += itemWarpperList.Length;

                    // 获取完记录时，退出循环
                    if (start >= totalCount)
                        break;
                }

                //==结束==

                return 0;

            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        }

        // 解析读者xml到内存对象
        public Patron ParsePatronXml(string strPatronXml,
            string recPath)
        {
            // 取出个人信息
            Patron patron = new Patron();
            patron.recPath = recPath;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strPatronXml);

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

        private string RemoveWeiXinId(string email)
        {
            //<email>test@163.com,123,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg,weixinid:o4xvUvnLTg6NnflbYdcS-sxJCGFo,weixinid:testid</email>
            string[] emailList = email.Split(new char[] { ',' });
            string clearEmail = "";
            for (int i = 0; i < emailList.Length; i++)
            {
                string oneEmail = emailList[i].Trim();
                if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == WeiXinConst.C_WeiXinIdPrefix)
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

            // 取出大类
            string bigClass = "";
            if (string.IsNullOrEmpty(accessNo) == true)
            {
                bigClass = "[空]";
            }
            else
            {
                bigClass = accessNo.Substring(0, 1);
            }

            item.AccessNo = accessNo;
            item.BigClass = bigClass;

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
    


        // 输出报表
        public string OutputReport(string style, string fileName)
        {
            string report = "";

            if (this._borrowedItems == null || this._borrowedItems.Count == 0)
            {
                report = "选择的日期范围该读者没有借阅记录。";
                return report;
            }

            
            // linq语句排序，按借书日期倒序
            var result = this._borrowedItems.OrderByDescending(x => x.BorrowDay);



            // 循环输出每笔借阅记录
            foreach (BorrowedItem one in this._borrowedItems)
            {
                report += one.Dump() + "\r\n";
            }


            return report;
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
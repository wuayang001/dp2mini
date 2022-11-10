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

        // 借书记录集合
        List<BorrowedItem> _borrowedItem = new List<BorrowedItem>();


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
            this._borrowedItem.Clear();

            RestChannel channel = this.GetChannel();
            try
            {
                // 获取读者信息
                string[] results = null;
                lRet = channel.GetReaderInfo(//null,
                    patronBarcode, //读者卡号,
                    "advancexml",
                    out results,
                    out strError);
                if (lRet == -1)
                {
                    // todo要不要再丰富一下错误信息
                    return -1;
                }

                XmlDocument dom = new XmlDocument();
                string strReaderXml = results[0];
                try
                {
                    dom.LoadXml(strReaderXml);
                }
                catch (Exception ex)
                {
                    strError = "将读者xml加载到dom出错：" + ExceptionUtil.GetDebugText(ex);
                    return -1;
                }

                //<name>王一诺</name> 
                //<department>1501</department> 
                //<gender>男</gender>
                //XmlNode root = dom.DocumentElement;
                //patron = new Patron();
                //patron.name = DomUtil.GetElementText(root, "name");
                //patron.department = DomUtil.GetElementText(root, "department");
                //patron.gender = DomUtil.GetElementText(root, "gender");

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
                            this._borrowedItem.Add(item);

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

            if (this._borrowedItem == null || this._borrowedItem.Count == 0)
            {
                report = "选择的日期范围该读者没有借阅记录。";
                return report;
            }

            
            // linq语句排序，按借书日期倒序
            var result = this._borrowedItem.OrderByDescending(x => x.BorrowDay);



            // 循环输出每笔借阅记录
            foreach (BorrowedItem one in this._borrowedItem)
            {
                report += one.Dump() + "\r\n";
            }


            return report;
        }

    }
}
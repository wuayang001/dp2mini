using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
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
        public void Build(string patronBarcode, string times)
        {
            // 先清空内存集合
            this._borrowedItem.Clear();

            RestChannel channel = this.GetChannel();
            try
            {
                ChargingItemWrapper[] itemWarpperList = null;

                long totalCount = 0;
                long start = 0;
                long preCount = 2;   // 每次固定取几笔 //lHitCount;
                long lRet = 0;
                string strError = "";

                // 从结果集中取出册记录
                for (; ; )
                {
                    //Application.DoEvents(); // 出让界面控制权

                    //token.ThrowIfCancellationRequested();
                    //SearchBiblioResponse response 
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

                    if (itemWarpperList != null && itemWarpperList.Length > 0)
                    {
                        // 增加到内存集合中
                        foreach (ChargingItemWrapper one in itemWarpperList)
                        {
                            BorrowedItem item = new BorrowedItem(one);
                            this._borrowedItem.Add(item);

                            // 获取索取号，用同一根通道即可


                        }



                    }

                    start += itemWarpperList.Length;

                    // 获取完记录时，退出循环
                    if (start >= totalCount)
                        break;
                }

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
                "xml",
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


            //获取索取号
            string accessNo = DomUtil.GetElementInnerText(itemDom.DocumentElement, "accessNo");

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


            // 处理题名等信息
            string strOutMarcSyntax = "";
            string strMARC = "";
            int nRet = MarcUtil.Xml2Marc(strBiblio,
                false,
                "", // 自动识别 MARC 格式
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            MarcRecord marcRecord = new MarcRecord(strMARC);
            string title = marcRecord.select("field[@name='200']/subfield[@name='a']").FirstContent;
            item.Title = title;
        //ISBN = marcRecord.select("field[@name='010']/subfield[@name='a']").FirstContent;
        //reserItem.Author = marcRecord.select("field[@name='200']/subfield[@name='f']").FirstContent;


        ERROR1:
            return -1;

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



            // 循环输出每笔借阅记录
            foreach (BorrowedItem one in this._borrowedItem)
            {
                report += one.Dump() + "\r\n";
            }


            return report;
        }

    }
}
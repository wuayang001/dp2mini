using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;

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

        public void Init(string serverUrl,string userName,string passowrd)
        {
            this.dp2ServerUrl = serverUrl;
            this.dp2Username = userName;
            this.dp2Password = passowrd;
        }

        #endregion

        #region 关于通道
        // 通道池
        RestChannelPool _channelPool = new RestChannelPool();

        public string dp2ServerUrl { get; set; }
        public string dp2Username { get; set; }
        public string dp2Password { get; set; }
        void channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (string.IsNullOrEmpty(this.dp2Username))
            {
                e.Cancel = true;
                e.ErrorInfo = "尚未登录";
            }

            e.LibraryServerUrl = this.dp2ServerUrl;
            e.UserName = this.dp2Username;
            e.Parameters = "type=worker,client=dp2analysis|0.01";
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

        // 借阅日志数组
        List<ChargingItemWrapper> _chargeItems=new List<ChargingItemWrapper>();


        // 创建内存结构
        // patronBarcode：读者证条码
        // times：操作时间范围 2022/11/01~2022/11/01 11:00
        public void Build(string patronBarcode, string times)
        {

            // 先清空内存集合
            this._chargeItems.Clear(); 

            RestChannel channel = this.GetChannel();
            try
            {
                // todo 要做成死循环
                string strStart = "";// this.textBox_searchCharging_start.Text.Trim();
                if (strStart == "")
                    strStart = "0";
                long start = Convert.ToInt64(strStart);

                string strCount = "";// this.textBox_searchCharging_count.Text.Trim();
                if (strCount == "")
                    strCount = "-1";
                long count = Convert.ToInt64(strCount);

                ChargingItemWrapper[] itemWarpperList = null;

                //SearchBiblioResponse response 
                long lRet = channel.SearchCharging(patronBarcode,//this.textBox_SearchCharging_patronBarcode.Text.Trim(),
                    times,
                    "return,lost", //this.textBox_searchCharging_actions.Text.Trim(),
                    "",//this.textBox_searchCharging_order.Text.Trim(),
                    start,
                    count,
                    out itemWarpperList,
                    out string strError); ;
                if (lRet == -1)
                {
                    throw new Exception(strError); //直接抛出异常
                }



                //this.textBox_result.Text = "count:" + lRet;

                if (itemWarpperList != null && itemWarpperList.Length > 0)
                {
                    // 增加到内存集合中
                    this._chargeItems.AddRange(itemWarpperList);

                    //string temp = "";
                    //foreach (ChargingItemWrapper one in itemWarpperList)
                    //{
                    //    temp += one.Item.ItemBarcode + "\r\n";
                    //}

                    //this.textBox_result.Text += temp;
                }
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        }


        // 输出报表
        public string OutputReport(string style,string fileName)
        {
            string report = "";

            if (this._chargeItems != null && this._chargeItems.Count > 0)
            {
                foreach (ChargingItemWrapper one in this._chargeItems)
                {
                    report+= one.Item.ItemBarcode +"-"+one.Item.Operator+ "\n";

                }
            }
            
        
            return report;
        }

    }
}
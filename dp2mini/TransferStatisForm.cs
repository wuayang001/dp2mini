using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Printing;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Forms;
using DigitalPlatform.LibraryRestClient;
using System.Collections.Generic;
using DigitalPlatform.IO;
using DigitalPlatform;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using DigitalPlatform.LibraryClient;

namespace dp2mini
{
    public partial class TransferStatisForm : Form
    {
        // mid父窗口
        MainForm _mainForm = null;

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public const string C_State_outof = "outof";
        public const string C_State_arrived = "arrived";

        public Hashtable ItemHt = new Hashtable();

        /// <summary>
        ///  构造函数
        /// </summary>
        public TransferStatisForm()
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

        /// <summary>
        /// 检索预约到书记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_search_Click(object sender, EventArgs e)
        {
            //时间范围
            string startDate = this.dateTimePicker_start.Value.ToString("yyyyMMdd");
            string endDate = this.dateTimePicker_end.Value.ToString("yyyyMMdd");

            // 批次号
            string batchNo = this.textBox_batchNo.Text;


            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 开一个新线程
            Task.Run(() =>
            {
                doSearch(this._cancel.Token,
                    startDate,
                    endDate,
                    batchNo);
            });
        }

        /// <summary>
        /// 检索做事的函数
        /// </summary>
        /// <param name="token"></param>
        private void doSearch(CancellationToken token,
            string startDate,
            string endDate,
            string batchNo)
        {
            string strError = "";

            // 记下原来的光标
            Cursor oldCursor = Cursor.Current;

            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                // 设置按钮状态
                EnableControls(false);

                //清空界面数据
                this.ClearInfo();

                // 鼠标设为等待状态
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
            }
            ));

            RestChannel channel = this._mainForm.GetChannel();
            try
            {

                int nRet = OperLogLoader.MakeLogFileNames(startDate,
                    endDate,
                    false,  // 是否包含扩展名 ".log"
                    out List<string> dates,
                    out string strWarning,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                {
                    //ProgressEstimate estimate = new ProgressEstimate();

                    OperLogLoader loader = new OperLogLoader
                    {
                        Channel = channel,
                        Stop = null, //  this.Progress;
                                     // loader.owner = this;
                        Estimate = null,// estimate,
                        Dates = dates,
                        Level = 0,  // 2019/7/23 注：2 最简略。不知何故以前用了这个级别。缺点是 oldRecord 元素缺乏 InnerText
                        AutoCache = false,
                        CacheDir = "",
                        LogType = LogType.OperLog,//logType,
                        Filter = "",// setReaderInfo",
                        ServerVersion = ""//serverVersion
                    };

                    //TimeSpan old_timeout = channel.Timeout;
                    //channel.Timeout = new TimeSpan(0, 2, 0);   // 二分钟

                    //loader.Prompt += Loader_Prompt;
                    try
                    {
                        // int nRecCount = 0;

                        string strLastItemDate = "";
                        long lLastItemIndex = -1;
                        // TODO: 计算遍历耗费的时间。如果太短了，要想办法让调主知道这一点，放缓重新调用的节奏，以避免 CPU 和网络资源太高
                        foreach (OperLogItem item in loader)
                        {
                            token.ThrowIfCancellationRequested();

                            //if (stop != null)
                            //    stop.SetMessage("正在同步 " + item.Date + " " + item.Index.ToString() + " " + estimate.Text + "...");

                            if (string.IsNullOrEmpty(item.Xml) == true)
                                goto CONTINUE;

                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(item.Xml);
                            }
                            catch (Exception ex)
                            {
                                throw new ChannelException(channel.ErrorCode, strError);
                            }

                            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                            //if (strOperation == "setEntity")
                            {
                                this.Invoke((Action)(() =>
                                {
                                   nRet= this.LoadLog(dom, out strError);
                                    if (nRet == -1)
                                        throw new Exception(strError);
                                }
                                ));
                            }


                            if (nRet == -1)
                            {
                                strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;
                                throw new Exception(strError);

                                //throw new ChannelException(channel.ErrorCode, strError);
                            }

                        // lProcessCount++;
                        CONTINUE:
                            // 便于循环外获得这些值
                            strLastItemDate = item.Date;
                            lLastItemIndex = item.Index + 1;

                            // index = 0;  // 第一个日志文件后面的，都从头开始了
                        }
                        //// 记忆
                        //strLastDate = strLastItemDate;
                        //last_index = lLastItemIndex;
                    }
                    finally
                    {
                        //loader.Prompt -= Loader_Prompt;
                        //channel.Timeout = old_timeout;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                // 设置按置状态
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                    this.Cursor = oldCursor;

                    this._mainForm.ReturnChannel(channel);
                }
                ));
            }

        ERROR1:
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(strError);
            }
            ));
        }

        public int LoadLog(XmlDocument dom,out string error)
        {
            error = "";

            string action = DomUtil.GetElementText(dom.DocumentElement, "action");

            this.textBox1.Text += action+"\r\n";

            return 0;
        }

        /// <summary>
        /// 清空界面数据
        /// </summary>
        public void ClearInfo()
        {
            this.textBox1.Text = "";
            // 清空listview
            //this.listView_results.Items.Clear();
            //this.listView_outof.Items.Clear();

            ////设置父窗口状态栏参数
            //this._mainForm.SetStatusMessage("");
        }

        /// <summary>
        /// 设置控件是否可用
        /// </summary>
        /// <param name="bEnable"></param>
        void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            this.button_stop.Enabled = !(bEnable);
        }

        /// <summary>
        /// 停止检索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_stop_Click(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }




    }



}

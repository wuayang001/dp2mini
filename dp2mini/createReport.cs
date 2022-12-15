using DigitalPlatform.ChargingAnalysis;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using xml2html;

namespace dp2mini
{
    public partial class createReport : Form
    {
        public createReport()
        {
            InitializeComponent();
        }

        #region 内部函数

        // 报表目录
        public string OutputDir
        {
            get
            {
                return this.textBox_outputDir.Text.Trim();
            }
            set
            {
                this.textBox_outputDir.Text = value;
            }
        }


        // 获取输入参数
        private int GetInput(out string patronBarcodes,
            out string startDate,
            out string endDate,
            out string dir)
        {

            //时间范围
            startDate = this.dateTimePicker_start.Value.ToString("yyyy/MM/dd");
            endDate = this.dateTimePicker_end.Value.ToString("yyyy/MM/dd");

            patronBarcodes = "";
            // 输出目录
            dir = this.textBox_outputDir.Text.Trim();
            if (string.IsNullOrEmpty(dir) == true)
            {
                MessageBox.Show(this, "尚未选择报表输出目录。");
                return -1;
            }
            if (Directory.Exists(dir) == false)  // 如果目录不存在，则创建一个新目录
                Directory.CreateDirectory(dir);

            // 如果目录不是空目录，提醒。
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            if (dirInfo.GetFiles("*.xml").Length > 0
                || dirInfo.GetFiles("*.html").Length > 0)  // todo，后面里面可能会放一个cfg目录，里面是配置文件
            {
                MessageBox.Show(this, "报表目录[" + dir + "]里，存在已创建好的报表文件，新创建报表时请选择一个空目录。");
                return -1;
            }


            // 证条码号，可能多个
            patronBarcodes = this.textBox_patronBarcode.Text.Trim();
            if (string.IsNullOrEmpty(patronBarcodes) == true)
            {
                MessageBox.Show(this, "请先输入读者证条码号。");
                return -1;
            }

            // 拆分证条码号，每个号码一行
            patronBarcodes = patronBarcodes.Replace("\r\n", "\n");
            string[] patronBarcodeList = patronBarcodes.Split(new char[] { '\n' });
            if (patronBarcodeList.Length == 0)
            {
                MessageBox.Show(this, "请先输入读者证条码号2。");
                return -1;
            }

            return 0;
        }


        #endregion


        #region 快速设置时间
        delegate void Delegate_QuickSetFilenames(Control control);
        void QuickSetFilenames(Control control)
        {
            string strStartDate = "";
            string strEndDate = "";

            string strName = control.Text.Replace(" ", "").Trim();

            if (strName == "今天")
            {
                DateTime now = DateTime.Now;

                strStartDate = DateTimeUtil.DateTimeToString8(now);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                // strEndDate = DateTimeUtil.DateTimeToString8(start + new TimeSpan(7, 0,0,0));
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7 - 1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                MessageBox.Show(this, "无法识别的周期 '" + strName + "'");
                return;
            }

            this.dateTimePicker_start.Value = DateTimeUtil.Long8ToDateTime(strStartDate);
            this.dateTimePicker_end.Value = DateTimeUtil.Long8ToDateTime(strEndDate);
        }

        private void comboBox_quickSetFilenames_SelectedIndexChanged(object sender, EventArgs e)
        {
            Delegate_QuickSetFilenames d = new Delegate_QuickSetFilenames(QuickSetFilenames);
            this.BeginInvoke(d, new object[] { sender });
        }

        #endregion

        #region 按钮事件

        // 选择目录
        private void button_selectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = this.textBox_outputDir.Text;

            DialogResult result = dlg.ShowDialog();

            // todo记住上次选择的目录

            if (result != DialogResult.OK || string.IsNullOrEmpty(dlg.SelectedPath) == true)
            {
                //MessageBox.Show(this, "取消");
                return;
            }

            this.textBox_outputDir.Text = dlg.SelectedPath;
        }

        // 选择目录发生变化
        private void textBox_outputDir_TextChanged(object sender, EventArgs e)
        {
            // 先把除选择文件外的所有输入框和按钮置为不可用状态

            // 开始，停止按钮
            this.button_plan.Enabled = false;
            this.button_stop.Enabled = false;

            // 先统一将 证条码输入框与选择按钮 设为不可用
            this.SetBarcodeDateCtrol(false);
            this.textBox_patronBarcode.Text = "";
            this.dateTimePicker_start.Text = DateTime.Now.ToString("yyyy/MM/dd");
            this.dateTimePicker_end.Text = DateTime.Now.ToString("yyyy/MM/dd");
            this.comboBox_quickSetFilenames.Text = "";

            // 清空进度条和信息
            this.progressBar1.Value = 0;
            this.label_info.Text = "";

            // ==根据是否存在计划文件，设置各控件是否可用
            string planFile = this.OutputDir + "\\plan.txt";
            if (File.Exists(planFile) == false)  // 无plan文件，开始创建报表
            {
                // 无plan文件，条码输入框和日期范围可用
                this.SetBarcodeDateCtrol(true); 

                // 按钮名称设为 开始...
                this.button_plan.Enabled = true;
                this.button_plan.Text = "开始创建借阅报表";
            }
            else   // 存在plan文件
            {
                // 无plan文件，条码输入框和日期范围不可用
                this.SetBarcodeDateCtrol(false);

                // 检查plan中的状态，如果是close则已经完成，空值表示未完成。
                XmlDocument dom = new XmlDocument();
                dom.Load(planFile);
                XmlNode root = dom.DocumentElement;
                string state = DomUtil.GetAttr(root, "state");
                if (state == BorrowAnalysisService.C_state_close)
                {
                    this.button_plan.Text = "此目录下的借阅报表已创建完成";
                    this.button_plan.Enabled = false;
                }
                else
                {
                    this.button_plan.Enabled = true;
                    this.button_plan.Text = "继续创建借阅报表";
                }

                //<root startDate="2022/11/15" endDate="2022/12/15" dir="d:\temp\a" state="">
                //    <barcodes>XZP00002
                //        XZP00003
                // 显示原来的条码和日期范围
                string barcodes = DomUtil.GetElementText(root, "barcodes");
                barcodes = barcodes.Replace("\r\n", "\n");
                barcodes = barcodes.Replace("\n", "\r\n");
                this.textBox_patronBarcode.Text = barcodes;
                
                string startDate = DomUtil.GetAttr(root, "startDate");
                string endDate = DomUtil.GetAttr(root, "endDate");
                this.dateTimePicker_start.Text = startDate;
                this.dateTimePicker_end.Text = endDate;
            }
        }

        // 设置条码输入框和日期控件是否可用
        public void SetBarcodeDateCtrol(bool bEnable)
        {
            // 条码
            this.textBox_patronBarcode.ReadOnly = !bEnable;
            this.button_selectBatcodeFile.Enabled = bEnable;

            // 时间范围控件
            this.comboBox_quickSetFilenames.Enabled = bEnable;
            this.dateTimePicker_start.Enabled = bEnable;
            this.dateTimePicker_end.Enabled = bEnable;
        }



        // 选择证条码号文件
        private void button_selectBatcodeFile_Click(object sender, EventArgs e)
        {
            string fileName = "";
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "请指定读者证条码号文件";
                dlg.FileName = "";
                dlg.Filter = "读者证条码号文件 （*.txt|*.txt|All files(*.*)|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                fileName = dlg.FileName;
            }


            // 取出条码号文件的内容，放在条码框里
            using (StreamReader reader = new StreamReader(fileName))//, Encoding.UTF8))
            {
                this.textBox_patronBarcode.Text = reader.ReadToEnd().Trim();
            }
        }

        // 关闭
        private void button_close_Click_1(object sender, EventArgs e)
        {
            // 先停止线程
            this._cancel.Cancel();

            // 再关闭
            this.Close();
        }

        private void createReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 先停止线程
            this._cancel.Cancel();
        }


        // 暂停
        private void button_stop_Click(object sender, EventArgs e)
        {
            // 暂停。注：会走到线程的catch里，那里会设置按钮的状态
            this._cancel.Cancel();

            // 这个时候，显示新的按钮名称
            this.button_plan.Text = "继续创建借阅报表";
        }

        #endregion

        #region 界面控件是否可用，和进度条

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();


        ////创建报表过程，设置 开始和停止 按钮是否可用
        //void EnableControls(bool bEnable)
        //{
        //    // 停止按钮
        //    this.button_stop.Enabled = !(bEnable);

        //    // 开始/继续
        //    //this.button_plan.Enabled = bEnable;

        //    // 选择目录
        //    this.textBox_outputDir.Enabled = bEnable;
        //    this.button_selectDir.Enabled = bEnable;

        //    if (bEnable == false)
        //    {
        //        // 条码号与日期范围
        //        this.EnableCtrlForSelectDir(bEnable);
        //    }
        //}

        // 根据选择目录，决定条码框输入和选择日期范围是否可用。
        private void EnableCtrlForSelectDir(bool bEnable)
        {
            // 证条码输入框与选择按钮
            this.textBox_patronBarcode.Enabled = bEnable;
            this.button_selectBatcodeFile.Enabled = bEnable;

            // 时间范围控件
            this.comboBox_quickSetFilenames.Enabled = bEnable;
            this.dateTimePicker_start.Enabled = bEnable;
            this.dateTimePicker_end.Enabled = bEnable;
        }

        // 设置进度条
        public void SetProcess(int min, int max, int value)
        {
            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                if (min != -1)
                    this.progressBar1.Minimum = min;

                if (max != -1)
                    this.progressBar1.Maximum = max;

                if (value != -1)
                    this.progressBar1.Value = value;
            }
            ));
        }

        // 显示信息
        public void SetProcessInfo(string text)
        {
            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                this.label_info.Text = text;
            }
            ));
        }

        #endregion

        #region 按计划创建的报表


        // 按计划创建报表
        private void button_plan_Click(object sender, EventArgs e)
        {
            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();

            // 继续创建报表
            if (this.button_plan.Text == "继续创建借阅报表")
            {
                string dir = this.textBox_outputDir.Text.Trim();
                // 开一个新线程
                Task.Run(() =>
                {
                    ContinuePlan(this._cancel.Token, dir);
                });
            }
            else if (this.button_plan.Text == "开始创建借阅报表")
            {
                // 获取界面输入参数
                int nRet = this.GetInput(out string patronBarcodes,
                    out string startDate,
                    out string endDate,
                    out string dir);
                if (nRet == -1)
                    return;

                // 开一个新线程
                Task.Run(() =>
                {
                    StartPlan(this._cancel.Token,
                        patronBarcodes,
                        startDate,
                        endDate,
                        dir);
                });
            }
            else
            {
                MessageBox.Show(this, "不认识的操作按钮名称");
                return;
            }
        }

        private Cursor SetCtrlForThreadStart(bool first)
        {
            Cursor oldCursor = Cursor.Current;

            // 用Invoke线程安全的方式来调
            this.Invoke((Action)(() =>
            {
                // 设置开始按钮为不可用，停止按钮可用
                this.button_plan.Enabled = false;
                this.button_stop.Enabled = true;

                // 设置选择目录不可用
                this.textBox_outputDir.Enabled = false;
                this.button_selectDir.Enabled = false;

                //清空界面数据
                this.label_info.Text = "";

                // 鼠标设为等待状态
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                // 如果是首次创建，还需把条码和日期范围置为不可用
                if (first == true)
                {
                    this.SetBarcodeDateCtrol(false);
                }
            }
            ));

            return oldCursor;
        }

        private void SetCtrlForThreadEnd(Cursor oldCursor)
        {
            this.Invoke((Action)(() =>
            {
                // 设置选择目录可用，用户可选择其它目录
                this.textBox_outputDir.Enabled = true;
                this.button_selectDir.Enabled = true;

                //停止按钮不可用
                this.button_stop.Enabled = false;

                if (oldCursor!=null)
                    this.Cursor = oldCursor;
            }
            ));
        }

        // 开始创建报表
        private void StartPlan(CancellationToken token,
            string patronBarcodes,
            string startDate,
            string endDate,
            string dir)
        {
            // 设置按钮状态不可用，只有停止按钮可用
            Cursor oldCursor = SetCtrlForThreadStart(true);

            try
            {

                // 创建计划文件
                string planFile = BorrowAnalysisService.CreatePlan(this._cancel.Token,
                    patronBarcodes,
                    startDate,
                    endDate,
                    dir,
                    this.SetProcessInfo);

                // 执行计划
                BorrowAnalysisService.ExecutePlan(this._cancel.Token,
                        planFile,
                        this.SetProcess,
                        this.SetProcessInfo);

                this.Invoke((Action)(() =>
                {
                    this.button_plan.Text = "此目录下的借阅报表已创建完成";

                    // 创建完成后，创建按钮就不可用了
                    this.button_plan.Enabled = false;


                    MessageBox.Show(this, "创建报表完成");
                }
                ));


            }
            catch (Exception ex)
            {
                // 报错的情况
                this.Invoke((Action)(() =>
                {
                    // 出错时，创建按钮变为可用，可以继续处理
                    this.button_plan.Enabled = true;

                    MessageBox.Show(this, "创建报表出错："+ex.Message);
                }
                ));
            }
            finally
            {
                // 线程结束，设置选择目录可用，停止按钮不可用，光标还原
                 this.SetCtrlForThreadEnd(oldCursor);
            }


        }

        // 继续创建报表
        private void ContinuePlan(CancellationToken token, string dir)
        {
            // 设置按钮状态，记下原来的光标
            Cursor oldCursor =this.SetCtrlForThreadStart(false);

            try
            {
                string planFile = dir + "\\plan.txt";

                // 执行计划
                BorrowAnalysisService.ExecutePlan(token,
                        planFile,
                        this.SetProcess,
                        this.SetProcessInfo);

                //
                this.Invoke((Action)(() =>
                {
                    this.button_plan.Text = "此目录下的借阅报表已创建完成";

                    // 创建完成，按钮变不可用状态
                    this.button_plan.Enabled = false;

                    MessageBox.Show(this, "创建报表完成");
                }
                    ));
            }
            catch (Exception ex)
            {
                // 设置按置状态
                this.Invoke((Action)(() =>
                {
                    // 出错时，创建按钮变为可用，可以继续处理
                    this.button_plan.Enabled = true;

                    MessageBox.Show(this, "创建报表出错：" + ex.Message);
                }
                ));
            }
            finally
            {
                // 线程结束，设置选择目录可用，停止按钮不可用，光标还原
                this.SetCtrlForThreadEnd(oldCursor);
            }

        }


        #endregion

    }
}

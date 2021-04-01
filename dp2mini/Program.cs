using DigitalPlatform.CirculationClient;
using DigitalPlatform.Forms;
using DigitalPlatform.Text;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2mini
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            // 前端信息
            ClientInfo.TypeOfProgram = typeof(Program);

            //// 检查是否是开发模式,即命令行是否有develop
            //if (StringUtil.IsDevelopMode() == false)
            //    ClientInfo.PrepareCatchException();

            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
            bool createdNew = true;
            // mutex name need contains windows account name. or us programes file path, hashed
            using (Mutex mutex = new Mutex(true, "dp2mini", out createdNew))
            {
                if (createdNew)
                {

                    //ProgramUtil.SetDpiAwareness();



                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());


                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            API.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }
    }
}

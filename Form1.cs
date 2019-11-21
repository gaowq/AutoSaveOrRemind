using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoSaveOrRemind
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        private const byte vbKeyControl = 0x11;   // CTRL 键
        private const byte vbKeyS = 83;
        //计算使用时间线程
        private Thread gapThread;

        //自动保存默认时间：60分钟
        private int circleMinute = 60;
        //一分钟间隔
        private int gapTime = 60 * 1000;
        private int countGap = 0;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        //[DllImport("user32.dll", EntryPoint = "GetParent", SetLastError = true)]
        //public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetClassName")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCont);

        [DllImport("User32.dll")]
        static extern int GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        //private string[] autoSoftName = { "abc.exe" };

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 离开事件
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAuto();
        }

        /// <summary>
        /// 开始
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            //先退出当前线程
            StopAuto();

            gapThread = new Thread(new ThreadStart(GapTimeCount));
            gapThread.Start();

            label2.Text = "启动中。。。";
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            StopAuto();
            label2.Text ="已关闭";
        }

        #region 自动方法
        /// <summary>
        /// 自动按键
        /// </summary>
        private void AutoPressSave()
        {
            //按下
            keybd_event(vbKeyControl, 0, 0, 0);
            keybd_event(vbKeyS, 0, 0, 0);
            Thread.Sleep(100);
            //松开
            keybd_event(vbKeyControl, 0, 2, 0);
            keybd_event(vbKeyS, 0, 2, 0);
        }

        /// <summary>
        /// 自动提醒
        /// </summary>
        private void AutoRemind()
        {
            MessageBox.Show(circleMinute.ToString() + "分钟过去了，该保存了");
        }

        private void StopAuto()
        {
            if (gapThread != null)
            {
                gapThread.Abort();
            }
        }


        private void GapTimeCount()
        {
            while (true)
            {
                Thread.Sleep(gapTime);
                
                //if (GetCurrentSoft().StartsWith("Adobe"))
                //if (GetCurrentSoft().ToLower().Contains(".jpg") || GetCurrentSoft().ToLower().Contains(".pdf") || GetCurrentSoft().ToLower().Contains(".gif") || GetCurrentSoft().ToLower().Contains(".png")
                //    || GetCurrentSoft().ToLower().Contains(".psd") || GetCurrentSoft().ToLower().Contains(".pdd") || GetCurrentSoft().ToLower().Contains(".jpeg")
                //    || GetCurrentSoft().ToLower().Contains(".eps") || GetCurrentSoft().ToLower().Contains(".tiff") || GetCurrentSoft().ToLower().Contains(".jpeg")
                //    )
                
                if (GetCurrentSoft().ToLower().Contains("photoshop") || GetCurrentSoft().ToLower().Contains("illustrator") || GetCurrentSoft().ToLower().Contains("indesign"))
                {
                    //MessageBox.Show(GetCurrentSoft());
                    countGap++;

                    if (countGap > circleMinute)
                    {
                        if (radioButton1.Checked)
                        {
                            AutoPressSave();
                        }
                        else
                        {
                            AutoRemind();
                        }

                        countGap = 0;
                    }
                }
            }
        }

        private string GetCurrentSoft()
        {
            var hand = GetForegroundWindow();
            int len = GetWindowTextLength(hand);
            StringBuilder text = new StringBuilder(len + 1);
            //int i = GetWindowText(hand, text, len + 1);


            GetClassName(hand, text, 100);
            return text.ToString().Trim();
        }

        /*

        private static class NativeMethods
        {
            internal const uint GW_OWNER = 4;

            internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            internal static extern bool IsWindowVisible(IntPtr hWnd);
        }

        public static IntPtr GetMainWindowHandle(IntPtr child)
        {
            IntPtr MainWindowHandle = IntPtr.Zero;

            NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((hWnd, lParam) =>
            {
                IntPtr PID;
                NativeMethods.GetWindowThreadProcessId(hWnd, out PID);

                if (PID == lParam &&
                    NativeMethods.IsWindowVisible(hWnd) &&
                    NativeMethods.GetWindow(hWnd, NativeMethods.GW_OWNER) == IntPtr.Zero)
                {
                    MainWindowHandle = hWnd;
                    return false;
                }

                return true;

            }), child);

            return MainWindowHandle;
        }
        */
        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                int newCircleTime = 0;
                int.TryParse(textBox1.Text, out newCircleTime);

                if (newCircleTime > 0)
                {
                    circleMinute = newCircleTime;
                }
            }
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton2.Checked = false;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = true;
        }
    }
}

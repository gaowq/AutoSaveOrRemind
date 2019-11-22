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
            StartHook(this);
            label2.Text = "启动成功";
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            StopAuto();
            label2.Text = "已关闭";
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
            StopHook();

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

        #region 勾子

        private static int hHook = 0;
        private static Form HookControlForm;
        private static HookProc KeyHookProcedure;

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// GetForegroundWindow
        ///  　函数功能：该函数返回前台窗口（用户当前工作的窗口）。系统分配给产生前台窗口的线程一个稍高一点的优先级。
        ///  　函数原型：HWND GetForegroundWindow（VOID）
        ///  　参数：无。
        ///  　返回值：函数返回前台窗回的句柄。
        ///  　速查：Windows NT：3.1以上版本；Windows：95以上版本：Windows CE：1.0以上版本：头文件：Winuser.h；库文件：user32.lib。
        ///  　摘要：
        ///  　函数功能：该函数返回前台窗口（用户当前工作的窗口）。系统分配给产生前台窗口的线程一个稍高一点的优先级。
        ///  　函数原型：HWND GetForegroundWindow（VOID）
        ///  　参数：无。
        ///  　返回值：函数返回前台窗回的句柄。
        ///  　速查：Windows NT：3.1以上版本；Windows：95以上版本：Windows CE：1.0以上版本：头文件：Winuser.h；库文件：user32.lib。
        /// 
        /// <summary>
        /// GetWindowThreadProcessId这个函数来获得窗口所属进程ID和线程ID
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, int ID);

        //[StructLayout(LayoutKind.Sequential)] //声明键盘钩子的封送结构类型 
        //public class KeyboardHookStruct
        //{
        //    public int vkCode; //表示一个在1到254间的虚似键盘码 
        //    public int scanCode; //表示硬件扫描码 
        //    public int flags;
        //    public int time;
        //    public int dwExtraInfo;
        //}

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetKeyState(int keyCode);

        /// <summary>
        /// 消息处理中心
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int HOOKProcReturn(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0)
            {
                int state = GetKeyState(vbKeyControl);
                int state2 = GetKeyState(vbKeyS);
                //label2.Text = wParam.ToString() + "," + GetKeyState(vbKeyControl);// +","+state.ToString();

                if (state != 0 && state2 != 0 && state != 1 && state2 != 1)
                {
                    if (GetCurrentSoft().ToLower().Contains("photoshop") || GetCurrentSoft().ToLower().Contains("illustrator") || GetCurrentSoft().ToLower().Contains("indesign"))
                    {
                        //label2.Text = state.ToString() + "," + state2.ToString();
                        countGap = 0;
                    }
                }

                //KeyboardHookStruct KeyDataFromHook = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                //Keys keyData = (Keys)KeyDataFromHook.vkCode;

                //if (keyData == Keys.LControlKey || keyData == Keys.RControlKey)
                //{

                //}

                if (wParam.ToInt32() == vbKeyControl)
                {
                    IntPtr intpr = GetForegroundWindow();
                    if ((intpr == HookControlForm.Handle))
                    {
                        MessageBox.Show("Control");
                    }
                }

                ////鼠标右键
                //if (wParam.ToInt32() == 0x205)
                //{
                //    IntPtr intpr = GetForegroundWindow();
                //    if ((intpr == HookControlForm.Handle))
                //    {
                //        MessageBox.Show("ok- 0x205");
                //    }
                //}
                //else if (wParam.ToInt32() == 0x203)
                //{
                //    if (GetForegroundWindow() == HookControlForm.Handle)
                //    {
                //        MessageBox.Show("ok- 0x203");
                //    }
                //}//鼠标左键
                //else if (wParam.ToInt32() == 0x201)
                //{
                //    if (GetForegroundWindow() == HookControlForm.Handle)
                //        MessageBox.Show("ok- 0x201");


                //}
                //else if (wParam.ToInt32() == 0xa1)
                //{
                //    if ((GetForegroundWindow() == HookControlForm.Handle))
                //    {
                //        MessageBox.Show("ok- 0xa1");
                //    }
                //}
                //else if (wParam.ToInt32() == 0x202)
                //{
                //    //MessageBox.Show("ok- 0x202");
                //}
                //else if (wParam.ToInt32() == 0x200)
                //{
                //    //  MessageBox.Show("ok-0x200");
                //}
            }
            //监听下一次事件
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //[DllImport("kernel32")]
        //public static extern int GetModuleHandle(string lpModuleName);
        public bool StartHook(Form HookControl)
        {
            HookControlForm = HookControl;
            KeyHookProcedure = new HookProc(HOOKProcReturn);
            //监听 事件 发送消息
            hHook = SetWindowsHookEx((int)HookType.WH_KEYBOARD_LL, KeyHookProcedure, IntPtr.Zero, 0);
            if (hHook == 0)
            {
                return false;
            }
            return true;
        }

        public static bool StopHook()
        {
            return UnhookWindowsHookEx(hHook);
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //[StructLayout(LayoutKind.Sequential)]
        //public class MouseHookStruct
        //{
        //    public POINT pt;
        //    public int hwnd;
        //    public int wHitTestCode;
        //    public int dwExtraInfo;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public class POINT
        //{
        //    public int x;
        //    public int y;
        //}

        /// <summary>
        /// 钩子类型舰艇鼠标键盘等事件
        /// </summary>
        private enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,//局部 线程级别
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14 //全局
        }
        #endregion
    }
}

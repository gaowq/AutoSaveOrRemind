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
    //todo 1.ps,ai,id
    //

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        private const byte vbKeyControl = 0x11;   // CTRL 键
        private const byte vbKeyS = 83;
        //提醒和自动保存线程
        private Thread thread;
        //计算使用时间线程
        private Thread thread2;

        //自动保存默认时间：1h
        private int circleTime = 60 * 60 * 1000;
        private int gapTime = 1000;
        private int countGap = 0;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        static extern int GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        private string[] autoSoftName = { "abc.exe" };

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

            if (radioButton1.Checked)
            {
                thread = new Thread(new ThreadStart(AutoPressSave));
                thread.Start();
            }
            else
            {
                thread = new Thread(new ThreadStart(AutoRemind));
                thread.Start();
            }
            MessageBox.Show("设置成功");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            StopAuto();
            MessageBox.Show("关闭成功");
        }

        #region 自动方法
        /// <summary>
        /// 自动按键
        /// </summary>
        private void AutoPressSave()
        {
            while (true)
            {
                //按下
                keybd_event(vbKeyControl, 0, 0, 0);
                keybd_event(vbKeyS, 0, 0, 0);
                Thread.Sleep(100);
                //松开
                keybd_event(vbKeyControl, 0, 2, 0);
                keybd_event(vbKeyS, 0, 2, 0);
                Thread.Sleep(circleTime);
            }
        }

        /// <summary>
        /// 自动提醒
        /// </summary>
        private void AutoRemind()
        {
            while (true)
            {
                Thread.Sleep(circleTime);
                MessageBox.Show((circleTime / 60000).ToString() + "分钟过去了，该保存了");
            }
        }

        private void StopAuto()
        {
            if (thread != null)
            {
                thread.Abort();
            }
        }


        private void GapTimeCount()
        {
            while (true)
            {
                Thread.Sleep(gapTime);
                if(autoSoftName.Contains(GetCurrentSoft()))
                    countGap++;
            }
        }

        private string GetCurrentSoft()
        {
            var hand = GetForegroundWindow();
            int len = GetWindowTextLength(hand);
            StringBuilder text = new StringBuilder(len + 1);
            int i = GetWindowText(hand, text, len + 1);
            return text.ToString().Trim();
        }
        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                int newCircleTime = 0;
                int.TryParse(textBox1.Text, out newCircleTime);

                if (newCircleTime > 0)
                {
                    circleTime = newCircleTime * 60 * 1000;
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

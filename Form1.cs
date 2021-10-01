using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YeeLight
{
    public partial class Form1 : Form
    {
        Locator loc;
        public static NotifyIcon Notifier { get { return notifier; } }
        private static NotifyIcon notifier;
        public Form1()
        {
            InitializeComponent();
            notifier = this.notifyIcon;
            loc = new Locator();
            loc.Find();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            FadeIn(this, 10);
        }
        private async void FadeIn(Form o, int interval = 80)
        {
            //Object is not fully invisible. Fade it in
            while (o.Opacity < 1.0)
            {
                await Task.Delay(interval);
                o.Opacity += 0.05;
            }
            o.Opacity = 1;
        }
        private void research_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
            }
            
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
            loc.Find();
        }


    }
}

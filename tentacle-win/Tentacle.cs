using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime;
using cn.org.hentai.tentacle.graphic;
using System.Threading;
using cn.org.hentai.tentacle.display;
using System.Drawing.Imaging;

namespace cn.org.hentai.tentacle.win
{
    public partial class Tentacle : Form
    {
        public Tentacle()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(test).Start();
        }

        private void test()
        {
            DisplayContext screen = new DisplayContext();
            for (int i = 0; i < 100; i++)
            {
                long time = DateTime.Now.Ticks;
                screen.CaptureScreen();
                time = DateTime.Now.Ticks - time;
                Console.WriteLine("Spend: " + (time / 1000));
                Thread.Sleep(100);
            }
        }
    }
}

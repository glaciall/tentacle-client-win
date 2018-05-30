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
using System.Net.Sockets;
using System.IO;
using cn.org.hentai.tentacle.app;

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

        }

        private void Tentacle_Load(object sender, EventArgs e)
        {
            // new TentacleApp().start();
            new XXWorker().start();
        }
    }

    class XXWorker : Worker
    {
        public override void before()
        {
            Console.WriteLine("some works before...");
        }

        public override void after()
        {
            Console.WriteLine("do something after...");
        }

        public override int loops()
        {
            return -1;
        }

        public override void run()
        {
            Console.WriteLine("吭哧吭哧。。。");
        }
    }
}

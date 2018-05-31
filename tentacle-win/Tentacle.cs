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
using cn.org.hentai.tentacle.util;
using System.Net;

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
            // new XXWorker().start();
            client = new SocketClient();
            client.connect("192.168.1.10", 1234, xxoo);
        }

        SocketClient client = null;

        public void xxoo(byte[] data)
        {
            string recv = Encoding.ASCII.GetString(data);
            Console.WriteLine("Recv: " + recv);
            if (recv.IndexOf("exit") > -1)
            {
                client.close();
                return;
            }
            client.send(Encoding.ASCII.GetBytes("FUCK: " + recv.ToUpper()));
        }
    }

    class XXWorker : Worker
    {
        public override void run()
        {
            
        }

        public override void after()
        {
            Console.WriteLine("这就完事了？");
        }
    }
}

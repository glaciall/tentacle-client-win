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
using cn.org.hentai.tentacle.system;

namespace cn.org.hentai.tentacle.win
{
    public partial class Tentacle : Form
    {
        TentacleApp app = null;

        public Tentacle()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // ...
        }

        private void Tentacle_Load(object sender, EventArgs e)
        {
            (app = new TentacleApp()).start();
        }

        private void Tentacle_FormClosing(object sender, FormClosingEventArgs e)
        {
            // app.terminate();
        }
    }
}

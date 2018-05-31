using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace cn.org.hentai.tentacle.util
{
    public abstract class Worker
    {
        private Thread thread;
        public bool terminated { get; set; }

        public virtual void before()
        {
            // 事前准备
        }

        public abstract void run();

        public virtual int loops()
        {
            return 1;
        }

        public virtual int loopInterval()
        {
            return 0;
        }

        public virtual void after()
        {
            // 事后处理
        }

        private void work()
        {
            before();
            try
            {
                var count = 0;
                var loopCount = loops();
                do
                {
                    run();
                    count += 1;
                    if (loopCount >= 0 && count >= loopCount) break;
                    if (loopInterval() > 0) Thread.Sleep(loopInterval());
                }
                while (!this.terminated);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Thread: " + e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
            after();
        }

        public void start()
        {
            (this.thread = new Thread(this.work)).Start();
        }
    }
}

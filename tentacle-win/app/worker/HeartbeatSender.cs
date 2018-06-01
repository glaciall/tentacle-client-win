using cn.org.hentai.tentacle.protocol;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cn.org.hentai.tentacle.app
{
    public class HeartbeatSender : Worker
    {
        private SocketClient socketClient;
        public HeartbeatSender(SocketClient socketClient)
        {
            this.socketClient = socketClient;
        }

        public override void run()
        {
            this.socketClient.send(Packet.create(Command.HEARTBEAT, 5).addBytes(Encoding.ASCII.GetBytes("HELLO")).getBytes());
        }

        public override int loops()
        {
            return -1;
        }

        public override int loopInterval()
        {
            return 10000;
        }
    }
}

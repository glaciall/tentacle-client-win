using cn.org.hentai.tentacle.hid;
using cn.org.hentai.tentacle.system;
using cn.org.hentai.tentacle.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace cn.org.hentai.tentacle.app
{
    public class HIDCommandExecutor : Worker
    {
        private ConcurrentQueue<HIDCommand> commands = new ConcurrentQueue<HIDCommand>();

        public void add(HIDCommand cmd)
        {
            commands.Enqueue(cmd);
        }

        public override int loops()
        {
            return -1;
        }

        public override int loopInterval()
        {
            return 20;
        }

        public override void run()
        {
            HIDCommand hidCommand = null;
            if (!commands.TryDequeue(out hidCommand)) return;

            if (hidCommand is MouseCommand)
            {
                MouseCommand cmd = (MouseCommand)hidCommand;
                // if (cmd.eventType == MouseCommand.MOUSE_MOVE) MouseCtrl.mouseMove(cmd.x, cmd.y);
                if (cmd.eventType == MouseCommand.MOUSE_DOWN) MouseCtrl.mouseDown(cmd.x, cmd.y, cmd.key);
                else if (cmd.eventType == MouseCommand.MOUSE_UP) MouseCtrl.mouseUp(cmd.x, cmd.y, cmd.key);
                else if (cmd.eventType == MouseCommand.MOUSE_WHEEL) MouseCtrl.mouseScroll(cmd.x, cmd.y, cmd.key);
            }
            else if (hidCommand is KeyboardCommand)
            {
                // TODO: 键盘指令
            }
        }
    }
}

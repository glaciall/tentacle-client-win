using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace tentacle_lib.system
{
    public class ClipboardAsync
    {
        private string _getText;
        private void ThGetText(object format)
        {
            try
            {
                _getText = format == null ? Clipboard.GetText() : Clipboard.GetText((TextDataFormat)format);
            }
            catch
            {
                _getText = null;
            }
        }

        private void ThSetText(object text)
        {
            try
            {
                Clipboard.SetText((string)text);
            }
            catch
            {
                // ...
            }
        }

        public static string GetText()
        {
            var instance = new ClipboardAsync();
            var staThread = new Thread(instance.ThGetText);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return instance._getText;
        }

        public static void SetText(string text)
        {
            var instance = new ClipboardAsync();
            var staThread = new Thread(instance.ThSetText);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(text);
            staThread.Join();
        }

        public static string GetText(TextDataFormat format)
        {
            var instance = new ClipboardAsync();
            var staThread = new Thread(instance.ThGetText);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(format);
            staThread.Join();
            return instance._getText;
        }
    }
}

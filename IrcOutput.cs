using Sync;
using Sync.MessageFilter;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneKey
{
    class IRCOutput : ISyncOutput
    {
        public void Clear()
        {
        }

        public void Write(string msg, bool newline = true, bool time = true)
        {
            SyncHost.Instance.ClientWrapper.Client.SendMessage(new IRCMessage(SyncHost.Instance.ClientWrapper.Client.NickName, msg));
        }

        public void WriteColor(string text, ConsoleColor color, bool newline = true, bool time = true)
        {
            Write(text);
        }

        public void WriteHelp(string cmd, string desc)
        {
        }

        public void WriteHelp()
        {
        }

        public void WriteStatus()
        {
        }

        public void WriteWelcome()
        {
        }
    }
}

using Sync;
using Sync.MessageFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneKey.IRC
{
    class IRCCommand : ISourceClient, IFilter
    {
        private const string COMMAND_PREFIX = "?ok";

        public void onMsg(ref IMessageBase msg)
        {
            if (!msg.Message.RawText.StartsWith(COMMAND_PREFIX))
                return;

            msg.Cancel = true;

            var cmd = msg.Message.RawText.Remove(0,1).Trim();

            SyncHost.Instance.Commands.invokeCmdString(cmd);
        }
    }
}

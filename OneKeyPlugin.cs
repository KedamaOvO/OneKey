using Sync.Command;
using Sync.MessageFilter;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sync.Plugins.PluginEvents;

namespace OneKey
{
    public class OneKeyPlugin : Plugin, IFilter,ISourceClient
    {
        private const string PLUGIN_NAME = "OneKey";
        private const string PLUGIN_AUTHOR = "KedamaOvO";
        public const string VERSION = "0.0.1";

        private IRCOutput ircOutput = new IRCOutput();

        public OneKeyPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            new PluginConfigurationManager(this).AddItem(new SettingIni());
            EventBus.BindEvent<PluginEvents.InitCommandEvent>(InitCommand);
            EventBus.BindEvent<PluginEvents.ProgramReadyEvent>(OneKeyCommandInit);
            EventBus.BindEvent<InitFilterEvent>((filter) => filter.Filters.AddFilter(this));
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
        }

        private void OneKeyCommandInit(PluginEvents.ProgramReadyEvent @event)
        {
            RegisterCommandHandler("exec", (args,output) =>
             {
                 if (args.Count >= 1)
                 {
                     Process.Start(string.Join(" ", args));
                     return true;
                 }
                 return false;
             });

            var bililive = new BilibiliOneKeyLive();

            bool BililiveHandler(Arguments args, ISyncOutput output)
            {
                bililive.Output = output;
                if (args.Count >= 1)
                {
                    if (args[0].ToLower() == "start")
                    {
                        bililive.StartLive();
                        return true;
                    }
                    else if (args[0].ToLower() == "stop")
                    {
                        bililive.StopLive();
                        return true;
                    }
                    else if (args[0].ToLower() == "room-name")
                    {
                        if (args.Count >= 2)
                        {
                            bililive.SetRoomName(args[1]);
                        }
                        else
                        {
                            bililive.GetRoomName().ContinueWith(roomNameTask =>
                            {
                                roomNameTask.Wait();
                                string roomName = roomNameTask.Result;
                                output.WriteColor($"[OneKey]Room Name: {roomName ?? "!!!Get room name Failed!!!"}", ConsoleColor.Green);
                            });

                        }
                        return true;
                    }

                }
                return false;
            };

            RegisterCommandHandler("bililive", BililiveHandler);
            RegisterIrcCommandHandler("bililive", BililiveHandler);
        }

        private void InitCommand(PluginEvents.InitCommandEvent @event)
        {
            @event.Commands.Dispatch.bind("onekey", CommandHandle,"One key plugin");
            @event.Commands.Dispatch.bind("ok", CommandHandle,"One key plugin");

            void AddBindedCommand(string key,string cmd)
            {
                @event.Commands.Dispatch.bind(key, _ =>
                {
                    getHoster().Commands.invokeCmdString(cmd);
                    return true;
                }, $"Binding command => {cmd}");
            }

            foreach (var p in Setting.CommandBindedDictionary)
                AddBindedCommand(p.Key,p.Value);

            RegisterCommandHandler("bind", (args,output)=>
            {
                if (args.Count >= 2)
                {
                    string key = args[0];
                    string cmd = string.Join(" ", args.Skip(1));

                    Setting.CommandBindedDictionary.Add(key, cmd);
                    AddBindedCommand(key, cmd);

                    return true;
                }
                return false;
            });
        }

        private Dictionary<string, Func<Arguments,ISyncOutput, bool>> commandHandleMapping = new Dictionary<string, Func<Arguments, ISyncOutput, bool>>();
        private Dictionary<string, Func<Arguments,ISyncOutput, bool>> IrcCommandHandleMapping = new Dictionary<string, Func<Arguments,ISyncOutput, bool>>();

        public void RegisterCommandHandler(string command,Func<Arguments,ISyncOutput, bool> handle)
        {
            commandHandleMapping.Add(command, handle);
        }
        public void RegisterIrcCommandHandler(string command, Func<Arguments,ISyncOutput, bool> handle)
        {
            IrcCommandHandleMapping.Add(command, handle);
        }

        private bool CommandHandle(Arguments args)
        {
            if (args.Count >= 2)
            {
                string cmd = args[0];
                var inArgs = new Arguments(args.Skip(1).ToArray());
                return commandHandleMapping[cmd](inArgs,Sync.Tools.IO.CurrentIO);
            }
            return false;
        }

        public void onMsg(ref IMessageBase msg)
        {
            if (msg.Message.RawText.StartsWith("?ok"))
            {
                string[] msgs = msg.Message.RawText.Remove(0,1).Split(' ');
                if (msgs.Length >= 2)
                {
                    if (IrcCommandHandleMapping.ContainsKey(msgs[1]))
                    {
                        var inArgs = new Arguments(msgs.Skip(2).ToArray());
                        IrcCommandHandleMapping[msgs[1]](inArgs,ircOutput);
                        msg.Cancel = true;
                    }
                }
            }
        }
    }
}

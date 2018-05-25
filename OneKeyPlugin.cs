using OneKey.IRC;
using Sync.Command;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneKey
{
    public class OneKeyPlugin : Plugin
    {
        private const string PLUGIN_NAME = "OneKey";
        private const string PLUGIN_AUTHOR = "KedamaOvO";
        public const string VERSION = "0.0.1";

        public OneKeyPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            new PluginConfigurationManager(this).AddItem(new SettingIni());
            base.EventBus.BindEvent<PluginEvents.InitCommandEvent>(InitCommand);
            base.EventBus.BindEvent<PluginEvents.ProgramReadyEvent>(OneKeyCommandInit);
            base.EventBus.BindEvent<PluginEvents.InitFilterEvent>(OneKeyFilterInit);
        }

        public override void OnEnable()
        {
            Sync.Tools.IO.CurrentIO.WriteColor(PLUGIN_NAME + " By " + PLUGIN_AUTHOR, ConsoleColor.DarkCyan);
        }

        private void OneKeyFilterInit(PluginEvents.InitFilterEvent @event)
        {
            @event.Filters.AddFilter(new IRCCommand());
        }

        private void OneKeyCommandInit(PluginEvents.ProgramReadyEvent @event)
        {
            RegisterCommandHandle("exec", args =>
             {
                 if (args.Count >= 1)
                 {
                     Process.Start(string.Join(" ", args));
                     return true;
                 }
                 return false;
             });

            var bililive = new BilibiliOneKeyLive();
            RegisterCommandHandle("bililive", args =>
             {
                 if(args.Count>=1)
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
                     else if(args[0].ToLower() == "room-name")
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
                                 Sync.Tools.IO.CurrentIO.WriteColor($"[OneKey]Room Name: {roomName ?? "!!!Get room name Failed!!!"}", ConsoleColor.Green);
                             });
                             
                         }
                         return true;
                     }

                 }
                 return false;
             });
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

            RegisterCommandHandle("bind", args=>
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

        private Dictionary<string, Func<Arguments, bool>> commandHandleMapping = new Dictionary<string, Func<Arguments, bool>>();

        public void RegisterCommandHandle(string command,Func<Arguments, bool> handle)
        {
            commandHandleMapping.Add(command, handle);
        }

        private bool CommandHandle(Arguments args)
        {
            if (args.Count >= 2)
            {
                string cmd = args[0];
                var inArgs = new Arguments(args.Skip(1).ToArray());
                return commandHandleMapping[cmd](inArgs);
            }
            return false;
        }
    }
}

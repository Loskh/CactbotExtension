using Advanced_Combat_Tracker;
using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin.EventSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CactbotExtension.Events
{
    internal class Events
    {
        private EventSource ce;
        private IActPluginV1 ffxivPlugin;

        //private BackgroundWorker tester;
        public Events(EventSource cactbotExtensionEventSource)
        {
            this.ce = cactbotExtensionEventSource;
            Attach();
            //tester = new BackgroundWorker { WorkerSupportsCancellation = true };
            //tester.DoWork += fakeMessage;
            //tester.RunWorkerAsync();
            
        }

        //private void fakeMessage(object sender, DoWorkEventArgs e)
        //{
        //    while (true) {
        //        if (tester.CancellationPending) {
        //            e.Cancel = true;
        //            break;
        //        }

        //        ce.DispatchToJS(new MapEffectEvent(114514, 1, 2, 3, 4));
        //        //ce.LogInfo("test");
        //        Thread.Sleep(3000);
        //    }
        //}

        public void Attach()
        {
            lock (this) {
                if (ActGlobals.oFormActMain == null) {
                    this.ffxivPlugin = null;
                    return;
                }

                if (this.ffxivPlugin == null) {
                    //this.ffxivPlugin =
                    //     ActGlobals.oFormActMain.ActPlugins
                    //     .Where(x =>
                    //     x.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) &&
                    //     x.lblPluginStatus.Text.ToUpper().Contains("FFXIV_ACT_Plugin Started.".ToUpper()))
                    //     .Select(x => x.pluginObj)
                    //     .FirstOrDefault();
                    this.ffxivPlugin= ActGlobals.oFormActMain.ActPlugins.FirstOrDefault(x => x.pluginObj?.GetType().ToString() == "FFXIV_ACT_Plugin.FFXIV_ACT_Plugin")?.pluginObj;
                }

                if (this.ffxivPlugin != null) {
                    ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin).DataSubscription.NetworkReceived -= ffxivPluginNetworkReceivedDelegate;
                    ((FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)this.ffxivPlugin).DataSubscription.NetworkReceived += ffxivPluginNetworkReceivedDelegate;
                    ce.LogInfo("CactbotExtension: Handled: DataSubscription");
                }
            }
        }


        private unsafe void ffxivPluginNetworkReceivedDelegate(string connection, long epoch, byte[] message)
        {
            if (message.Length < sizeof(ServerMessageHeader)) {
                return;
            }
            try {
                fixed (byte* ptr = message) {
                    var header = (ServerMessageHeader*)ptr;
                    var dataPtr = ptr + 0x20;
                    if (header->MessageType == 0x03D7) {
                        //ce.LogInfo("tst");
                        onMapEffectEvent(header->ActorID, (FFXIVIpcMapEffect*)dataPtr);
                    }
                    //ce.LogInfo($"{header->MessageType:X}");
                }
            }
            catch (Exception ex) {
                ce.LogError($"{ex}");
            }
        }

        public class MapEffectEvent : JSEvent
        {
            public MapEffectEvent(uint actorId, uint parm1, uint parm2, ushort parm3, ushort parm4)
            {
                this.actorId = actorId;
                this.parm1 = parm1;
                this.parm2 = parm2;
                this.parm3 = parm3;
                this.parm4 = parm4;
            }
            public string EventName() { return "onMapEffectEvent"; }
            public uint actorId;
            public uint parm1;
            public uint parm2;
            public ushort parm3;
            public ushort parm4;
        }

        [EventName("onMapEffectEvent")]
        public unsafe void onMapEffectEvent(uint actorId, FFXIVIpcMapEffect* message)
        {
            ce.DispatchToJS(new MapEffectEvent(actorId, message->parm1, message->parm2, message->parm3, message->parm4));
            string message2 = $"{actorId:X}:{message->parm1:X8}:{message->parm2:X8}:{message->parm3:X8}:{message->parm4:X8}:";
            Log("103", message2);
            //ce.LogInfo(message2);
        }

        private void Log(string type, string message)
        {
            string text = DateTime.Now.ToString("HH:mm:ss");
            string message2 = "[" + text + "] [" + type + "] " + message.Trim();
            //PluginUI.Log(message2);
            message2 = $"00|{DateTime.Now:O}|0|{type}:{message}|";
            ActGlobals.oFormActMain.ParseRawLogLine(isImport: false, DateTime.Now, message2 ?? "");
        }

    }
}

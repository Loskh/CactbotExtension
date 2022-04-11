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
        private FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxivPlugin;

        //private BackgroundWorker tester;
        public Events(EventSource cactbotExtensionEventSource)
        {
            this.ce = cactbotExtensionEventSource;
            Attach();         
        }
        public void Attach()
        {
            lock (this) {
                if (ActGlobals.oFormActMain == null) {
                    this.ffxivPlugin = null;
                    return;
                }

                if (this.ffxivPlugin == null) {
                    this.ffxivPlugin= (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)(ActGlobals.oFormActMain.ActPlugins.FirstOrDefault(x => x.pluginObj?.GetType().ToString() == "FFXIV_ACT_Plugin.FFXIV_ACT_Plugin")?.pluginObj);
                }
                if (this.ffxivPlugin != null) {
                    this.ffxivPlugin.DataSubscription.ProcessChanged += CactbotExtensionOnProcessChanged;
                    //var task = new Task(() => {

                    //});
                    //task.Start();
                }
            }
        }

        private void CactbotExtensionOnProcessChanged(System.Diagnostics.Process process)
        {
            var gameProcess = this.ffxivPlugin.DataRepository.GetCurrentFFXIVProcess();
            var gameVersion = Utils.GetGameVersion(gameProcess);
            var gameRegion = Utils.GetRegion(this.ffxivPlugin);
            if (OpcodeManager.Instance.GameVersion == gameVersion && OpcodeManager.Instance.Region == gameRegion)
                return;
            OpcodeManager.Instance.LogInfo += ce.LogInfo;
            OpcodeManager.Instance.LogError += ce.LogError;
            OpcodeManager.Instance.LogDebug += ce.LogDebug;
            OpcodeManager.Instance.Init(gameVersion, gameRegion);
            OpcodeManager.Instance.EnsureOpcode();

            this.MapEffect = (ushort)OpcodeManager.Instance.GetOpcode("MapEffect");
            ce.LogInfo($"Opcode:MapEffect={this.MapEffect:X4}");

            this.ffxivPlugin.DataSubscription.NetworkReceived -= ffxivPluginNetworkReceivedDelegate;
            this.ffxivPlugin.DataSubscription.NetworkReceived += ffxivPluginNetworkReceivedDelegate;
            ce.LogInfo("Net Data Subscription Added.");
        }

        private ushort MapEffect=0xFFFF;
        private unsafe void ffxivPluginNetworkReceivedDelegate(string connection, long epoch, byte[] message)
        {
            if (message.Length < sizeof(ServerMessageHeader)) {
                return;
            }
            try {
                fixed (byte* ptr = message) {
                    var header = (ServerMessageHeader*)ptr;
                    var dataPtr = ptr + 0x20;
                    if (header->MessageType == MapEffect) {
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
            string logline = $"{actorId:X}:{message->parm1:X8}:{message->parm2:X8}:{message->parm3:X8}:{message->parm4:X8}:";
            Utils.AddLogLine("103", logline);
            //ce.LogInfo(logline);
        }

        public void Dispose()
        {
            this.ffxivPlugin.DataSubscription.NetworkReceived -= ffxivPluginNetworkReceivedDelegate;
            ce.LogInfo("Net Data Subscription Removed.");
        }
    }
}

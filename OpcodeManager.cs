using Advanced_Combat_Tracker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CactbotExtension
{
    public sealed class OpcodeManager
    {
        private static readonly Lazy<OpcodeManager> _instance = new Lazy<OpcodeManager>(() => new OpcodeManager());

        private const string REMOTE_STORE_URL = "https://loskh.diemoe.net/";
        public static OpcodeManager Instance
        {
            get { return _instance.Value; }
        }

        private readonly string LocalOpcodeFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config", "CactbotExtension.Opcode.json");

        private string GameVersion = "2022.03.29.0000.0000";
        private string Region = "cn";

        private JObject Opcode = new();

        public delegate void LogInfoHandler(params object[] args);
        public event LogInfoHandler LogInfo;
        public delegate void LogErrorHandler(params object[] args);
        public event LogErrorHandler LogError;
        public delegate void LogDebugHandler(params object[] args);
        public event LogDebugHandler LogDebug;
        public OpcodeManager()
        {

        }

        public void Init(string gameVerson, string region)
        {
            this.GameVersion = gameVerson;
            this.Region = region;
            LogInfo?.Invoke($"{region}:{gameVerson}");
        }
        public void EnsureOpcode()
        {
            if (CheckOpcodeVersion()) return;
            GetOpcodeFileFromNetwork();
            if (!CheckOpcodeVersion()) {
                LogError?.Invoke("无法找到合适的Opcode,请等待更新！");
                throw new Exception("无法找到合适的Opcode,请等待更新！");
            }
            return;
        }

        private bool CheckOpcodeVersion()
        {
            try {
                var opcodeJsonString = File.ReadAllText(LocalOpcodeFile);
                var opcodeJson = JObject.Parse(opcodeJsonString);
                foreach (var ver in opcodeJson["FFXIVClients"]) {
                    if (ver["GameVersion"].ToString() == GameVersion && ver["Region"].ToString().ToLower() == Region) {
                        Opcode = (JObject)ver["Opcode"];
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void GetOpcodeFileFromNetwork()
        {
            using var client = new WebClient();
            var remoteOpcode = client.DownloadString(REMOTE_STORE_URL + "Opcode.json");

            if (File.Exists(LocalOpcodeFile)) {
                File.Delete(LocalOpcodeFile);
                LogInfo("Local opcode removed");
            }
            LogInfo("Remote opcode file downloaded");
            File.WriteAllText(LocalOpcodeFile, remoteOpcode);

        }

        public ushort GetOpcode(string opcodeName)
        {
            var opcodeStr = Opcode[opcodeName].ToString();
            if (string.IsNullOrEmpty(opcodeStr)) {
                LogError($"无法找到Opcode:{opcodeName}");
                throw new Exception("无法找到合适的Opcode,请等待更新！");
            }
            var opcode = Convert.ToInt16(opcodeStr, 16);
            return (ushort)opcode;
        }

    }
}

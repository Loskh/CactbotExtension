using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CactbotExtension
{
    internal class Utils
    {
        public static void Log(string type, string message)
        {
            var logline = $"00|{DateTime.Now:O}|0|{type}:{message}|";
            ActGlobals.oFormActMain.ParseRawLogLine(isImport: false, DateTime.Now, logline ?? "");
        }

        public static string GetGameVersion(Process process) {
            var gamePath = Path.GetDirectoryName(process.MainModule.FileName);
            gamePath = Path.Combine(gamePath, "ffxivgame.ver");
            //string version = "2000.01.01.0000.0000";
            var version = File.ReadAllText(gamePath);
            return version;
        }

        //https://github.com/quisquous/cactbot/blob/f0b312bfb0cc263c82c0ea2eceb983de5b25de8b/plugin/CactbotEventSource/FFXIVPlugin.cs#L28
        public static string GetLocaleString(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxiv_plugin)
        {
            switch (GetLanguageId(ffxiv_plugin)) {
                case 1:
                    return "en";
                case 2:
                    return "fr";
                case 3:
                    return "de";
                case 4:
                    return "ja";
                case 5:
                    return "cn";
                case 6:
                    return "ko";
                default:
                    return null;
            }
        }

        public static int GetLanguageId(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxiv_plugin)
        {
            if (ffxiv_plugin == null) {
                return 0;
            }
            try {
                return (int)ffxiv_plugin.DataRepository.GetSelectedLanguageID();
            }
            catch (Exception e) {
                return 0;
            }
        }
    }
}

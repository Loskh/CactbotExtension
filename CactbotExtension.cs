using Advanced_Combat_Tracker;
using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;

namespace CactbotExtension
{
    public class CactbotExtension : IActPluginV1, IOverlayAddonV2
    {
        public static string pluginPath = "";

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginStatusText.Text = "Ready.";

            // We don't need a tab here.
            ((TabControl)pluginScreenSpace.Parent).TabPages.Remove(pluginScreenSpace);

            foreach (var plugin in ActGlobals.oFormActMain.ActPlugins)
            {
                if (plugin.pluginObj == this)
                {
                    pluginPath = plugin.pluginFile.FullName;
                    break;
                }
            }
            Init();
        }

        public void DeInitPlugin()
        {

        }

        public void Init()
        {
            var container = Registry.GetContainer();
            var registry = container.Resolve<Registry>();

            // Register EventSource
            registry.StartEventSource(new EventSource(container));

        }
    }
}

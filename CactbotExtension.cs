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
using System.Reflection;

namespace CactbotExtension
{
    public class CactbotExtension : IActPluginV1, IOverlayAddonV2
    {
        public string pluginPath = "";
        private static EventSource EventSource;
        private static TinyIoCContainer TinyIoCContainer;
        private static Registry Registry;

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
            foreach (var item in Registry.EventSources) {
                if (item.Name == "CactbotExtension" && item != null) {
                    item.Dispose();
                }
            }
            Type type = typeof(Registry);
            FieldInfo fieldInfo = type.GetField("_eventSources", BindingFlags.Instance | BindingFlags.NonPublic);
            ((List<IEventSource>)fieldInfo.GetValue(Registry)).Remove(EventSource);
        }

        public void Init()
        {
            TinyIoCContainer = Registry.GetContainer();
            Registry = TinyIoCContainer.Resolve<Registry>();
            EventSource = new EventSource(TinyIoCContainer);
            // Register EventSource
            Registry.StartEventSource(EventSource);

        }
    }
}

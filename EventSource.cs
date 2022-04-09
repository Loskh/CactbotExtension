using Newtonsoft.Json.Linq;
using RainbowMage.OverlayPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CactbotExtension.Actions;
using RainbowMage.OverlayPlugin.EventSources;
using CactbotExtension.Events;

namespace CactbotExtension
{
    public class EventSource : EventSourceBase
    {
        public delegate void HandlerDelegate(JToken payload);

        public EventSource(TinyIoCContainer container)
        : base(container)
        {
            Name = "CactbotExtension";
            LogInfo("Started");
            //InitializeActions();
            InitializeEvents();
        }
        #region Events
        private Events.Events EventsModule;
        private List<string> EventBind = new();
        public void InitializeEvents()
        {
            var type = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name == "Events" && t.IsClass).FirstOrDefault();
            EventsModule = (Events.Events)Activator.CreateInstance(type, this);
            var events = EventsModule.GetType().GetMethods().Where(method => method.GetCustomAttribute<EventNameAttribute>() != null).ToArray();
            foreach (var jsevent in events) {
#if DEBUG
                LogInfo($"{jsevent.Name}@{jsevent.GetCustomAttribute<EventNameAttribute>().EventName}");
#endif
                EventBind.Add(jsevent.GetCustomAttribute<EventNameAttribute>().EventName);
                //var handlerDelegate = (HandlerDelegate)Delegate.CreateDelegate(typeof(HandlerDelegate), EventsModule, jsevent);
                //SetEvents(jsevent.GetCustomAttribute<EventNameAttribute>().EventName, handlerDelegate);
            }
            RegisterEventTypes(EventBind);
        }

        // Sends an event called |event_name| to javascript, with an event.detail that contains
        // the fields and values of the |detail| structure.
        public void DispatchToJS(JSEvent e)
        {
            JObject ev = new JObject();
            ev["type"] = e.EventName();
            ev["detail"] = JObject.FromObject(e);
            DispatchEvent(ev);
        }

        #endregion

        #region Actions
        private Dictionary<string, HandlerDelegate> CmdBind = new Dictionary<string, HandlerDelegate>();
        private Actions.Action ActionsModule;
        /// <summary>
        ///     注册命令
        /// </summary>
        public void InitializeActions()
        {
            var type = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name == "Actions" && t.IsClass).FirstOrDefault();
            ActionsModule = (Actions.Action)Activator.CreateInstance(type, this);
            var commands = ActionsModule.GetType().GetMethods().Where(method => method.GetCustomAttribute<CommandAttribute>() != null).ToArray();
            foreach (var action in commands) {
#if DEBUG
                LogInfo($"{action.Name}@{action.GetCustomAttribute<CommandAttribute>().Command}");
#endif
                var handlerDelegate = (HandlerDelegate)Delegate.CreateDelegate(typeof(HandlerDelegate), ActionsModule, action);
                SetAction(action.GetCustomAttribute<CommandAttribute>().Command, handlerDelegate);
            }

            RegisterEventHandler(Name, DoAction);
            try {

            }
            catch (Exception) {
                //ignored
            }
        }

        /// <summary>
        ///     执行指令对应的方法
        /// </summary>
        /// <param name="command"></param>
        /// <param name="payload"></param>
        public void DoAction(string command, JToken payload)
        {
            try {
                GetAction(command)(payload);
            }
            catch (Exception ex) {
                LogError(ex.ToString());
            }
        }

        /// <summary>
        ///     设置指令与对应的方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="action">对应指令的方法委托</param>
        public void SetAction(string command, HandlerDelegate action)
        {
            CmdBind[command] = action;
        }

        /// <summary>
        ///     获取指令对应的方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <returns>对应指令的委托方法</returns>
        private HandlerDelegate GetAction(string command)
        {
            try {
                return CmdBind[command];
            }
            catch {
                throw new Exception($@"不支持的操作{command}");
            }
        }

        /// <summary>
        ///     清空绑定的委托列表
        /// </summary>
        public void ClearAction()
        {
            CmdBind.Clear();
        }

        private JToken DoAction(JObject jo)
        {
            var command = jo["command"]?.Value<string>() ?? "null";
            var payload = jo["payload"];
            DoAction(command, payload);
            return null;
        }
        #endregion
        #region dummy

        public override Control CreateConfigControl()
        {
            return null;
        }

        public override void LoadConfig(IPluginConfig config)
        {
            return;
        }

        public override void SaveConfig(IPluginConfig config)
        {
            return;
        }

        protected override void Update()
        {
            return;
        }
        #endregion
        #region Log
        public interface ILogger
        {
            void LogDebug(string format, params object[] args);
            void LogError(string format, params object[] args);
            void LogWarning(string format, params object[] args);
            void LogInfo(string format, params object[] args);
        }
        public void LogDebug( params object[] args)
        {
            this.Log(LogLevel.Debug, "CactbotExtension:{0}", args);
        }
        public void LogError(params object[] args)
        {
            this.Log(LogLevel.Error, "CactbotExtension:{0}", args);
        }
        public void LogWarning(params object[] args)
        {
            this.Log(LogLevel.Warning, "CactbotExtension:{0}", args);
        }
        public void LogInfo( params object[] args)
        {
            this.Log(LogLevel.Info, "CactbotExtension:{0}", args);
        }
        #endregion
    }
}

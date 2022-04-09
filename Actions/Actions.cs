using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CactbotExtension.Actions
{
    internal class Action
    {
        private EventSource ce;
        public Action(EventSource cactbotExtensionEventSource) {
            this.ce = cactbotExtensionEventSource;
        }

        [Command("Say")]
        public void Say(JToken p) {
            ce.LogInfo("test");
        }
    }
}

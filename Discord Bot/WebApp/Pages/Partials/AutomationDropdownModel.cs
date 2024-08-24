using Discord;
using Models;

namespace Models
{
    public class AutomationInfo
    {
        public int id;
        public string input;
        public string desc;

        // Constructor
        public AutomationInfo(int id = -1, string input = "", string desc = "")
        {
            this.id = id;
            this.input = input;
            this.desc = desc;
        }
    }
}

namespace WebApp.Pages.Partials
{
    public class AutomationDropdownModel
    {
        public string ButtonName { get; set; }
        public IEnumerable<AutomationInfo> Automations { get; set; }
        public string DropdownId { get; set; }

		//WHENs
		static AutomationInfo when_nothing = new AutomationInfo(0, "none", "None");
		static AutomationInfo when_types = new AutomationInfo(1, "text", "When a user types this message. (Input text)");
        static AutomationInfo when_reacts = new AutomationInfo(2, "emoji", "When a user reacts with this emoji. (Input emoji ID)");
		static AutomationInfo when_item = new AutomationInfo(3, "item", "When a user uses this item. (Input item ID)");

		//IFs
		static AutomationInfo if_nothing = new AutomationInfo(0, "none", "None");
		static AutomationInfo if_role = new AutomationInfo(1, "role", "If they have this role. (Input role ID)");
        static AutomationInfo if_channel = new AutomationInfo(2, "channel", "If in this channel. (Input channel ID)");

		//Dos
		static AutomationInfo do_nothing = new AutomationInfo(0, "none", "None");
		static AutomationInfo do_reply = new AutomationInfo(1, "text", "Respond with this message. (Input text)");
        static AutomationInfo do_react = new AutomationInfo(2, "emoji", "React with this emoji. (Input emoji ID)");
		static AutomationInfo do_role = new AutomationInfo(3, "role", "Give them this role. (Input role ID)");

		//Arrays
		public AutomationInfo[] con_When = { when_nothing, when_types, when_reacts, when_item };
        public AutomationInfo[] con_If = { if_nothing, if_role, if_channel };
        public AutomationInfo[] con_Do = { do_nothing, do_reply, do_react, do_role };
    }
}

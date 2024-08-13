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
        public bool AddNoneOption { get; set; }
        public IEnumerable<AutomationInfo> Automations { get; set; }
        public string DropdownId { get; set; }

        //WHENs
        static AutomationInfo when_types = new AutomationInfo(0, "text", "When a user types this message.");
        static AutomationInfo when_reacts = new AutomationInfo(1, "emoji", "When a user reacts with this emoji.");

        //IFs
        static AutomationInfo if_role = new AutomationInfo(0, "role", "If they have this role.");
        static AutomationInfo if_channel = new AutomationInfo(1, "channel", "If in this channel.");

        //Dos
        static AutomationInfo do_reply = new AutomationInfo(0, "text", "Respond with this message.");
        static AutomationInfo do_react = new AutomationInfo(1, "emoji", "React with this emoji.");

        //Arrays
        public AutomationInfo[] con_When = { when_types, when_reacts };
        public AutomationInfo[] con_If = { if_role, if_channel };
        public AutomationInfo[] con_Do = { do_reply, do_react };
    }
}

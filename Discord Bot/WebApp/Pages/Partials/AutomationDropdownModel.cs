using Discord;
using Models;

namespace Models
{
    public class Automation
    {
        public int id;
        public string input;
        public string desc;

        // Constructor
        public Automation(int id = -1, string input = "", string desc = "")
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
        public IEnumerable<Automation> Automations { get; set; }
        public string DropdownId { get; set; }

        //WHENs
        static Automation when_types = new Automation(0, "", "When a user types.");
        static Automation when_reacts = new Automation(1, "", "When a user reacts.");

        //IFs
        static Automation if_role = new Automation(0, "role", "If they have this role.");
        static Automation if_channel = new Automation(1, "channel", "If in this channel.");

        //Dos
        static Automation do_reply = new Automation(0, "text", "Respond with this message.");
        static Automation do_react = new Automation(1, "emoji", "React with this emoji.");

        //Arrays
        public Automation[] con_When = { when_types, when_reacts };
        public Automation[] con_If = { if_role, if_channel };
        public Automation[] con_Do = { do_reply, do_react };
    }
}

using Discord;
using Models;

namespace Models
{
    public class Conditional
    {
        public int id;
        public string input;
        public string desc;

        // Constructor
        public Conditional(int id = -1, string input = "", string desc = "")
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
        public IEnumerable<Conditional> Conditionals { get; set; }
        public string DropdownId { get; set; }

        //WHENs
        static Conditional when_types = new Conditional(0, "", "When a user types.");
        static Conditional when_reacts = new Conditional(1, "", "When a user reacts.");

        //IFs
        static Conditional if_role = new Conditional(0, "role", "If they have this role.");
        static Conditional if_channel = new Conditional(1, "channel", "If in this channel.");

        //Dos
        static Conditional do_reply = new Conditional(0, "text", "Respond with this message.");
        static Conditional do_react = new Conditional(1, "emoji", "React with this emoji.");

        //Arrays
        public Conditional[] con_When = { when_types, when_reacts };
        public Conditional[] con_If = { if_role, if_channel };
        public Conditional[] con_Do = { do_reply, do_react };
    }
}

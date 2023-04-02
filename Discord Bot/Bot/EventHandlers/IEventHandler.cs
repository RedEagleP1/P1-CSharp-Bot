using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
    public interface IEventHandler
    {
        public void Subscribe();
    }
}

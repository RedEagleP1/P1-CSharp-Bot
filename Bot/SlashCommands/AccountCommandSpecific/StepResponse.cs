using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.AccountCommandSpecific
{
    public class StepResponse
    {
        public static StepResponse Empty = new StepResponse((request) => { return Task.CompletedTask; }, (request) => { return true; });
        public Func<Request, Task> Response { get; private set; }
        public Func<Request, bool> ShouldIRespond { get; private set; }
        public StepResponse(Func<Request, Task> response, Func<Request, bool> shouldIRespond)
        {
            Response = response;
            ShouldIRespond = shouldIRespond;
        }
    }
}

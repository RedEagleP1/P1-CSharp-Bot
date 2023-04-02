using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public class CustomResponse : IResponse
    {
        Conditions triggerConditions;
        Func<Request, Task> response;
        public CustomResponse WithResponse(Func<Request, Task> response)
        {
            this.response = response;
            return this;
        }
        public CustomResponse WithConditions(Conditions conditions)
        {
            triggerConditions = conditions;
            return this;
        }
        public async Task HandleResponse(Request request)
        {
            await response.Invoke(request);
        }

        public bool ShouldRespond(Request request)
        {
            return triggerConditions.CheckConditions(request);
        }
    }
}

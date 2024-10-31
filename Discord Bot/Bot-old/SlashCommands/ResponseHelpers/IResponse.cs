using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public interface IResponse
    {
        public bool ShouldRespond(Request request);
        public Task HandleResponse(Request request);
    }
}

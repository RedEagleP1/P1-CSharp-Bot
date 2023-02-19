using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class DBReadWrite
    {
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        public static async Task LockReadWrite()
        {
            await semaphoreSlim.WaitAsync();
        }

        public static void ReleaseLock()
        {
            semaphoreSlim.Release();
        }
    }
}

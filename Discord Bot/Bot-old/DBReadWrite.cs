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
        public static bool IsLocked { get; private set; }

        public static async Task LockReadWrite()
        {
            await semaphoreSlim.WaitAsync();
            IsLocked = true;
        }

        public static void ReleaseLock()
        {
            semaphoreSlim.Release();
            IsLocked = false;
        }
    }
}

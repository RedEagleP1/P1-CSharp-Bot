using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public static class DBReadWrite
    {
        private static readonly object readWriteLock = new object();

        public static void SafeReadWrite(Action<ApplicationDbContext> readWriteAction, ApplicationDbContext parameter)
        {
            lock (readWriteLock)
            {
                readWriteAction(parameter);
            }
        }
    }
}

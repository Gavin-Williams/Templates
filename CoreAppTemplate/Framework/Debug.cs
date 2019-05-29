using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public static class Debug
    {
        public static void LogMessage(string name)
        {
#if DEBUG
            int thread = Environment.CurrentManagedThreadId;
            System.Diagnostics.Debug.WriteLine("T: " + thread + " | " + name);
#endif
        }
    }
}

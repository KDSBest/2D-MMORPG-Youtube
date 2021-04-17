using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer
{
    public static class TaskExtensionMethods
    {
        public static void FireAndForget(this Task task)
        {
            Task.Run(async () => await task).ConfigureAwait(false);
        }
    }
}

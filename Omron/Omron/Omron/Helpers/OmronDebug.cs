using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Omron.Helpers
{
    public static class OmronDebug
    {
        static Stopwatch sw = new Stopwatch();
        public static TimeSpan TimeFunc(Action act, int trials)
        {
            sw.Restart();
            for (int i = 0; i < trials; i++)
                act();
            sw.Stop();
            return sw.Elapsed;
        }
    }
}

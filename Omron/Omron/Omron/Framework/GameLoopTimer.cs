using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Microsoft.Xna.Framework;

using Omron.Helpers;
using System.Threading;

namespace Omron.Framework
{
    public class GameLoopTimer
    {
        System.Timers.Timer timer;
        public event Action<GameTime> Update;

        DateTime globalStart;
        public GameLoopTimer(double interval)
        {
            timer = new System.Timers.Timer(interval);
            timer.SynchronizingObject = new ElapsedEventReceiver(ThreadPriority.Highest);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }
        public void Start()
        {
            timer.Start();
            globalStart = DateTime.Now;
        }
        public void Stop()
        {
            timer.Stop();
        }

        DateTime lastTriggered;
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var intv = e.SignalTime - lastTriggered;
            lastTriggered = e.SignalTime;
            var gmTm = new GameTime(e.SignalTime - globalStart, intv);
            if (Update != null)
                Update(gmTm);
        }
    }
}

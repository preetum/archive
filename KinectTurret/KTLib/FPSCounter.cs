using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KTLib
{
    public class FPSCounter
    {
        DateTime lastTime;
        TimeSpan accumTime;
        int curFrames;

        int lastFps;
        public int FPS
        {
            get
            {
                return lastFps;
            }
        }

        public FPSCounter()
        {
            lastTime = DateTime.Now;
            curFrames = 0;
            lastFps = 0;
        }
        public void PushFrame()
        {
            curFrames++;

            var dtime = DateTime.Now - lastTime;
            lastTime = DateTime.Now;

            accumTime = accumTime.Add(dtime);

            if (accumTime.TotalSeconds > 1)
            {
                int sec = (int)accumTime.TotalSeconds;
                lastFps = curFrames / sec;
                curFrames = 0;
                accumTime = accumTime.Subtract(TimeSpan.FromSeconds(sec));
            }

        }
    }
}

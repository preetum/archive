using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omron.Framework
{
    public delegate void TimerEventHandler(GameTime gameTime);

    public class Timer
    {
        /// <summary>
        /// the delay between triggers
        /// </summary>
        public float Interval;
        float tAccum;

        bool triggerFirst;

        public bool IsRunning;

        /// <summary>
        /// fired with a delay of Interval
        /// </summary>
        public event TimerEventHandler Triggered;
        void onTriggered(GameTime gameTime)
        {
            if (Triggered != null)
                Triggered(gameTime);
        }

        DateTime startTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">the delay between triggers</param>
        public Timer(float interval)
            :this(interval, false)
        { 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">the delay between triggers</param>
        /// <param name="triggerFirst">set to true to trigger on the first update, then wait. (as opposed to waiting [interval] before the first trigger)</param>
        public Timer(float interval, bool triggerFirst)
        {
            this.triggerFirst = triggerFirst;
            Interval = interval;
            tAccum = triggerFirst ? interval : 0f;
            IsRunning = false;
        }
        /// <summary>
        /// effectivly a pause and reset
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            Reset();
        }
        /// <summary>
        /// clears the accumulator (or prepares for first trigger), does not stop the timer.
        /// </summary>
        public void Reset()
        {
            tAccum = triggerFirst ? Interval : 0f;
            startTime = DateTime.Now;
        }
        /// <summary>
        /// reset then start
        /// </summary>
        public void Restart()
        {
            Reset();
            Start();
        }
        public void Start()
        {
            if(!IsRunning)
                startTime = DateTime.Now;

            IsRunning = true;
        }
        /// <summary>
        /// pauses timer without clearing accumulator (no reset)
        /// </summary>
        public void Pause()
        {
            IsRunning = false;
        }
        public void Update(GameTime gameTime)
        {
            if (IsRunning)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                tAccum += dt;

                if (tAccum >= Interval)
                {
                    onTriggered(new GameTime(TimeSpan.FromSeconds(0f), TimeSpan.FromSeconds(tAccum)));
                    tAccum -= Interval;

                    if (tAccum > Interval)
                        tAccum = 0f; //prevent multiple consecutive update if game is running too slowly
                }
            }
        }
    }
}

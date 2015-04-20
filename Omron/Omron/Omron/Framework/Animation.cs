using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Omron.Actors;
using System.Diagnostics.Contracts;

namespace Omron.Framework
{
    [Serializable]
    public class AnimationData
    {
        public float SclWidth, SclHeight;
        public OffsetData Offset;
        public float FPS;
        public bool Loop;
        public bool Reverse;
        public string ImageName;
    }

    public enum AnimationType : byte
    {
        Idle,
        Move,
        Work,
        MeleeAttack,
        RangedAttack,
        Build
    }

    public class Animation
    {
        Texture2D[] frames;
        byte currFrame = 0;
        public bool Loop;

        public bool Reverse = false;

        FrameData frameTemplate;

        public void GoToFrameRatio(float rat)
        {
            Contract.Requires(0 <= rat && rat < 1); //by the way, if rat is one then it will fail (0 <= rat < 1)
            currFrame = (byte)(frames.Length * rat);
        }

        public byte FrameIndex
        {
            get { return currFrame; }
            set
            {
                if (value >= 0 && value < frames.Length)
                    currFrame = value;
            }
        }

        public float FPS;

        Timer frameTimer;

        public bool IsComplete;


        public Animation(AnimationData animData)
        {
            frames = ResourceManager.Resources[animData.ImageName];
            FPS = animData.FPS;
            frameTemplate = new FrameData(null, animData.SclWidth, animData.SclHeight, animData.Offset);
            Loop = animData.Loop;
            Reverse = animData.Reverse;

            float fps = FPS;
            if (fps == 0f)
            {
                fps = 1f;
            }
            frameTimer = new Timer(1f / fps, true);
            frameTimer.Triggered += new TimerEventHandler(frameTimer_Triggered);



            IsComplete = false;
        }
        //stops timer and resets frame #
        public void Stop()
        {
            frameTimer.Stop();
            Reset();
        }
        /// <summary>
        /// resets frame num to 0 without stopping animation
        /// </summary>
        public void Reset()
        {
            frameTimer.Reset();
            currFrame = 0;
            IsComplete = false;
            inc = true;
        }
        public void Pause()
        {
            frameTimer.Pause();
        }
        public void Start()
        {
            frameTimer.Start();
        }
        bool inc = true;
        void frameTimer_Triggered(GameTime gameTime)
        {
            if (inc)
            {
                if (currFrame < frames.Length - 1)
                    currFrame++;
                else
                {
                    if (Reverse)
                    {
                        inc = false;
                        if (currFrame > 0)
                            currFrame--;
                    }
                    else if (Loop)
                    {
                        currFrame = 0;
                    }
                    else
                    {
                        frameTimer.Stop();
                        IsComplete = true;
                    }
                }
            }
            else
            {
                if (currFrame > 0)
                    currFrame--;
                else
                {
                    if (Loop)
                    {
                        inc = true;
                        if (frames.Length > 1)
                            currFrame++;
                    }
                    else
                    {
                        frameTimer.Stop();
                        IsComplete = true;
                    }
                }
            }
        }
        public FrameData GetCurrentFrame()
        {
            frameTemplate.Image = frames[currFrame];
            return frameTemplate;
        }
        public void Update(GameTime gameTime)
        {
            if (FPS != 0)
                frameTimer.Update(gameTime);
        }
    }
}

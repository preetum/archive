using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainGame.GameWorld
{
    class Pulse
    {
        public Vector2 Root;
        public DateTime StartTime;
        public float Radius;
        public float SpeedPx = 200f; //300
        public float FadeTail = 500f; //500

        public bool PlayerOriginated = true;

        public float GetAge(DateTime currTime)
        {
            return (float)(currTime - StartTime).TotalSeconds;
        }
        public float GetRadius(DateTime currTime)
        {
            var rad = SpeedPx * GetAge(currTime);
            return Math.Min(rad, Radius);
        }
    }
    class PulseManager
    {
        public List<Pulse> pulses;

        GraphicsDevice GraphicsDevice;
        SpriteBatch spriteBatch;

        Texture2D scratch;
        Effect pulseGen;

        RenderTarget2D mask;


        int w, h;

        public PulseManager(GraphicsDevice gd, int w, int h)
        {
            GraphicsDevice = gd;
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
            pulses = new List<Pulse>();

            pulseGen = ResourceManager.Resources["pulseGen"];

            this.w = w;
            this.h = h;
            scratch = new Texture2D(GraphicsDevice, w, h);


           mask = new RenderTarget2D(
                    GraphicsDevice,
                    w,
                    h,
                    false,
                    GraphicsDevice.PresentationParameters.BackBufferFormat,
                    DepthFormat.Depth24);
                    }

        public void StartPulse(Vector2 root, float radius, bool playerOriginated)
        {
            Pulse p = new Pulse();
            p.Root = root;
            p.Radius = radius;
            p.StartTime = DateTime.Now;
            p.PlayerOriginated = playerOriginated;

            pulses.Add(p);
        }
        public void StartPulse(Vector2 root, float radius)
        {
            StartPulse(root, radius, true);
        }

        public void Update(GameTime gtime)
        {
            List<Pulse> newPulses = new List<Pulse>();
            foreach (var p in pulses)
            {
                if (p.GetAge(DateTime.Now) < 2 * (p.Radius + p.FadeTail) / p.SpeedPx) //2* for safety
                {
                    newPulses.Add(p);
                }
            }
            pulses = newPulses;
        }

        public Texture2D RenderMask()
        {



            Vector2 size = new Vector2(w, h);

            GraphicsDevice.SetRenderTarget(mask);
            GraphicsDevice.Clear(Color.Black);


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, null, null);

            //additional fade time is roughly FadeTail/Radius
            pulseGen.Parameters["Size"].SetValue(new Vector2(w, h));

            foreach (Pulse p in pulses)
            {
                pulseGen.Parameters["Root"].SetValue(p.Root);
                pulseGen.Parameters["Radius"].SetValue(p.Radius);
                pulseGen.Parameters["SpeedPx"].SetValue(p.SpeedPx);
                pulseGen.Parameters["FadeTail"].SetValue(p.FadeTail);
                pulseGen.Parameters["Time"].SetValue(p.GetAge(DateTime.Now));

                pulseGen.CurrentTechnique.Passes[0].Apply();

                spriteBatch.Draw(scratch, Vector2.Zero, Color.White);
            }
            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);



            return mask;
        }
    }
}

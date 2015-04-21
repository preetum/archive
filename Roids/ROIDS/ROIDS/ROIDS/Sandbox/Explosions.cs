using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using GameCore;
using WorldCore;
using UICore;
using PhysicsCore;
using Utilities;

namespace ROIDS.Sandbox
{
    [Demo]
    class EXPLOSIONSFUCKYEAH : GameState
    {
        PhysicsEngine PE;
        UIEngine _uiEngine;

        Texture2D fire;
        Texture2D smoke;

        Size winSize = DefaultSettings.Settings["WindowSize"];

        public override void Load()
        {

            Game currentGame = Globals.Data["Game"];
            SpriteBatch spriteBatch = Globals.Data["SpriteBatch"];
            currentGame.IsMouseVisible = true;

            // Already done by XNAGAme
            //GraphicsUtils.Load(spriteBatch,
            //    ContentRepository.Repository["Pixel"],
            //    ContentRepository.Repository["Ball"]);

            fire = ContentRepository.Repository["fire"];
            smoke = ContentRepository.Repository["smoke"];

            PE = new PhysicsEngine(new Region(0, winSize.Width, 0, winSize.Height), 10);
            PE.AddUniversalForce(DefaultForces.LinearDrag);


            //UI stuff...
            _uiEngine = new UIEngine();
            var frame = new UIFrames.BasicFrame();

            frame.MouseClick += new MouseEventHandler(frame_MouseClick);

            _uiEngine.AddAndLoad(frame);


            Console.WriteLine("CONTROLS:");
            Console.WriteLine("[RCLICK] - detailed 310 particle explosion");
            Console.WriteLine("[LCLICK] - dirty 35 particle explosion");
        }

        void frame_MouseClick(Element sender, MouseEventArgs e)
        {
            if (e.isClicked(MouseButtons.Right))
            {
                var pfire = new ParticleSystem(new Vector2(e.CurrentMouseState.X, e.CurrentMouseState.Y), 15, 100, 1, 10, 1, 3f, fire);
                pfire.InitVelocities(0, 110);
                PE.AddParticleSystem(pfire);

                var psmoke = new ParticleSystem(new Vector2(e.CurrentMouseState.X, e.CurrentMouseState.Y), 25, 100, 2f, 20, 1, 1f, smoke);
                psmoke.InitVelocities(0, 200);
                PE.AddParticleSystem(psmoke);
            }
            else if (e.isClicked(MouseButtons.Left))
            {
                PE.AddParticleSystems(ParticleSystemFactory.GetDirtyBomb(new Vector2(e.CurrentMouseState.X, e.CurrentMouseState.Y), 50));
            }
        }
        public override void Update(GameTime time)
        {
            if (!_uiEngine.Update(time))
                this.Exit();

            PE.Update((float)(time.ElapsedGameTime.Milliseconds) / 1000f);
        }
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;

            if (time.IsRunningSlowly)
                graphicsDevice.Clear(Color.DarkGray);
            else
                graphicsDevice.Clear(Color.Black);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            PE.PSystems.ForEach(ps => ps.Draw());
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
        }
    }
}

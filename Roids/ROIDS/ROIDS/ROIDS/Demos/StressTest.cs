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
using ROIDS.GameStates;
using ROIDS.GameObjects;
using ROIDS.GameObjects.Asteroids;

namespace ROIDS.GameStates
{
    [Demo] 
    class StressTest : PlayableState
    {
        float delay = .01f;
        float dint = 0.0f;

        Size winSize;

        bool draw = true;

        public override void Load()
        {
            UIManager = new UIEngine();
            var frame = new UIFrames.BasicFrame();
            frame.KeyPressDown += new KeyEventHandler(frame_KeyPressDown);
            UIManager.AddAndLoad(frame);


            Reload();
        }

        void frame_KeyPressDown(Element sender, KeyEventArgs e)
        {
            if (e.InterestingKeys[0] == Keys.OemPeriod)
                draw = !draw;
        }




        public override void Reload()
        {
            winSize = DefaultSettings.Settings["WindowSize"];
            PhysicsManager = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10);

            PhysicsManager.AddUniversalForce(DefaultForces.LinearDrag);
            //PhysicsManager.AddUniversalForce(DefaultForces.Gravity);


            var world = new List<WorldObject>();
            Random rand = new Random();


            ActiveMap = new Map(world);  // Create map from world

            // Walls -- invisible
            var w0 = new ROIDS.Sandbox.VertWallBody(new Vector2(0, winSize.Height / 2), winSize.Height);
            var w1 = new ROIDS.Sandbox.VertWallBody(new Vector2(winSize.Width, winSize.Height / 2), winSize.Height);

            var w2 = new ROIDS.Sandbox.HorizWallBody(new Vector2(winSize.Width / 2, 0), winSize.Width);
            var w3 = new ROIDS.Sandbox.HorizWallBody(new Vector2(winSize.Width / 2, winSize.Height), winSize.Width);

            PhysicsManager.MapBodies.Add(w0);
            PhysicsManager.MapBodies.Add(w1);
            PhysicsManager.MapBodies.Add(w2);
            PhysicsManager.MapBodies.Add(w3);

            //
            Console.WriteLine("HIT PERIOD TO DISABLE GRFX");
        }

        public override void Update(GameTime time)
        {
            if (!UIManager.Update(time))
                base.Exit();
            ActiveMap.UpdateDynamicObjects(time);
            PhysicsManager.Update(time.ElapsedGameTime.Milliseconds / 1000f);

            if (!time.IsRunningSlowly)
            {
                dint -= time.ElapsedGameTime.Milliseconds / 1000f;
                if (dint <= 0.0f)
                {
                    dynamic obj;
                    if (MathUtils.Rand.Next(0, 2) == 0)
                    {
                        obj = new BombRoid(new Vector2((float)MathUtils.Rand.Next(60, (int)winSize.Width - 60), (float)MathUtils.Rand.Next(60, (int)winSize.Height - 60)), MathUtils.Rand.Next(2, 10));
                    }
                    else
                    {
                        obj = new InertRoid(new Vector2((float)MathUtils.Rand.Next(60, (int)winSize.Width - 60), (float)MathUtils.Rand.Next(60, (int)winSize.Height - 60)), MathUtils.Rand.Next(2, 10));               
                    }
                    obj.Velocity = 1000 * (float)MathUtils.Rand.NextDouble() * MathUtils.RandDirection();
                    PhysicsManager.ActiveBodies.Add(obj);
                    ActiveMap.AddObject(obj);

                    Console.WriteLine(PhysicsManager.ActiveBodies.Count.ToString() + " bodies");
                    dint = delay;
                }
            }
            else
            {
                Console.Write(PhysicsManager.ActiveBodies.Count.ToString() + " bodies     ");
                Console.WriteLine("SLOW - HIT PERIOD TO DISABLE GRFX");
                dint = 0.0f;
            }
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (time.IsRunningSlowly)
                spriteBatch.GraphicsDevice.Clear(Color.IndianRed);
            else
                spriteBatch.GraphicsDevice.Clear(Color.Black);

            if (draw)
            {
                UIManager.Draw(time, spriteBatch);
                ActiveMap.WorldObjects.ForEach(
                    obj => ((GameObjects.Actor)obj).Draw(time, spriteBatch));

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                PhysicsManager.PSystems.ForEach(ps => ps.Draw());
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            }
        }

        public override void EndGame()
        {
            throw new NotImplementedException();
        }



    }
}

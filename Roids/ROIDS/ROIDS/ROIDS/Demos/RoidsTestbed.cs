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
using ROIDS.GameObjects.Devices;

namespace ROIDS.GameStates
{
     [Demo]
    class RoidsTestbed : PlayableState
    {
        private KeyboardController _controller;

        Size winSize;

        public override void Load()
        {
            UIManager = new UIEngine();
            var frame = new UIFrames.BasicFrame();
            frame.MouseClick += new MouseEventHandler(frame_MouseClick);
            frame.KeyPressDown += new KeyEventHandler(frame_KeyPressDown);
            frame.MouseOver += new MouseEventHandler(frame_MouseOver);
            UIManager.AddAndLoad(frame);

            _controller = new KeyboardController(UIManager.ActiveFrame, KeyMappings.PolarMouseMapping);
            _controller.KeyControlEngaged += new KeyControlEventHandler(_controller_KeyControlEngaged);
            _controller.KeyControlDisengaged += new KeyControlEventHandler(_controller_KeyControlDisengaged);


            Reload();
        }


        public override void Reload()
        {
            winSize = DefaultSettings.Settings["WindowSize"];
            PhysicsManager = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10);

            PhysicsManager.AddUniversalForce(DefaultForces.LinearDrag);
            //PhysicsManager.AddUniversalForce(DefaultForces.Gravity);


            var world = new List<WorldObject>();
            ActiveMap = new Map(world);
            Random rand = new Random();

            var player = new Player(
                new Vector2((float)rand.NextDouble() * (winSize.Width - 50), (float)rand.NextDouble() * (winSize.Height - 50)),
                0f);

            SpawnObject<Actor>(player);

            //
            //
            //


            int goodRoidNumber = 10;
            int BadRoidNumber = 5;

            for (int i = 0; i < goodRoidNumber; i++)
            {
                var pos = new Vector2((float)MathUtils.Rand.Next(50, (int)winSize.Width - 50), (float)MathUtils.Rand.Next(50, (int)winSize.Height - 50));
                var rad = 15f + (float)MathUtils.Rand.NextDouble() * 20;
                var gr = new GoodRoid(pos, rad, 50f, .1f, Vector2.Zero);
                SpawnObject<Actor>(gr);
            }

            for (int i = 0; i < BadRoidNumber; i++)
            {
                var pos = new Vector2((float)MathUtils.Rand.Next(50, (int)winSize.Width - 50), (float)MathUtils.Rand.Next(50, (int)winSize.Height - 50));
                var rad = 15f + (float)MathUtils.Rand.NextDouble() * 20;
                var br = new BadRoid(pos, rad, 50f, .1f, Vector2.Zero);
                SpawnObject<Actor>(br);
            }




            //
            //
            //


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
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("[LCLICK] - launch charge");
            Console.WriteLine("[RCLICK] - launch sensor");
            Console.WriteLine("[SPACE] - detonate all charges");
        }


        void frame_MouseClick(Element sender, MouseEventArgs e)
        {
            var player = (Player)ActiveMap.Player;
            var mdir = Vector2.Normalize(e.CurrentPosition - player.Position);
            var launchPort = player.Position + mdir * (player.Radius + 5); //launch stuff from slightly in front of player
            float launchVel = 300f + player.Velocity.Length();

            var carrier = new DeviceTransporter(launchPort, 15);
            carrier.Velocity = launchVel * mdir;
            SpawnObject<Actor>(carrier);
            
            if (e.isClicked(MouseButtons.Left))
            {
                var c = new Charge(50, 15);
                ActiveMap.AddObject(c);
                carrier.AddDevice(c);             
            }
            if (e.isClicked(MouseButtons.Right))
            {
                var s = new Sensor(60f);
                ActiveMap.AddObject(s);
                carrier.AddDevice(s);
            }
        }
        void frame_MouseOver(Element sender, MouseEventArgs e)
        {
            ((IRigidBody)ActiveMap.Player).Rotation = MathUtils.GetAngle(e.CurrentPosition - ((IRigidBody)ActiveMap.Player).Position);
        }

        void frame_KeyPressDown(Element sender, KeyEventArgs e)
        {
            switch (e.InterestingKeys[0])
            {
                case Keys.Space:
                    var objclone = new List<WorldObject>(ActiveMap.WorldObjects);
                    foreach (var obj in objclone)
                    {
                        if (obj is DeviceCarrierActor)
                        {
                            var devs = ((DeviceCarrierActor)obj).GetDevices();
                            foreach (var d in devs)
                            {
                                if (d is Charge) ((Charge)d).Detonate(1/1000);
                            }
                        }
                    }
                    break;
            }
        }

        public override void Update(GameTime time)
        {
            if (!UIManager.Update(time))
                base.Exit();

            ActiveMap.UpdateDynamicObjects(time);
            PhysicsManager.Update(time.ElapsedGameTime.Milliseconds / 1000f);
        }

        void _controller_KeyControlEngaged(int control)
        {
            KeyMappings.PolarMouseGameControls c = (KeyMappings.PolarMouseGameControls)control;
            var player = (Player)ActiveMap.Player;
            Vector2 mouseDirection = (new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) - player.Position;
            var rad = mouseDirection.Length();
            mouseDirection.Normalize();

            switch (c)
            {
                case KeyMappings.PolarMouseGameControls.MoveIn:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * mouseDirection));
                    break;
                case KeyMappings.PolarMouseGameControls.MoveOut:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, -player.Mass * player.SpeedScale * mouseDirection));
                    break;
                case KeyMappings.PolarMouseGameControls.MoveClockwise:
                    var tangentCW = mouseDirection.RotateInPlace(-(float)Math.PI / 2);
                    var tangVel = Vector2.Dot(player.Velocity, tangentCW);
                    player.Velocity = tangentCW * tangVel;
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * tangentCW));
                    break;
                case KeyMappings.PolarMouseGameControls.MoveCounterClockwise:
                    var tangentCCW = mouseDirection.RotateInPlace((float)Math.PI / 2);
                    var tangVelCCW = Vector2.Dot(player.Velocity, tangentCCW);
                    player.Velocity = tangentCCW * tangVelCCW;
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * tangentCCW));
                    break;
            }
        }

        void _controller_KeyControlDisengaged(int control)
        {
            KeyMappings.PolarMouseGameControls c = (KeyMappings.PolarMouseGameControls)control;
            var player = (Player)ActiveMap.Player;
            switch (c)
            {
                case KeyMappings.PolarMouseGameControls.MoveIn:

                    break;
                case KeyMappings.PolarMouseGameControls.MoveOut:

                    break;
                case KeyMappings.PolarMouseGameControls.MoveClockwise:
                    player.Velocity /= 2f;
                    break;
                case KeyMappings.PolarMouseGameControls.MoveCounterClockwise:
                    player.Velocity /= 2f;
                    break;
            }
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {

            UIManager.Draw(time, spriteBatch);
            ActiveMap.WorldObjects.ForEach(
                obj => ((WorldCore.IDrawable)obj).Draw(time, spriteBatch));

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            PhysicsManager.PSystems.ForEach(ps => ps.Draw());
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

        }

        public override void EndGame()
        {
            throw new NotImplementedException();
        }
    }
}

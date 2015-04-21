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
    [Demo] //Too lazy to add a new attribute, so whatevs. <---- AAAAHH A HACK. A WILD HACK HAS APPEARED. DO YOU: A. REMOVE IT. B. LEAVE IT DUE TO LAZINESS. C. TASK IT TO PRAMOD TO FIX.
           // ^^ I think this is fine for now, since we're going to have a different loader for the final game. 
    class ExplosionSandbox : PlayableState
    {
        private KeyboardController _controller;

        Size winSize;
        bool gactive = false;
        bool draw = true;

        public override void Load()
        {
            UIManager = new UIEngine();
            var frame = new UIFrames.BasicFrame();
            frame.MouseClick += new MouseEventHandler(frame_MouseClick);
            frame.MouseOver += new MouseEventHandler(frame_MouseOver);
            frame.MouseDown += new MouseEventHandler(frame_MouseDown);
            frame.KeyPressDown += new KeyEventHandler(frame_KeyPressDown);
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

            int maxAstroids = 10;


            var world = new List<WorldObject>();
            Random rand = new Random();

            var player = new Player(
                new Vector2((float)rand.NextDouble() * (winSize.Width - 50), (float)rand.NextDouble() * (winSize.Height - 50)),
                0f);

            world.Add(player);
            PhysicsManager.ActiveBodies.Add(player);



            int nx = 30;
            int ny = 30;
            var rad = 4f;
            var buf = 2f;
            var pos = new Vector2(winSize.Width / 2 - nx * (2f * rad + buf) / 2f, winSize.Height / 2 - ny * (2f * rad + buf) / 2f);
            for (int xi = 0; xi < nx; xi++)
            {
                for (int yi = 0; yi < ny; yi++)
                {
                    var pert = 0.1f * buf * (float)MathUtils.Rand.NextDouble() * MathUtils.RandDirection(); //minor perturbation from perfect grid.
                    var cent = pos + new Vector2((2f * rad + buf) * xi, (2f * rad + buf) * yi) + pert;
                    var obs = new InertRoid(cent, rad);

                    world.Add(obs);
                    PhysicsManager.ActiveBodies.Add(obs);
                }
            }



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
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("[LCLICK] - trigger explosion");
            Console.WriteLine("[RCLICK] - infect area");
        }

        public override void Update(GameTime time)
        {
            if (!UIManager.Update(time))
                base.Exit();
            ActiveMap.UpdateDynamicObjects(time);
            PhysicsManager.Update(time.ElapsedGameTime.Milliseconds / 1000f);
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (draw)
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
        }

        public override void EndGame()
        {
            throw new NotImplementedException();
        }

        void frame_MouseClick(Element sender, MouseEventArgs e)
        {
            if(e.isClicked(MouseButtons.Left))
            {
                var rad = 25;
                PhysicsManager.AddParticleSystems(ParticleSystemFactory.GetDirtyBomb(e.CurrentPosition, rad));
                PhysicsManager.AddInstantaneousForceField(new InstantaneousForceField(e.CurrentPosition, rad, DefaultForces.GenerateExplosiveField(rad, 1)));
            }
            if (e.isClicked(MouseButtons.Right))
            {
            }
            if (e.isClicked(MouseButtons.Middle))
            {
            }
        }

        void frame_MouseDown(Element sender, MouseEventArgs e)
        {
        }

        void frame_MouseOver(Element sender, MouseEventArgs e)
        {
            ((IRigidBody)ActiveMap.Player).Rotation = MathUtils.GetAngle(e.CurrentPosition - ((IRigidBody)ActiveMap.Player).Position);

            if (e.isDown(MouseButtons.Right))
            {
                var rad = PhysicsManager.QTbodies.Query(Region.FromCircle(e.CurrentPosition, 10));
                foreach (var gr in rad)
                {
                    if (gr is InertRoid)
                    {
                        KillObject((Actor)gr);

                        var gra = (InertRoid)gr;
                        var br = new BombRoid(gra.Position, gra.Radius);
                        ActiveMap.AddObject(br);
                        PhysicsManager.ActiveBodies.Add(br);
                    }

                }
            }
        }


        void frame_KeyPressDown(Element sender, KeyEventArgs e)
        {
            switch (e.InterestingKeys[0])
            {
                case Keys.G:
                    gactive = !gactive;
                    if (gactive)
                        PhysicsManager.AddUniversalForce(DefaultForces.Gravity);
                    else
                        PhysicsManager.RemoveUniversalForce(DefaultForces.Gravity);
                    break;
                case Keys.X:
                    var br = new BombRoid(new Vector2((float)MathUtils.Rand.Next(60, (int)winSize.Width - 60), winSize.Height), 25);
                    br.Mass = 100;
                    br.Velocity = -1000 * Vector2.UnitY;
                    br.FuseTime = .1f;
                    ActiveMap.AddObject(br);
                    PhysicsManager.ActiveBodies.Add(br);
                    break;
                case Keys.C:
                    var gr = new InertRoid(new Vector2((float)MathUtils.Rand.Next(60, (int)winSize.Width - 60), winSize.Height), 25);
                    gr.Mass = 100;
                    gr.Velocity = -1000 * Vector2.UnitY;
                    ActiveMap.AddObject(gr);
                    PhysicsManager.ActiveBodies.Add(gr);
                    break;
                case Keys.OemPeriod:
                    draw = !draw;
                    break;

                case Keys.Space:
                   
                    break;
            }
        }

        void _controller_KeyControlEngaged(int control)
        {
            KeyMappings.PolarMouseGameControls c = (KeyMappings.PolarMouseGameControls)control;
            var player = (Player)ActiveMap.Player;
            Vector2 mouseDirection = (new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) - player.Position;
            var rad = mouseDirection.Length();
            mouseDirection.Normalize();

            /*switch (c)
            {
                case KeyMappings.PolarMouseGameControls.MoveIn:          
                    player.Velocity = player.SpeedScale * mouseDirection;
                    break;
                case KeyMappings.PolarMouseGameControls.MoveOut:
                    player.Velocity = -player.SpeedScale * mouseDirection;
                    break;
                case KeyMappings.PolarMouseGameControls.MoveClockwise:
                    //player.Velocity = -player.Speed * Vector2.UnitX;
                    break;
                case KeyMappings.PolarMouseGameControls.MoveCounterClockwise:
                    //player.Velocity = player.Speed * Vector2.UnitX;
                    break;
            }*/
            switch (c)
            {
                case KeyMappings.PolarMouseGameControls.MoveIn:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass*player.SpeedScale * mouseDirection));
                    break;
                case KeyMappings.PolarMouseGameControls.MoveOut:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, -player.Mass*player.SpeedScale * mouseDirection));
                    break;
                case KeyMappings.PolarMouseGameControls.MoveClockwise:
                    var tangentCW = mouseDirection.RotateInPlace(-(float)Math.PI / 2);
                    var tangVel = Vector2.Dot(player.Velocity, tangentCW);
                    player.Velocity = tangentCW * tangVel;
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * tangentCW));
                    break;
                case KeyMappings.PolarMouseGameControls.MoveCounterClockwise:
                    var tangentCCW = mouseDirection.RotateInPlace((float)Math.PI / 2);
                    //player.Velocity = player.SpeedScale/5 * tangentCCW;// + mouseDirection * Vector2.Dot(player.Velocity, mouseDirection);
                    //PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.Velocity * player.Velocity / rad * mouseDirection));
                    var tangVelCCW = Vector2.Dot(player.Velocity, tangentCCW);
                    player.Velocity = tangentCCW * tangVelCCW;
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * tangentCCW));
                    break;
            }
        }

        void _controller_KeyControlDisengaged(int control)
        {
            //if (control <= 4)
            //    ((Player)ActiveMap.Player).Velocity = Vector2.Zero;
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

    }
}

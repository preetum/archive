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
    class Roids : PlayableState
    {
        PlayerProfile PlayerProfile;
        #region Initialize Game
        public override void Reload()
        {
            var winSize = DefaultSettings.Settings["WindowSize"];
            PhysicsManager = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10);

            PhysicsManager.AddUniversalForce(DefaultForces.LinearDrag);


            var world = new List<WorldObject>();
            ActiveMap = new Map(world);



            /*
             * Setup Map
             */

            int goodRoidNumber = 10;
            int badRoidNumber = 5;
            int bombRoidNumber = 5;

            int chargesAllowed = 5;
            int sensorsAllowed = 5;


            PlayerProfile = new PlayerProfile(1000, // Health
                badRoidNumber, goodRoidNumber); // Roids

            Random rand = new Random();

            var player = new Player(
                new Vector2((float)rand.NextDouble() * (winSize.Width - 50), (float)rand.NextDouble() * (winSize.Height - 50)),
                0f);
            player.GotHurt += new PlayerEventHandler(player_GotHurt);
            SpawnObject<Actor>(player);

            for (int i = 0; i < chargesAllowed; i++)
                PlayerProfile.AddCharge(new Charge(50, 15));

            for (int i = 0; i < sensorsAllowed; i++)
                PlayerProfile.AddSensor(new Sensor(60f));

            for (int i = 0; i < goodRoidNumber; i++)
            {
                var pos = new Vector2((float)MathUtils.Rand.Next(50, (int)winSize.Width - 50), (float)MathUtils.Rand.Next(50, (int)winSize.Height - 50));
                var rad = 15f + (float)MathUtils.Rand.NextDouble() * Asteroid.MaxRadius;
                var gr = new GoodRoid(pos, rad, 1000f, .1f, new Vector2((float)MathUtils.Rand.NextDouble() * 300, (float)MathUtils.Rand.NextDouble() * 300));
                gr.Destroyed += new WorldObjectEventHandler(goodRoid_Destroyed);
                SpawnObject<Actor>(gr);
            }

            for (int i = 0; i < badRoidNumber; i++)
            {
                var pos = new Vector2((float)MathUtils.Rand.Next(50, (int)winSize.Width - 50), (float)MathUtils.Rand.Next(50, (int)winSize.Height - 50));
                var rad = 15f + (float)MathUtils.Rand.NextDouble() * Asteroid.MaxRadius;
                var br = new BadRoid(pos, rad, 1000f, .1f, new Vector2((float)MathUtils.Rand.NextDouble()*300,(float)MathUtils.Rand.NextDouble()*300));
                br.Destroyed += new WorldObjectEventHandler(badRoid_Destroyed);
                SpawnObject<Actor>(br);
            }
            for (int i = 0; i < bombRoidNumber; i++)
            {
                var pos = new Vector2((float)MathUtils.Rand.Next(50, (int)winSize.Width - 50), (float)MathUtils.Rand.Next(50, (int)winSize.Height - 50));
                var rad = 15f + (float)MathUtils.Rand.NextDouble() * Asteroid.MaxRadius;
                var br = new BombRoid(pos, rad);
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

            this.Write("Health: {0}\nCharges: {1}\nSensors: {2}\nRoids to Blast: {3}",
                PlayerProfile.Health,
                PlayerProfile.ChargesLeft,
                PlayerProfile.SensorsLeft,
                PlayerProfile.RoidsToBlast);
        }




        public override void Load()
        {
            UIManager = new UIEngine();

            var frame = new Frame();
            frame.KeyUp += new KeyEventHandler(frame_KeyUp);
            frame.MouseClick += new MouseEventHandler(game_MouseClick);
            UIManager.AddAndLoad(frame);

            var keyController = new KeyboardController(frame, KeyMappings.SimpleMapping);
            keyController.KeyControlEngaged += new KeyControlEventHandler(keyController_KeyControlEngaged);
            keyController.KeyControlDisengaged += new KeyControlEventHandler(keyController_KeyControlDisengaged);
            keyController.KeyControlJustEngaged += new KeyControlEventHandler(keyController_KeyControlJustEngaged);

            frame.MouseOver += new MouseEventHandler(mouseController_MouseOver);

            Reload();
        }
        

        #endregion

        #region Clean Up
        public override void EndGame()
        {
            if (!this.isDead)
            {
                this.Write("-----------------\n----Game-Over----: {3}!\nTime: {0}:{1}:{2}\n-----------------",
                    PlayerProfile.TimePlayed.Hours, PlayerProfile.TimePlayed.Minutes, PlayerProfile.TimePlayed.Seconds,
                    PlayerProfile.RoidsToBlast == 0 ? "Yous Won" : "You Lost");
                this.Exit();
            }
        }
        public override void Exit()
        {
            base.Exit();
        }
        #endregion

        bool debugMode;
        #region Frame Controls
        void frame_KeyUp(Element sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Escape))
                this.Exit();
            else if (e.InterestingKeys.Contains<Keys>(Keys.Tab))
            {
                debugMode = !debugMode;
                ActiveMap.DynamicObjects.ForEach(x =>
                    { if (x is Asteroid) ((Asteroid)x).DebugMode = debugMode; });
            }
        }
        #endregion

        #region Game Controls

        // Mouse Controls
        void mouseController_MouseOver(Element sender, MouseEventArgs e)
        {
            ((IRigidBody)ActiveMap.Player).Rotation =
                MathUtils.GetAngle(((IRigidBody)ActiveMap.Player).Position - e.CurrentPosition);
        }

        Charge activeCharge;

        void game_MouseClick(Element sender, MouseEventArgs e)
        {
            if (e.isClicked(MouseButtons.Left))
            {
                //// This code sets of a charge by clicking on the asteroid that contains it, so multiple chargs can be set
                //// Good for debugging
                //activeCharge = null;
                //var bodies =
                //    PhysicsManager.QTbodies.Query(
                //        new Region(e.CurrentPosition.X - Asteroid.MaxRadius, e.CurrentPosition.X + Asteroid.MaxRadius,
                //            e.CurrentPosition.Y - Asteroid.MaxRadius, e.CurrentPosition.Y + Asteroid.MaxRadius));
                //foreach (IRigidBody body in bodies)
                //    if (body is Asteroid)
                //    {
                //        var astr = (Asteroid)body;
                //        if (MathUtils.Distance(astr.Position, e.CurrentPosition) < astr.Radius)
                //        {
                //            activeCharge = (Charge)astr.GetDevices().FirstOrDefault<Device>(x => x is Charge);
                //            break;
                //        }
                //    }
                //// End of above code block
 
                if (activeCharge == null)
                {
                    activeCharge = PlayerProfile.ReleaseCharge(0);
                    if (activeCharge == null)
                        this.Write("No Charges Left!");
                    else
                    {
                        releaseItem(activeCharge);
                        this.Write("Charges Left: {0}", PlayerProfile.ChargesLeft);
                    }
                }
                else
                {
                    activeCharge.Detonate(1/1000);
                    activeCharge = null;
                }
            }
            else if (e.isClicked(MouseButtons.Right))
            {
                var sensor = PlayerProfile.ReleaseSensor(0);
                if (sensor == null)
                    this.Write("No Sensors Left!");
                else
                {
                    releaseItem(sensor);
                    this.Write("Sensors Left: {0}", PlayerProfile.SensorsLeft);
                }
            }
        }

        void keyController_KeyControlJustEngaged(int control)
        {
            KeyMappings.PolarMouseGameControls c = (KeyMappings.PolarMouseGameControls)control;

            //switch (c)
            //{
            //    case KeyMappings.PolarMouseGameControls.Shoot:
            //        var charge = playerProfile.ReleaseCharge(0);
            //        if (charge == null)
            //            this.Write("No Charges Left!");
            //        else
            //        {
            //            releaseItem(charge);
            //            this.Write("Charges Left: {0}", playerProfile.ChargesLeft);
            //        }
            //        break;
            //    case KeyMappings.PolarMouseGameControls.ThrowSensor:
            //        var sensor = playerProfile.ReleaseSensor(0);
            //        if (sensor == null)
            //            this.Write("No Sensors Left!");
            //        else
            //        {
            //            releaseItem(sensor);
            //            this.Write("Sensors Left: {0}", playerProfile.SensorsLeft);
            //        }
            //        break;
            //    case KeyMappings.PolarMouseGameControls.BlastCharges:                    
            //        var objclone = new List<WorldObject>(ActiveMap.WorldObjects);
            //        foreach (var obj in objclone)
            //        {
            //            if (obj is DeviceCarrierActor)
            //            {
            //                var devs = ((DeviceCarrierActor)obj).GetDevices();
            //                foreach (var d in devs)
            //                {
            //                    if (d is Charge) ((Charge)d).Detonate();
            //                }
            //            }
            //        }
            //        break;
            //    default:
            //        break;
            //}
        }

        void keyController_KeyControlEngaged(int control)
        {
            KeyMappings.SimpleGameControls c = (KeyMappings.SimpleGameControls)control;
            var player = (Player)ActiveMap.Player;
            Vector2 mouseDirection = (new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) - player.Position;
            var rad = mouseDirection.Length();
            mouseDirection.Normalize();

            switch (c)
            {
                //case KeyMappings.PolarMouseGameControls.MoveIn:
                //    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * mouseDirection));
                //    break;
                //case KeyMappings.PolarMouseGameControls.MoveOut:
                //    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, -player.Mass * player.SpeedScale * mouseDirection));
                //    break;
                //case KeyMappings.PolarMouseGameControls.MoveClockwise:
                //    var tangentCW = mouseDirection.RotateInPlace(-(float)Math.PI / 2);
                //    var tangVel = Vector2.Dot(player.Velocity, tangentCW);
                //    player.Velocity = tangentCW * tangVel;
                //    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * tangentCW));
                //    break;
                //case KeyMappings.PolarMouseGameControls.MoveCounterClockwise:
                //    var tangentCCW = mouseDirection.RotateInPlace((float)Math.PI / 2);
                //    var tangVelCCW = Vector2.Dot(player.Velocity, tangentCCW);
                //    player.Velocity = tangentCCW * tangVelCCW;
                //    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * tangentCCW));
                //    break;

                case KeyMappings.SimpleGameControls.MoveUp:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * -Vector2.UnitY));
                    break;
                case KeyMappings.SimpleGameControls.MoveDown:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * Vector2.UnitY));
                    break;
                case KeyMappings.SimpleGameControls.MoveLeft:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * -Vector2.UnitX));
                    break;
                case KeyMappings.SimpleGameControls.MoveRight:
                    PhysicsManager.AddInstantaneousForce(new InstantaneousForce((IRigidBody)player, player.Mass * player.SpeedScale * Vector2.UnitX));
                    break;
                default:
                    break;
            }
        }


        void keyController_KeyControlDisengaged(int control)
        {
            KeyMappings.SimpleGameControls c = (KeyMappings.SimpleGameControls)control;
            var player = (Player)ActiveMap.Player;
            switch (c)
            {
                case KeyMappings.SimpleGameControls.MoveUp:

                    break;
                case KeyMappings.SimpleGameControls.MoveDown:

                    break;
                case KeyMappings.SimpleGameControls.MoveLeft:
                    //player.Velocity /= 2f;
                    break;
                case KeyMappings.SimpleGameControls.MoveRight:
                    //player.Velocity /= 2f;
                    break;
                default:
                    break;
            }
        } 
        #endregion


        private void releaseItem(Device device)
        {
            var player = (Player)ActiveMap.Player;
            var mdir = new Vector2((float)Math.Cos(player.Rotation + Math.PI / 2), (float)Math.Sin(player.Rotation + Math.PI / 2));
            var launchPort = player.Position + mdir * (player.Radius + 5); //launch stuff from slightly in front of player
            float launchVel = 300f + player.Velocity.Length();

            var carrier = new DeviceTransporter(launchPort, 15);
            carrier.Velocity = launchVel * mdir;
            carrier.AddDevice(device);

            SpawnObject<Actor>(carrier);
            SpawnObject<Device>(device);
        }

        void badRoid_Destroyed(WorldObject obj)
        {
            PlayerProfile.RoidsToBlast -= 1;

            if (PlayerProfile.RoidsToBlast == 0)
                EndGame();
            else
                this.Write("Roids to Blast: {0}", PlayerProfile.RoidsToBlast);
        }
        void goodRoid_Destroyed(WorldObject obj)
        {
            PlayerProfile.CurrentGoodNumber -= 1;

            if (PlayerProfile.CurrentGoodNumber == (PlayerProfile.RoidsToNotBlast - 3))
                Death();

        }

        void Death()
        {
            var gr = ActiveMap.DynamicObjects;
            for (int i = 0; i < gr.Count; i++)
            {
                if (gr.ElementAt(i) is GoodRoid)
                {
                    var goodroid = (GoodRoid)gr.ElementAt(i);
                    goodroid.Shoot = true;
                }

            }

        }
        void player_GotHurt(dynamic damage)
        {
            PlayerProfile.Health -= damage;
            this.Write("Health: {0}", PlayerProfile.Health);
            if (PlayerProfile.Health < 0)
                EndGame();
        }
        public override void Update(Microsoft.Xna.Framework.GameTime time)
        {
            if (!UIManager.Update(time))
                base.Exit();

            ActiveMap.UpdateDynamicObjects(time);
            PhysicsManager.Update(time.ElapsedGameTime.Milliseconds / 1000f);
            PlayerProfile.UpdateTime(time.ElapsedGameTime);
        }

        /// <summary>
        /// Safely Writes to Screen
        /// </summary>
        public void Write(string output, params object[] args)
        {
            if (!this.isDead)
            {
                Console.WriteLine(output, args);
            }
        }
        public void Write(string output)
        {
            if (!this.isDead)
            {
                Console.WriteLine(output);
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            UIManager.Draw(time, spriteBatch);
            ActiveMap.WorldObjects.ForEach(
                obj => ((WorldCore.IDrawable)obj).Draw(time, spriteBatch));

            spriteBatch.End();

            // Draw Particles
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            PhysicsManager.PSystems.ForEach(ps => ps.Draw());
            spriteBatch.End();
            // End Drawing Particles

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
        }
    }
}

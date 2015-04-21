using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using GameCore;
using WorldCore;
using UICore;
using PhysicsCore;
using Utilities;

using ROIDS.GameStates;
using Microsoft.Xna.Framework.Graphics;

namespace ROIDS.Demos.Swarm
{
    [Demo]
    class SwarmGame : PlayableState
    {
        /*
         * IPlayable Reqs
         */
        public override void Reload()
        {
            Size winSize = (Size)DefaultSettings.Settings["WindowSize"];

            //CollisionEngine.Init(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10, 0.3f);
            PhysicsManager = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10);

            // Fill Map
            int maxDroids = 100;
            int maxTreasures = 20;
            int test = 2;
            int degreeOfLaxity = 5;
            var world = new WorldObject[maxDroids+maxTreasures+1];


            Random rand = new Random();

            // Add Player
            var player = new Player(
                new Vector2((float)rand.NextDouble()*(winSize.Width-50), (float)rand.NextDouble()*(winSize.Height-50)),
                0f);

            world[0] = player;
            PhysicsManager.ActiveBodies.Add(player);

            // Randomly add Treasures
            for (int i = 1; i < maxTreasures; i++)
            {
                var obs = new Treasure(
                    new Vector2((float)rand.NextDouble() * winSize.Width, (float)rand.NextDouble() * winSize.Height),
                    0f);

                // obs.Destroyed += new WorldObjectEventHandler(TreasureTaken);
                world[i] = obs;
                PhysicsManager.ActiveBodies.Add(obs);
            }
            /*for (int i = 1; i < test; i++)
            {
                var obs = new ROIDS.GameObjects.Devices.Sensor(
                    new Vector2((float)rand.NextDouble() * winSize.Width, (float)rand.NextDouble() * winSize.Height),
                    0f);

                // obs.Destroyed += new WorldObjectEventHandler(TreasureTaken);
                world[i] = obs;
                PhysicsManager.ActiveBodies.Add(obs);
            }*/


            // Randomly add droids
            for (int i = 1; i <= maxDroids; i++)
            {
                var obs = new Droid(
                    new Vector2((float)rand.NextDouble() * winSize.Width, (float)rand.NextDouble() * winSize.Height),
                    0f, (rand.Next(degreeOfLaxity) == 2) ? 1:0);

                // if necessary: obs.Destroyed += new WorldObjectEventHandler(obs_Destroyed);
                world[maxTreasures+i] = obs;
                PhysicsManager.ActiveBodies.Add(obs);
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

            // Game Logic Setup
            PlayerManger = new PointsEtc(0, 100);
        }
        public override void EndGame()
        {
            Console.WriteLine("------------Game-Over-----------");
            Console.WriteLine("Points: "+PlayerManger.Points);
            this.Exit();
        }

        /*
         * GameState Reqs
         */
        public override void Load()
        {          

            UIManager = new UIEngine();
            UIManager.AddAndLoad(new UIFrames.BasicFrame()); // Setup for UI input + GUI stuff

            // For Key Mappings
            _controller = new KeyboardController(UIManager.ActiveFrame, DemoKeyMappings.WasdMapping);
            _controller.KeyControlEngaged += new KeyControlEventHandler(_controller_KeyControlEngaged);
            _controller.KeyControlDisengaged += new KeyControlEventHandler(_controller_KeyControlDisengaged);

            Reload(); // For Map changes
        }

        public override void Update(GameTime time)
        {
            if (!UIManager.Update(time)) // Allows exit event to close game
                base.Exit();
            ActiveMap.UpdateDynamicObjects(time);
            PhysicsManager.Update(time.ElapsedGameTime.Milliseconds / 10f);
            if (PlayerManger.Health <= 0)
                this.EndGame();
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {

            UIManager.Draw(time, spriteBatch); // Draws GUI stuff

            // We really need a graphics engine
            ActiveMap.WorldObjects.ForEach(
                obj => ((GameObjects.Actor)obj).Draw(time, spriteBatch));
        }

        /*
         * Swarm Logic
         */
        KeyboardController _controller;
        public PointsEtc PlayerManger;

        void _controller_KeyControlEngaged(int control)
        {
            DemoKeyMappings.SimpleGameControls c = (DemoKeyMappings.SimpleGameControls)control;

            var player = (Player)ActiveMap.Player;
            switch (c)
            {
                case DemoKeyMappings.SimpleGameControls.MoveUp:
                    player.Velocity = -player.Speed * Vector2.UnitY;
                    break;
                case DemoKeyMappings.SimpleGameControls.MoveDown:
                    player.Velocity = player.Speed * Vector2.UnitY;
                    break;
                case DemoKeyMappings.SimpleGameControls.MoveLeft:
                    player.Velocity = -player.Speed * Vector2.UnitX; 
                    break;
                case DemoKeyMappings.SimpleGameControls.MoveRight:
                    player.Velocity = player.Speed * Vector2.UnitX;
                    break;
            }
        }

        void _controller_KeyControlDisengaged(int control)
        {
            if (control <= 4)
                ((Player)ActiveMap.Player).Velocity = Vector2.Zero;
        }

        public void KillObject<T>(T obj)
        {
            ActiveMap.SafelyRemoveObject(obj as WorldObject);
            if (obj is IRigidBody)
                PhysicsManager.SafeDelete(obj as IRigidBody);
        }

    }
}

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
using ROIDS.Demos;
using Microsoft.Xna.Framework.Graphics;

namespace ROIDS.MovingBoxDemo
{
    [Demo]
    class MovingBoxDemoState : GameState, IPlayable
    {

        /*
         * Required for GameState
         */
        public override void Load()
        {
            UIManager = new UIEngine();
            UIManager.AddAndLoad(new UIFrames.BasicFrame()); // Setup for UI input + GUI stuff

            // For Key Mappings
            _controller = new KeyboardController(UIManager.ActiveFrame, DemoKeyMappings.WasdMapping);
            _controller.KeyControlEngaged += new KeyControlEventHandler(_controller_KeyControlEngaged);
            _controller.KeyControlDisengaged += new KeyControlEventHandler(_controller_KeyControlDisengaged);

            // To toggle key maps
            UIManager.ActiveFrame.KeyUp += new KeyEventHandler(ActiveFrame_KeyUp);
            Reload(); // For Map changes
        }

        public override void Update(GameTime time)
        {
            if (!UIManager.Update(time)) // Allows exit event to close game
                base.Exit();

            PhysicsManager.Update(time.ElapsedGameTime.Milliseconds / 10f); 
        }
        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            UIManager.Draw(time, spriteBatch); // Draws GUI stuff

            // Draw Actors
            _world.ForEach(
                actor => ((IMovingBoxDemoObject)actor).Draw(time, spriteBatch));
        }

        /*
         * Required for IPlayable
         */
        public Map ActiveMap { get; set; }
        public PhysicsEngine PhysicsManager { get; private set; }
        public UIEngine UIManager { get; private set; }

        // Load new Map + init
        public void Reload()
        {
            _world = new List<WorldObject>();
            Size winSize = (Size)DefaultSettings.Settings["WindowSize"];

            PhysicsManager = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10);

            _player = new MovingBoxDemoPlayer("Player", Vector2.Zero, 0f);
            _player.Mass = 10;
            _world.Add(_player);
            PhysicsManager.ActiveBodies.Add(_player);

            // Fill Map
            int maxEnts = 50;

            Random rand = new Random();

            for (int i = 0; i < maxEnts; i++)
            {
                var obs = new MovingBoxDemoObstacle(i.ToString(),
                    new Vector2((float)rand.NextDouble() * winSize.Width, (float)rand.NextDouble() * winSize.Height));
                obs.HasInfiniteMass = (rand.Next(2) == 1) ? true : false;
                obs.Destroyed += new WorldObjectEventHandler(obs_Destroyed);
                _world.Add(obs);
                PhysicsManager.ActiveBodies.Add(obs);
            }


            int nx = 15;
            int ny = 15;
            var rad = 4f;
            var buf = 0f;
            var pos = new Vector2(winSize.Width / 2 - nx * (2f * rad + buf) / 2f, winSize.Height / 2 - ny * (2f * rad + buf) / 2f);
            for (int xi = 0; xi < nx; xi++)
            {
                for (int yi = 0; yi < ny; yi++)
                {
                    var cent = pos + new Vector2((2f * rad + buf) * xi, (2f * rad + buf) * yi);
                    var ball = new MovingBoxDemoObstacle("lolwtf", cent );
                    ball.Radius = rad;
                    ball.HasInfiniteMass = false;
                    ball.Destroyed += new WorldObjectEventHandler(obs_Destroyed);
                    _world.Add(ball);
                    PhysicsManager.ActiveBodies.Add(ball);
                }
            }

            var w0 = new ROIDS.Sandbox.VertWallBody(new Vector2(0, winSize.Height / 2), winSize.Height);
            var w1 = new ROIDS.Sandbox.VertWallBody(new Vector2(winSize.Width, winSize.Height / 2), winSize.Height);

            var w2 = new ROIDS.Sandbox.HorizWallBody(new Vector2(winSize.Width / 2, 0), winSize.Width);
            var w3 = new ROIDS.Sandbox.HorizWallBody(new Vector2(winSize.Width / 2, winSize.Height), winSize.Width);

            PhysicsManager.MapBodies.Add(w0);
            PhysicsManager.MapBodies.Add(w1);
            PhysicsManager.MapBodies.Add(w2);
            PhysicsManager.MapBodies.Add(w3);
        }


        void obs_Destroyed(WorldObject obj)
        {
            _world.Remove(obj);
            PhysicsManager.ActiveBodies.Remove((GameObjects.Actor)obj);
        }
        // Cleanup before switching Map
        public void EndGame()
        {
            Reload();
        }


        /*
         * Definitions for Demo
         */

        // Customizable Key Mappings Manager
        KeyboardController _controller;

        // WorldEngine stand in
        List<WorldObject> _world;

        // Player stand in 
        MovingBoxDemoPlayer _player;



        // To toggle key maps
        void ActiveFrame_KeyUp(GUIElement sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Tab))
            {
                if (_controller.KeyMapping.ContainsKey(Keys.Up))
                    _controller.KeyMapping =  DemoKeyMappings.WasdMapping;
                else
                    _controller.KeyMapping = DemoKeyMappings.ArrowMapping;
            }
        }

        // User Controls -- see Load() for setup
        void _controller_KeyControlEngaged(int intControl)
        {
            DemoKeyMappings.SimpleGameControls control = (DemoKeyMappings.SimpleGameControls)intControl;

            switch (control)
            {
                case DemoKeyMappings.SimpleGameControls.MoveUp:
                    _player.Velocity = -_player.Speed*Vector2.UnitY;
                    break;
                case DemoKeyMappings.SimpleGameControls.MoveDown:
                    _player.Velocity = _player.Speed*Vector2.UnitY;
                    break;
                case DemoKeyMappings.SimpleGameControls.MoveLeft:
                    _player.Velocity = -_player.Speed*Vector2.UnitX;
                    break;
                case DemoKeyMappings.SimpleGameControls.MoveRight:
                    _player.Velocity = _player.Speed*Vector2.UnitX;
                    break;
            }

        }
        // User Controls -- see Load() for setup
        void _controller_KeyControlDisengaged(int control)
        {
            //if (control <= 4)
            //    _player.Velocity = Vector2.Zero;
        }

        public void KillObject<T>(T obj)
        {
            //
        }
    }
}

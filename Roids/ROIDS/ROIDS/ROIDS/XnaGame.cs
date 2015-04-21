using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using GameCore;
[assembly: CLSCompliant(true)]
namespace ROIDS
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class XnaGame : Microsoft.Xna.Framework.Game
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        GraphicsDeviceManager graphics;
        GameEngine _gameEngine;


        public SpriteBatch spriteBatch;


        public XnaGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _gameEngine = GameEngine.Singleton;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Utilities.DefaultSettings.LoadDefaultSettings();

            // Set Window Size
            var winSize = (Utilities.Size)Utilities.DefaultSettings.Settings["WindowSize"];
            graphics.PreferredBackBufferWidth = (int)winSize.Width;
            graphics.PreferredBackBufferHeight = (int)winSize.Height;
            graphics.ApplyChanges();

            // Set Startup State
                // _gameEngine.AddState(new MovingBoxDemo.MovingBoxDemoState());
                //_gameEngine.AddState(new Sandbox.Sandbox1());
                // _gameEngine.AddState(new Sandbox.Sandbox2());
            //_gameEngine.AddState(new ROIDS.Demos.Swarm.SwarmGame());
            _gameEngine.AddState(new ROIDS.Demos.DemoLoader());
            //_gameEngine.AddState(new ROIDS.Sandbox.EXPLOSIONSFUCKYEAH());
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Utilities.ContentRepository.LoadContent(Content);

            // Load Global Data
            Utilities.Globals.LoadGlobals(this, spriteBatch);

            // Load GraphicsUtil
            Utilities.GraphicsUtils.Load(spriteBatch, 
                (Texture2D)Utilities.ContentRepository.Repository["Pixel"],
                (Texture2D)Utilities.ContentRepository.Repository["Ball"]);
            
            // Load Active State
            _gameEngine.ActiveState.Load();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!_gameEngine.Update(gameTime))
                this.Exit();
            
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            if (_gameEngine.ActiveState != null)
                _gameEngine.ActiveState.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

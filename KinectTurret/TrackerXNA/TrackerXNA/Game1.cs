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
using KTLib;

using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace TrackerXNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectInterface kinect;
        KinectView3D kview;

        TrackerManager trackerMan; 

        SpriteFont font1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsFixedTimeStep = false;
            IsMouseVisible = true;

            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.ApplyChanges();

            kinect = new KinectInterface(GraphicsDevice);
            kinect.Start();

            

            trackerMan = new TrackerManager(kinect);

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
            font1 = Content.Load<SpriteFont>("font1");

            Model sphere = Content.Load<Model>("SphereHighPoly");
            kview = new KinectView3D(GraphicsDevice, kinect, trackerMan, sphere, new Rectangle(600, 0, 1280 - 600, 720));
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            kview.Update();
            trackerMan.Update();

            
            base.Update(gameTime);
        }

        void drawW(Texture2D tex, int x, int y, int w)
        {
            spriteBatch.Draw(tex, new Rectangle(x, y, w, w* tex.Height / tex.Width), Color.White);
        }
        void drawH(Texture2D tex, int x, int y, int h)
        {
            spriteBatch.Draw(tex, new Rectangle(x, y, h * tex.Width / tex.Height, h), Color.White);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (!kinect.Ready)
                return;

            kview.Draw();

            spriteBatch.Begin();
            //spriteBatch.Draw(kinect.DepthFrameTex,  Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 0.5f,  SpriteEffects.None, 0f);
            //spriteBatch.Draw(kinect.ColorFrameTex, Vector2.UnitX * KinectInterface.w/2f, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            
            int w = 300;
            int h = w*kinect.DepthFrameTex.Height/kinect.DepthFrameTex.Width;
            drawW(kinect.DepthFrameTex, 0, 0, 300);
            //drawW(kinect.ColorFrameTex, 300, 0, 300);
            //drawW(trackerMan.detector.debugOut.ToTex(GraphicsDevice), 300, 0, 300);
            //drawW(kinect.FullDepth.PyrDown().ToTex(GraphicsDevice), 300, 0, 300);
            drawW(trackerMan.DisplayOut.ToTex(GraphicsDevice), 0, h, 600);
            spriteBatch.DrawString(font1, kinect.FPS.ToString(), new Vector2(600, 0), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

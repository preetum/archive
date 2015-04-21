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

using PhysicsCore;
using Utilities;

namespace Sandbox1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        PhysicsEngine PE;

        
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
            this.IsMouseVisible = true;

            var w = (float)graphics.PreferredBackBufferWidth;
            var h = (float)graphics.PreferredBackBufferHeight;

            CollisionEngine.Init(new Region(0, (float)w, 0, (float)h), 10);
            PE = new PhysicsEngine();
            
            
            var w0 = new VertWallBody(new Vector2(0, h / 2), h);
            var w1 = new VertWallBody(new Vector2(w, h / 2), h);

            var w2 = new HorizWallBody(new Vector2(w / 2, 0), w);
            var w3 = new HorizWallBody(new Vector2(w / 2, h), w);

            PE.MapBodies.Add(w0);
            PE.MapBodies.Add(w1);
            PE.MapBodies.Add(w2);
            PE.MapBodies.Add(w3);


            var c0 = new CircleBody(30, 300, new Vector2(600, h/2));
            c0.Velocity = -1000 * Vector2.UnitX;
            PE.ActiveBodies.Add(c0);


            Random rand = new Random();

            int nx = 40;
            int ny = 30;
            var rad = 4f;
            var buf = 0f;
            var pos = new Vector2(w / 2 - nx * (2f * rad + buf) / 2f, h / 2 - ny * (2f * rad + buf) / 2f);
            for (int xi = 0; xi < nx; xi++)
            {
                for (int yi = 0; yi < ny; yi++)
                {
                    var cent = pos + new Vector2((2f * rad + buf) * xi, (2f * rad + buf) * yi);
                    var ball = new CircleBody(rad, rad, cent);
                    PE.ActiveBodies.Add(ball);
                }
            }



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

            Texture2D px = Content.Load<Texture2D>("pixel");
            Texture2D ball = Content.Load<Texture2D>("ball");
            GraphicsUtils.Load(spriteBatch, px, ball);

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

            PE.Update((float)(gameTime.ElapsedGameTime.Milliseconds) / 1000f);

            base.Update(gameTime);
        }


        void drawTree<T>(QuadTree<T> qt) where T : IRegion
        {
            Vector2 p1 = new Vector2(qt.Span.XMax, qt.Span.YMin);
            Vector2 p2 = new Vector2(qt.Span.XMin, qt.Span.YMin);
            Vector2 p3 = new Vector2(qt.Span.XMin, qt.Span.YMax);
            Vector2 p4 = new Vector2(qt.Span.XMax, qt.Span.YMax);

            float t = 1f;
            GraphicsUtils.DrawLineTop(p1, p2, Color.GreenYellow, t);
            GraphicsUtils.DrawLineTop(p2, p3, Color.GreenYellow, t);
            GraphicsUtils.DrawLineTop(p3, p4, Color.GreenYellow, t);
            GraphicsUtils.DrawLineTop(p4, p1, Color.GreenYellow, t);

            if (qt.SubTrees != null)
            {
                foreach (var qtsub in qt.SubTrees)
                    drawTree<T>(qtsub);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (gameTime.IsRunningSlowly)
                GraphicsDevice.Clear(Color.MistyRose);
            else
                GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsUtils.Begin();

            foreach (RigidBody b in PE.ActiveBodies)
            {
                GraphicsUtils.DrawBall(b.Position, b.BoundingBox.Width / 2, Color.Black, 255);
            }

            foreach (RigidBody b in PE.MapBodies)
            {
                if (b is VertWallBody || b is HorizWallBody)
                {
                    var bb = b.BoundingBox;
                    GraphicsUtils.DrawRectangle(bb.XMin, bb.XMax, bb.YMin, bb.YMax, Color.Black);
                }
            }

            drawTree<RigidBody>(PE.ActiveQT);
            GraphicsUtils.End();


            base.Draw(gameTime);
        }
    }
}

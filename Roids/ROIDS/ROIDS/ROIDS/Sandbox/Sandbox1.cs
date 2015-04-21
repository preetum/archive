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
    class Sandbox1 : GameState
    {
        UIEngine _uiEngine;
        PhysicsEngine PE;

        public override void Load()
        {
            // Extract Global Data
            Game currentGame = Globals.Data["Game"];
            SpriteBatch spriteBatch = Globals.Data["SpriteBatch"];
            Size winSize = DefaultSettings.Settings["WindowSize"];

            // Initialize
            currentGame.IsMouseVisible = true;

            PE = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height),10);

            var w0 = new VertWallBody(new Vector2(0, winSize.Height / 2), winSize.Height);
            var w1 = new VertWallBody(new Vector2(winSize.Width, winSize.Height / 2), winSize.Height);

            var w2 = new HorizWallBody(new Vector2(winSize.Width / 2, 0), winSize.Width);
            var w3 = new HorizWallBody(new Vector2(winSize.Width / 2, winSize.Height), winSize.Width);

            PE.MapBodies.Add(w0);
            PE.MapBodies.Add(w1);
            PE.MapBodies.Add(w2);
            PE.MapBodies.Add(w3);


            var c0 = new CircleBody(30, 300, new Vector2(600, winSize.Height / 2));
            c0.Velocity = -1000 * Vector2.UnitX;
            PE.ActiveBodies.Add(c0);


            Random rand = new Random();

            int nx = 40;
            int ny = 40;
            var rad = 4f;
            var buf = 0f;
            var pos = new Vector2(winSize.Width / 2 - nx * (2f * rad + buf) / 2f, winSize.Height / 2 - ny * (2f * rad + buf) / 2f);
            for (int xi = 0; xi < nx; xi++)
            {
                for (int yi = 0; yi < ny; yi++)
                {
                    var cent = pos + new Vector2((2f * rad + buf) * xi, (2f * rad + buf) * yi);
                    var ball = new CircleBody(rad, rad, cent);
                    PE.ActiveBodies.Add(ball);
                }
            }

            // Load Content
            GraphicsUtils.Load(spriteBatch, 
                (Texture2D)ContentRepository.Repository["Pixel"], 
                (Texture2D)ContentRepository.Repository["Ball"]);

            // Setup UI
            _uiEngine = new UIEngine();
            _uiEngine.AddAndLoad(new UIFrames.BasicFrame());
        }
        public override void Update(GameTime time)
        {
            if (!_uiEngine.Update(time))
                this.Exit();

            PE.Update((float)(time.ElapsedGameTime.Milliseconds) / 1000f);
        }

        void drawTree<T>(QuadTree<T> qt) where T : IRegion
        {
            if (qt != null)
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
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;

            if (time.IsRunningSlowly)
                graphicsDevice.Clear(Color.MistyRose);
            else
                graphicsDevice.Clear(Color.CornflowerBlue);


            foreach (IRigidBody b in PE.ActiveBodies)
            {
                GraphicsUtils.DrawBall(b.Position, b.BoundingBox.Width / 2, Color.Black, 255);
            }

            foreach (IRigidBody b in PE.MapBodies)
            {
                if (b is IVertWallBody || b is IHorizWallBody)
                {
                    var bb = b.BoundingBox;
                    GraphicsUtils.DrawRectangle(bb.XMin, bb.XMax, bb.YMin, bb.YMax, Color.Black);
                }
            }

            drawTree<IRigidBody>(PE.QTbodies);
           // GraphicsUtils.End();

        }
    }
}

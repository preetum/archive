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
    class Sandbox2 : GameState
    {
        UIEngine _uiEngine;
        PhysicsEngine PE;

        bool mactive = false;
        bool factive = false;
        bool gactive = false;
        bool dactive = false;
        float rad = 50f;

        Random rand = new Random();

        Size winSize = DefaultSettings.Settings["WindowSize"];

        public override void Load()
        {
            // Extract Global Data
            Game currentGame = Globals.Data["Game"];
            SpriteBatch spriteBatch = Globals.Data["SpriteBatch"];


            // Initialize
            currentGame.IsMouseVisible = true;


            PE = new PhysicsEngine(new Region(0, (float)winSize.Width, 0, (float)winSize.Height), 10);
            //PE.AddUniversalForce(DefaultForces.Gravity);
            //PE.AddUniversalForce(DefaultForces.Drag);


            var w0 = new VertWallBody(new Vector2(0, winSize.Height / 2), winSize.Height);
            var w1 = new VertWallBody(new Vector2(winSize.Width, winSize.Height / 2), winSize.Height);

            var w2 = new HorizWallBody(new Vector2(winSize.Width / 2, 0), winSize.Width);
            var w3 = new HorizWallBody(new Vector2(winSize.Width / 2, winSize.Height), winSize.Width);

            PE.MapBodies.Add(w0);
            PE.MapBodies.Add(w1);
            PE.MapBodies.Add(w2);
            PE.MapBodies.Add(w3);


            //var c0 = new CircleBody(30, 300, new Vector2(600, winSize.Height / 2));
            //c0.Velocity = -1000 * Vector2.UnitX;
            //PE.ActiveBodies.Add(c0);

            //PE.AddBoundForceField(new BoundForceField(c0, 50, DefaultForces.StrongAttractor));


            int nx = 40;
            int ny = 20;
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

            printInstr();
            printStat();

            // Load Content
            GraphicsUtils.Load(spriteBatch,
                ContentRepository.Repository["Pixel"],
                ContentRepository.Repository["Ball"]);

            // Setup UI
            _uiEngine = new UIEngine();
            var frame = new UIFrames.BasicFrame();
            frame.MouseDown += new MouseEventHandler(frame_MouseDown);
            frame.MouseClick += new MouseEventHandler(frame_MouseClick);
            frame.MouseOver += new MouseEventHandler(frame_MouseOver);
            frame.KeyPressDown += new KeyEventHandler(frame_KeyPressDown);
            _uiEngine.AddAndLoad(frame);
        }
        void printStat()
        {
            Console.WriteLine("Enabled:");
            if (gactive) Console.WriteLine("-gravity");
            if (dactive) Console.WriteLine("-drag");
            if (factive) Console.WriteLine("-forcefields");
            Console.WriteLine("---");
        }
        void printInstr()
        {
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("[G] - toggle gravity");
            Console.WriteLine("[D] - toggle drag");
            Console.WriteLine("[F] - toggle force fields");
            Console.WriteLine("[L] - launch cannonball");
            Console.WriteLine("[MOUSE DRAG] - use the FORCE");
            Console.WriteLine("---");
        }
        void frame_KeyPressDown(Element sender, KeyEventArgs e)
        {
            switch (e.InterestingKeys[0])
            {
                case Keys.F:
                    factive = !factive;

                    if (factive)
                    {
                        foreach (var ball in PE.ActiveBodies)
                        {
                            if (rand.Next(5) == 0)
                            {
                                PE.AddBoundForceField(new BoundForceField(ball, 20, DefaultForces.StrongAttractor));
                                //PE.AddBoundForceField(new BoundForceField(ball, 20, DefaultForces.WeakAttractor));
                            }
                            if (rand.Next(50) == 0)
                            {
                                //PE.AddBoundForceField(new BoundForceField(ball, 100, DefaultForces.StrongAttractor));
                                //PE.AddBoundForceField(new BoundForceField(ball, 20, DefaultForces.WeakAttractor));
                            }
                        }
                    }
                    else
                    {
                        PE.bffields.Clear();
                    }
                    break;

                case Keys.L:
                    var c0 = new CircleBody(30, 300, new Vector2((new System.Random()).Next(0, (int)winSize.Width - 60), winSize.Height));
                    c0.Velocity = -1000 * Vector2.UnitY;
                    PE.ActiveBodies.Add(c0);
                    break;


                case Keys.D:
                    dactive = !dactive;
                    if (dactive)
                    {
                        PE.AddUniversalForce(DefaultForces.LinearDrag);
                    }
                    else
                    {
                        PE.RemoveUniversalForce(DefaultForces.LinearDrag);
                    }
                    break;

                case Keys.G:
                    gactive = !gactive;
                    if (gactive)
                    {
                        PE.AddUniversalForce(DefaultForces.Gravity);
                    }
                    else
                    {
                        PE.RemoveUniversalForce(DefaultForces.Gravity);
                    }
                    break;

                default:
                    break;
            }

            printStat();
            
        }

        void frame_MouseOver(Element sender, MouseEventArgs e)
        {
            var pcur = new Vector2(e.CurrentMouseState.X, e.CurrentMouseState.Y);
            var pold = new Vector2(e.PreviousMouseState.X, e.PreviousMouseState.Y);

            if (mactive)
                PE.AddInstantaneousForceField(new InstantaneousForceField(pcur, rad, DefaultForces.GenerateDirectionalField(400 * (pcur - pold), rad)));
        }

        void frame_MouseClick(Element sender, MouseEventArgs e)
        {
            mactive = false;
        }

        void frame_MouseDown(Element sender, MouseEventArgs e)
        {
            mactive = true;
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

            foreach (var bff in PE.bffields)
            {
                var bb = bff.AreaInfluenced;
                GraphicsUtils.DrawRectangle(bb.XMin, bb.XMax, bb.YMin, bb.YMax, Color.FromNonPremultiplied(200, 100, 100, 50));
            }


            if (mactive)
            {
                GraphicsUtils.DrawRectangle(new Vector2(Mouse.GetState().X, Mouse.GetState().Y), rad * 2, rad * 2, 0f, Color.FromNonPremultiplied(200, 100, 100, 150));
            }

            drawTree<IRigidBody>(PE.QTbodies);

            // GraphicsUtils.End();

        }
    }
}

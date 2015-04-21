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
    class Sandbox0 : GameState

    {
        QuadTree<Object> QT;
        List<Object> Objs;

        Random rand;
        Region query;
        bool qactive = false;

        UIEngine _uiEngine;
        public class Object : IRegion
        {
            public Region Span { get; set; }
            public bool Flagged = false;
            public bool Intersecting = false;

            public Object(Region span)
            {
                Span = span;
            }
        }

        Region getRandRegion(Size winSize)
        {
            var r = new Region();
            r.XMin = winSize.Width * (float)rand.NextDouble();
            r.YMin = winSize.Height * (float)rand.NextDouble();
            r.XMax = r.XMin + 1 + 10f * (float)rand.NextDouble();
            r.YMax = r.YMin + 1 + 10f * (float)rand.NextDouble();

            return r;
        }

        void printTree<T>(QuadTree<T> qt, int level) where T : IRegion
        {
            for (int i = 0; i < level; i++) Console.Write("\t");
            Console.Write("Nodes: " + qt.Nodes.Count + "     Subtrees: ");

            if (qt.SubTrees != null)
            {
                Console.Write("\r\n");
                printTree<T>(qt.SubTrees[0], level + 1);
                printTree<T>(qt.SubTrees[1], level + 1);
                printTree<T>(qt.SubTrees[2], level + 1);
                printTree<T>(qt.SubTrees[3], level + 1);
            }
            else
            {
                Console.Write("None\r\n");
            }
        }

        public override void Load()
        {
            // Extract Global Data
            Game currentGame = (Game)Globals.Data["Game"];
            SpriteBatch spriteBatch = (SpriteBatch)Globals.Data["SpriteBatch"];
            Size winSize = (Size)DefaultSettings.Settings["WindowSize"];

            // Initialize
            rand = new Random();

            currentGame.IsMouseVisible = true;


            // Load Content
            GraphicsUtils.Load(spriteBatch,
                (Texture2D)ContentRepository.Repository["Pixel"],
                (Texture2D)ContentRepository.Repository["Ball"]);

            Objs = new List<Object>();
            QT = new QuadTree<Object>(new Region(0, winSize.Width, 0, winSize.Height), 1, 10);

            printTree<Object>(QT, 0);


            /*for (int i = 0; i < 10000; i++)
            {
                var ob = new Object(getRandRegion(winSize));
                QT.AddNode(ob);
                Objs.Add(ob);
            }

            var start = DateTime.Now.Ticks; //time the query
            for (int i = 0; i < 1000; i++)
            {
                QT.Query(new Region(0, 50, 0, 50));
            }
            var end = DateTime.Now.Ticks;

            Console.WriteLine("QUERY TOOK " + (end - start).ToString() + " TICKS");*/


            // Setup UI
            _uiEngine = new UIEngine();

            var frame = new UIFrames.BasicFrame();
            frame.MouseClick += new MouseEventHandler(frame_MouseClick);
            frame.MouseDown += new MouseEventHandler(frame_MouseDown);
            frame.MouseOver += new MouseEventHandler(frame_MouseOver);
            frame.KeyUp += new KeyEventHandler(frame_KeyUp);
            _uiEngine.AddAndLoad(frame);

        }

        void frame_KeyUp(Element sender, KeyEventArgs e)
        {

            Size winSize = (Size)DefaultSettings.Settings["WindowSize"];
            switch (e.InterestingKeys[0])
            {
                case Keys.Z:
                    for (int i = 0; i < 1000; i++)
                    {
                        var ob = new Object(getRandRegion(winSize));
                        QT.AddNode(ob);
                        Objs.Add(ob);
                    }

                    break;

                case Keys.R:
                    for (int i = 0; i < 50; i++)
                    {
                        var ob = new Object(getRandRegion(winSize));
                        QT.AddNode(ob);
                        Objs.Add(ob);
                    }

                    break;
                case Keys.Space:
                    Objs.Clear();
                    QT = new QuadTree<Object>(new Region(0, winSize.Width, 0, winSize.Height), 1, 100);
                    break;

                default:
                    break;
            }

            Console.WriteLine(Objs.Count + " objects");
        }


        void frame_MouseOver(Element sender, MouseEventArgs e)
        {
            query.XMax = Mouse.GetState().X;
            query.YMax = Mouse.GetState().Y;
        }

        void frame_MouseDown(Element sender, MouseEventArgs e)
        {
            if (e.isPressed(MouseButtons.Right))
            {
                qactive = true;
                Objs.ForEach(n => n.Flagged = false);

                query = new Region();
                query.XMin = Mouse.GetState().X;
                query.YMin = Mouse.GetState().Y;
            }

        }

        void frame_MouseClick(Element sender, MouseEventArgs e)
        {
            if (e.isClicked(MouseButtons.Left))
            {
                var x = Mouse.GetState().X;
                var y = Mouse.GetState().Y;
                var ob = new Object(new Region(x - 10, x + 10, y - 10, y + 10));
                Objs.Add(ob);
                QT.AddNode(ob);
                //printTree<Object>(QT, 0); 
                Console.WriteLine(Objs.Count + " objects");
            }
            else if (e.isClicked(MouseButtons.Right))
            {
                qactive = false;
               
                query.FixBoundOrder();

                var start = DateTime.Now.Ticks; //time the query
                var hits = QT.Query(query);
                var end = DateTime.Now.Ticks;

                Console.WriteLine("QUERY TOOK " + (end - start).ToString() + " TICKS");

                hits.ForEach(n => n.Flagged = true);
                Console.WriteLine("selected: " + hits.Count + " objects");
            }
        }

        public override void Update(GameTime time)
        {
            if (!_uiEngine.Update(time))
                this.Exit();
        }

        void drawTree<T>(QuadTree<T> qt) where T : IRegion
        {
            Vector2 p1 = new Vector2(qt.Span.XMax, qt.Span.YMin);
            Vector2 p2 = new Vector2(qt.Span.XMin, qt.Span.YMin);
            Vector2 p3 = new Vector2(qt.Span.XMin, qt.Span.YMax);
            Vector2 p4 = new Vector2(qt.Span.XMax, qt.Span.YMax);

            float t = 3f;
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

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsDevice graphicsDevice = spriteBatch.GraphicsDevice;
            graphicsDevice.Clear(Color.CornflowerBlue);

            foreach (var o in Objs)
            {
                Color c = o.Flagged ? Color.Red : Color.Black;
                GraphicsUtils.DrawRectangle(o.Span.XMin, o.Span.XMax, o.Span.YMin, o.Span.YMax, c);
            }

            drawTree(QT);


            if (qactive)
            {
                var qfix = query;
                qfix.FixBoundOrder(); //don't mess with query itself, since the interaction code will mess up.
                GraphicsUtils.DrawRectangleTop(qfix.XMin, qfix.XMax, qfix.YMin, qfix.YMax, Color.FromNonPremultiplied(255, 0, 0, 127));
            }
        }
    }
}

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

namespace Sandbox0
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        QuadTree<Object> QT;
        List<Object> Objs;

        Random rand;

        ButtonState prevstateR = ButtonState.Released;
        ButtonState prevstateL = ButtonState.Released;
        Keys[] kold;

        Region query;
        bool qactive = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

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

        Region getRandRegion()
        {
            var r = new Region();
            r.XMin = graphics.PreferredBackBufferWidth*(float)rand.NextDouble();
            r.YMin = graphics.PreferredBackBufferHeight*(float)rand.NextDouble();
            r.XMax = r.XMin + 1 + 10f *(float)rand.NextDouble();
            r.YMax = r.YMin + 1 + 10f *(float)rand.NextDouble();

            return r;
        }

        void printTree<T>(QuadTree<T> qt, int level) where T:IRegion
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
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            rand = new Random();

            IsMouseVisible = true;

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

            Objs = new List<Object>();
            QT = new QuadTree<Object>(new Region(0, graphics.PreferredBackBufferWidth, 0, graphics.PreferredBackBufferHeight), 10, 10);

            printTree<Object>(QT, 0);


            for (int i = 0; i < 10000; i++)
            {
                var ob = new Object(getRandRegion());
                QT.AddNode(ob);
                Objs.Add(ob);
            }

            var start = DateTime.Now.Ticks; //time the query
            for (int i = 0; i < 1000; i++)
            {
                QT.Query(new Region(0, 50, 0, 50));
            }
            var end = DateTime.Now.Ticks;

            Console.WriteLine("QUERY TOOK " + (end - start).ToString() + " TICKS");

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

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && prevstateL == ButtonState.Released)
            {
                var x = Mouse.GetState().X;
                var y = Mouse.GetState().Y;
                var ob = new Object(new Region(x-10, x + 10, y-10, y + 10));
                Objs.Add(ob);
                QT.AddNode(ob);
                //printTree<Object>(QT, 0); 
                Console.WriteLine(Objs.Count + " objects");
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed && prevstateR == ButtonState.Released) //down
            {
                qactive = true;
                Objs.ForEach(n => n.Flagged = false);

                query = new Region();
                query.XMin = Mouse.GetState().X;
                query.YMin = Mouse.GetState().Y;
            }
            if (Mouse.GetState().RightButton == ButtonState.Released && prevstateR == ButtonState.Pressed) //up
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
            prevstateL = Mouse.GetState().LeftButton;
            prevstateR = Mouse.GetState().RightButton;


            query.XMax = Mouse.GetState().X;
            query.YMax = Mouse.GetState().Y;


            var kcurr = Keyboard.GetState().GetPressedKeys();
            foreach (var k in kcurr)
            {
                if (!kold.Contains(k))
                {
                    //
                    //Key 'k' newly pressed down. Process here.
                    //
                    switch (k)
                    {

                        case Keys.Z:
                            for (int i = 0; i < 1000; i++)
                            {
                                var ob = new Object(getRandRegion());
                                QT.AddNode(ob);
                                Objs.Add(ob);
                            }
                            
                            break;

                        case Keys.R:
                            for (int i = 0; i < 50; i++)
                            {
                                var ob = new Object(getRandRegion());
                                QT.AddNode(ob);
                                Objs.Add(ob);
                            }
                            
                            break;
                        case Keys.Space:
                            Objs.Clear();
                            QT = new QuadTree<Object>(new Region(0, graphics.PreferredBackBufferWidth, 0, graphics.PreferredBackBufferHeight), 1, 100);
                            break;

                        default:
                            break;
                    }

                    //printTree<Object>(QT, 0);
                    Console.WriteLine(Objs.Count + " objects");
                }
            }
            kold = kcurr;


            base.Update(gameTime);
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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsUtils.Begin();


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

            GraphicsUtils.End();
            base.Draw(gameTime);
        }
    }
}

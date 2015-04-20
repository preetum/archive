using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;

using Omron.Framework;
using Omron.Framework.Networking;
using Omron.Actors;

using Omron.Helpers;

using Lidgren.Network;
using System.Diagnostics;

namespace Omron.GameStates
{
   
    public class TestLOState : GameState
    {
        public TestLOState() { }

        World world;
        Camera ActiveCam;
        Faction playerF;
        UIManager UIMan;

        SpriteBatch spriteBatch;


        Tile activeTile;

        public override void Init()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            UIMan = new UIManager();
            UIMan.KeyDown += new KeyPressEventHandler(UIMan_KeyDown);
            UIMan.MouseLeftDown += new MouseClickEventHandler(UIMan_MouseLeftDown);
            UIMan.MouseLeftUp += new MouseClickEventHandler(UIMan_MouseLeftUp);

            world = new World(10, 10, 1.0f);
            playerF = new Faction(world);

            Point centUV = new Point((int)(world.TileGrid.U_length / 2), (int)(world.TileGrid.V_length / 2));
            Vector2 cent = 0.5f * world.TileGrid.UVToScreen(new Point(world.TileGrid.U_length - 1, world.TileGrid.V_length - 1));

            ActiveCam = new Camera(GraphicsDevice.Viewport);
            ActiveCam.Target = cent; //target camera on center of tile grid
            ActiveCam.Zoom = 50f;

            MapGenerator.renewSeed();
            MapGenerator.GeneratePerlinMap(world);


         

        }

        bool losActive;
        Vector2 p1, p2;
        bool IsLOSVisible;

        void UIMan_MouseLeftDown(Vector2 mPos)
        {
            losActive = true;
            p1 = Vector2.Transform(mPos, ActiveCam.GetUntransform());
        }
        void UIMan_MouseLeftUp(Vector2 mPos)
        {
            losActive = false;
        }

      

        void UIMan_KeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Escape:
                    GameEngine.PopState();
                    break;
                case Keys.OemPlus:
                    ActiveCam.Zoom *= 2f;
                    break;
                case Keys.OemMinus:
                    ActiveCam.Zoom /= 2f;
                    break;
            }
        }
     

        public override void Update(GameTime gameTime)
        {
            UIMan.Update();

            Vector2 mPos = UIMan.GetMousePos();
            Vector2 gamePt = Vector2.Transform(mPos, ActiveCam.GetUntransform());
            Point uvPt = world.TileGrid.ScreenToUV(gamePt);

            //active tile
            if (world.TileGrid.IsValidUV(uvPt))
                activeTile = world.TileGrid[uvPt];
            else
                activeTile = null;


            //los
            if (losActive)
            {
                p2 = Vector2.Transform(mPos, ActiveCam.GetUntransform());

                IsLOSVisible = world.TestLOS(p1, p2, a => true);
            }
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, ActiveCam.GetTransform());


            for (int u = 0; u < world.TileGrid.U_length; u++)
            {
                for (int v = 0; v < world.TileGrid.V_length; v++)
                {
                    var tile = world.TileGrid[u, v];

                    drawHexStroke(tile.Position, world.TileGrid.HexSideLen, Color.Gray);
                }
            }
            if (activeTile != null)
            {
                drawHex(activeTile.Position, world.TileGrid.HexSideLen, Color.Crimson * 0.5f);
            }


            foreach (var actor in world.GetActors())
            {
                actor.Draw(spriteBatch);
            }


            if (losActive)
            {
                Color c = IsLOSVisible ? Color.LimeGreen : Color.Red;
                GraphicsHelper.DrawLineInv(spriteBatch, p1, p2, c, 0.02f);
            }
            
            spriteBatch.End();


            spriteBatch.Begin();
            UIMan.Draw(spriteBatch);
            spriteBatch.End();
        }
        void drawHex(Vector2 pos, float sideLen, Color c)
        {
            var hexTex = ResourceManager.Resources["hex"];
            spriteBatch.Draw(hexTex, pos, null, c, 0.0f, new Vector2(hexTex.Width / 2, hexTex.Height / 2), new Vector2(2 * sideLen / hexTex.Width, -2 * sideLen / hexTex.Height), SpriteEffects.None, 0.8f);  //height is scaled -1 since y-axis gets flipped
        }
        void drawHexStroke(Vector2 pos, float sideLen, Color c)
        {
            var hexTex = ResourceManager.Resources["hexStroke"];
            spriteBatch.Draw(hexTex, pos, null, c, 0.0f, new Vector2(hexTex.Width / 2, hexTex.Height / 2), new Vector2(2 * sideLen / hexTex.Width, -2 * sideLen / hexTex.Height), SpriteEffects.None, 0.9f);  //height is scaled -1 since y-axis gets flipped
        }
    }
}

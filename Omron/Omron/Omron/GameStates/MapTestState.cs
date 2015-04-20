using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Omron.Framework;
using Omron.Actors;
using System.IO;


namespace Omron.GameStates
{
    public class MapTestState : GameState
    {
        public MapTestState() { }

        SpriteBatch spriteBatch;
        Texture2D noiseTex;
        Texture2D perlinTex;

        UIManager UIMan;

        float p = 0.5f;
        int n = 2;

        int size = 50;

        float thresh = 0.0f;

        public override void Init()
        {
            spriteBatch = new SpriteBatch(this.GraphicsDevice);

            UIMan = new UIManager();
            UIMan.KeyDown += new KeyPressEventHandler(UIMan_KeyDown);

            //
            //
            //

            Console.WriteLine("INSTRUCTIONS: Up/Down arrows to modify persistance, Left/Right to modify # of octaves, +/- for threshold");

            reload();
        }
        void reload()
        {
            //MapGenerator.renewSeed();

            perlinTex = new Texture2D(GraphicsDevice, size, size);
            noiseTex = new Texture2D(GraphicsDevice, size, size);
            
            Color[] nfield = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float k = MapGenerator.smoothNoise(x, y);
                    nfield[y * size + x] = Color.Lerp(Color.Black, Color.White, 0.5f * (k + 1));
                }
            }
            noiseTex.SetData<Color>(nfield);



            Color[] cfield = new Color[size * size];
            float[,] pField = MapGenerator.genPerlinField(size, size, p, n);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float k = pField[x, y];
                    //Color c = Color.Lerp(Color.Black, Color.White, 0.5f * (k + 1));
                    Color c = k > thresh ? Color.White : Color.Black;
                    cfield[y * size + x] = c;
                }
            }
            perlinTex.SetData<Color>(cfield);

            printStats();
        }

        void UIMan_KeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.Escape:
                    GameEngine.PopState();
                    break;
                case Keys.R:
                    reload();
                    break;
                case Keys.Up:
                    p = (p + 1) / 2f;
                    reload();
                    break;
                case Keys.Down:
                    p = (p + 0) / 2f;
                    reload();
                    break;
                case Keys.Right:
                    n++;
                    reload();
                    break;
                case Keys.Left:
                    if (n > 1)
                    {
                        n--;
                        reload();
                    }
                    break;
                case Keys.OemPlus:
                    thresh += 0.1f;
                    reload();
                    break;
                case Keys.OemMinus:
                    thresh -= 0.1f;
                    reload();
                    break;

            }
        }
        void printStats()
        {
            Console.WriteLine("persistance: " + p + "   |   octaves: " + n + "   |   threshold: " + thresh);
        }
        public override void Update(GameTime gameTime)
        {
            UIMan.Update();
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            float scale = 5.0f;
            spriteBatch.Draw(noiseTex, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(perlinTex, Vector2.UnitY * (noiseTex.Width * scale + 50), null, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
    }
}

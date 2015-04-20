using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omron
{
    public static class GraphicsHelper
    {
        static Texture2D _pixel;
        static GraphicsDevice _gdevice;

        public static void Init(GraphicsDevice gdevice)
        {
            _gdevice = gdevice;
            _pixel = GeneratePixelTex(Color.White);
        }

        public static Texture2D GeneratePixelTex(Color color)
        {
            Texture2D tex = new Texture2D(_gdevice, 1, 1);
            tex.SetData<Color>(new Color[] { color });
            return tex;
        }
        public static Texture2D GenerateRectTex(Color color, int w, int h)
        {
            Texture2D tex = new Texture2D(_gdevice, w, h);
            Color[] cData = new Color[w * h];
            for (int i = 0; i < w * h; i++)
                cData[i] = color;
            tex.SetData<Color>(cData);
            return tex;
        }

        public static void DrawHex(Vector2 pos, float sideLen, Color c, SpriteBatch spriteBatch)
        {
            var hexTex = Omron.Framework.ResourceManager.Resources["hex"];
            spriteBatch.Draw(hexTex, pos, null, c, 0.0f, new Vector2(hexTex.Width / 2, hexTex.Height / 2), new Vector2(2 * sideLen / hexTex.Width, -2 * sideLen / hexTex.Height), SpriteEffects.None, 0.9f);  //height is scaled -1 since y-axis gets flipped
        }

        public static void DrawBall(SpriteBatch _spriteBatch, Texture2D tex, Vector2 cent, float rad, Color c, float depth)
        {
            _spriteBatch.Draw(tex, cent, null, c, 0f, new Vector2(tex.Width / 2, tex.Height / 2), 2 * rad / tex.Width, SpriteEffects.None, depth);
        }
        public static void DrawBallInv(SpriteBatch _spriteBatch, Texture2D tex, Vector2 cent, float rad, Color c, float depth)
        {
            _spriteBatch.Draw(tex, cent, null, c, 0f, new Vector2(tex.Width / 2, tex.Height / 2), 2 * rad / tex.Width * new Vector2(1, -1), SpriteEffects.None, depth);
        }

        /// <summary>
        /// By the way, this works terrible
        /// </summary>
        /// <param name="_spriteBatch"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="girth"></param>
        /// <param name="c"></param>
        public static void DrawRectOutlineInv(SpriteBatch _spriteBatch, float x, float y, float w, float h, float girth, Color c)
        {
            //-------------
            //|     2     |
            //|1         3|
            //|     4     |
            //-------------
            Vector2 v1 = new Vector2(x, y + h);
            Vector2 s1 = new Vector2(girth, -h);
            Vector2 v2 = new Vector2(x, y + girth);
            Vector2 s2 = new Vector2(w, -girth);
            Vector2 v3 = new Vector2(x + w - girth, y + h);
            Vector2 s3 = new Vector2(girth, -h);
            Vector2 v4 = new Vector2(x, y + h);
            Vector2 s4 = new Vector2(w, -girth);
            _spriteBatch.Draw(_pixel, v1, null, c, 0, Vector2.Zero, s1, SpriteEffects.None, 0.1f);
            _spriteBatch.Draw(_pixel, v2, null, c, 0, Vector2.Zero, s2, SpriteEffects.None, 0.1f);
            _spriteBatch.Draw(_pixel, v3, null, c, 0, Vector2.Zero, s3, SpriteEffects.None, 0.1f);
            _spriteBatch.Draw(_pixel, v4, null, c, 0, Vector2.Zero, s4, SpriteEffects.None, 0.1f);
        }

        public static void DrawRectOutline(SpriteBatch _spriteBatch, float x, float y, float w, float h, float girth, Color c)
        {
            //-------------
            //|     2     |
            //|1         3|
            //|     4     |
            //------------
            Rectangle rect1 = new Rectangle((int)x, (int)(y), (int)(girth), (int)(h));
            Rectangle rect2 = new Rectangle((int)(x), (int)(y), (int)(w), (int)(girth));
            Rectangle rect3 = new Rectangle((int)(x + w - girth), (int)(y), (int)(girth), (int)(h));
            Rectangle rect4 = new Rectangle((int)(x), (int)(y - girth + h), (int)(w), (int)(girth));
            _spriteBatch.Draw(_pixel, rect1, c);
            _spriteBatch.Draw(_pixel, rect2, c);
            _spriteBatch.Draw(_pixel, rect3, c);
            _spriteBatch.Draw(_pixel, rect4, c);
        }
       
        public static void DrawRectangle(SpriteBatch _spriteBatch, Vector2 pos, float w, float h, float rot, Color c)
        {
            Rectangle rect = new Rectangle((int)(pos.X), (int)(pos.Y), (int)w, (int)h);
            _spriteBatch.Draw(_pixel, rect, null, c, rot, new Vector2(.5f, .5f), SpriteEffects.None, 0.5f);
        }

        public static void DrawRectangleInv(SpriteBatch _spriteBatch, Vector2 cent, float w, float h, float rot, Color c)
        {
            _spriteBatch.Draw(_pixel, cent, null, c, rot, new Vector2(0.5f, 0.5f), new Vector2(w, -h), SpriteEffects.None, 0.2f);
        }

        public static void DrawRectangle(SpriteBatch _spriteBatch, float xmin, float xmax, float ymin, float ymax, Color c)
        {
            Rectangle rect = new Rectangle((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));
            _spriteBatch.Draw(_pixel, rect, null, c, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.5f);
        }
        public static void DrawLine(SpriteBatch _spriteBatch, Vector2 p1, Vector2 p2, Color c, float thick)
        {
            DrawRectangle( _spriteBatch, ((p1 + p2) / 2), thick, (p1 - p2).Length(), -(float)Math.Atan((p2 - p1).X / (p2 - p1).Y), c);
        }
        public static void DrawLineInv(SpriteBatch _spriteBatch, Vector2 p1, Vector2 p2, Color c, float thick)
        {
            DrawRectangleInv(_spriteBatch, ((p1 + p2) / 2), thick, (p1 - p2).Length(), -(float)Math.Atan((p2 - p1).X / (p2 - p1).Y), c);
        }

        public static void DrawArrow(SpriteBatch _spriteBatch, Vector2 pos, Vector2 targ, Color c, float thick)
        {
            float LERP = .7f;
            float WID = (targ - pos).Length() / 4;

            DrawLine(_spriteBatch, pos, targ, c, thick);
            var p = Vector2.Lerp(pos, targ, LERP);
            var t = Vector2.Normalize(targ - pos).Perpen();
            var p1 = p + t * WID;
            var p2 = p - t * WID;

            DrawLine(_spriteBatch, pos, targ, c, thick);
            DrawLine(_spriteBatch, p1, targ, c, thick);
            DrawLine(_spriteBatch, p2, targ, c, thick);
        }

        static Vector2 Transpose(this Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }
        static Vector2 Perpen(this Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }
    }
}

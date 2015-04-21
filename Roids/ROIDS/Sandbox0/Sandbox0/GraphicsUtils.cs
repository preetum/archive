using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox0
{
    public static class GraphicsUtils
    {
        static SpriteBatch _spriteBatch;
        static Texture2D _pixel;
        static Texture2D _ball;

        public static void Load(SpriteBatch sb, Texture2D pixel, Texture2D ball)
        {
            _spriteBatch = sb;
            _pixel = pixel;
            _ball = ball;
        }
        public static void Begin()
        {
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
        }
        public static void End()
        {
            _spriteBatch.End();
        }
        public static void DrawBall(Vector2 cent, float rad, Color c, byte alpha)
        {
            c.A = alpha;
            _spriteBatch.Draw(_ball, cent, null, c, 0f, new Vector2(_ball.Width / 2, _ball.Height / 2), 2 * rad / _ball.Width, SpriteEffects.None, 0.5f);
        }
        public static void DrawBall(Vector2 cent, float rad, Color c, byte alpha, float depth)
        {
            c.A = alpha;
            _spriteBatch.Draw(_ball, cent, null, c, 0f, new Vector2(_ball.Width / 2, _ball.Height / 2), 2 * rad / _ball.Width, SpriteEffects.None, depth);
        }
        public static void DrawRectangle(Vector2 cent, float w, float h, float rot, Color c)
        {
            Rectangle rect = new Rectangle((int)(cent.X), (int)(cent.Y), (int)w, (int)h);
            _spriteBatch.Draw(_pixel, rect, null, c, rot, new Vector2(.5f, .5f), SpriteEffects.None, 0.5f);
        }
        public static void DrawRectangleTop(Vector2 cent, float w, float h, float rot, Color c)
        {
            Rectangle rect = new Rectangle((int)(cent.X), (int)(cent.Y), (int)w, (int)h);
            _spriteBatch.Draw(_pixel, rect, null, c, rot, new Vector2(.5f, .5f), SpriteEffects.None, 0f);
        }
        public static void DrawRectangleTop(float xmin, float xmax, float ymin, float ymax, Color c)
        {
            Rectangle rect = new Rectangle((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));
            _spriteBatch.Draw(_pixel, rect, null, c, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
        }
        public static void DrawRectangle(float xmin, float xmax, float ymin, float ymax, Color c)
        {
            Rectangle rect = new Rectangle((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));
            _spriteBatch.Draw(_pixel, rect, null, c, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.5f);
        }
        public static void DrawLine(Vector2 p1, Vector2 p2, Color c, float thick)
        {
            DrawRectangle(((p1 + p2) / 2), thick, (p1 - p2).Length(), -(float)Math.Atan((p2 - p1).X / (p2 - p1).Y), c);
        }
        public static void DrawLineTop(Vector2 p1, Vector2 p2, Color c, float thick)
        {
            DrawRectangleTop(((p1 + p2) / 2), thick, (p1 - p2).Length(), -(float)Math.Atan((p2 - p1).X / (p2 - p1).Y), c);
        }
        public static void DrawArrow(Vector2 pos, Vector2 targ, Color c, float thick)
        {
            float LERP = .7f;
            float WID = (targ - pos).Length() / 4;

            DrawLine(pos, targ, c, thick);
            var p = Vector2.Lerp(pos, targ, LERP);
            var t = Vector2.Normalize(targ - pos).Perpen();
            var p1 = p + t * WID;
            var p2 = p - t * WID;

            DrawLine(pos, targ, c, thick);
            DrawLine(p1, targ, c, thick);
            DrawLine(p2, targ, c, thick);
        }

        public static Vector2 Transpose(this Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }
        public static Vector2 Perpen(this Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

    }
}

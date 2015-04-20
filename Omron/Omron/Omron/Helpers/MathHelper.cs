using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omron
{
    public static class MathHelper
    {
        public static Random Rand;

        public static class Constants
        {
            public static Vector2 HexAxis1, HexAxis2, HexAxis3;
            public const float sqrt3 = 1.732050807568f;
        }
        

        public static void Init()
        {
            Constants.HexAxis1 = Vector2.UnitY;
            Constants.HexAxis2 = MathHelper.Rotate(Vector2.UnitY, 2f / 3f * (float)Math.PI);
            Constants.HexAxis3 = MathHelper.Rotate(Vector2.UnitY, -2f / 3f * (float)Math.PI);

            Rand = new Random();
        }

        public static Point Floor(this Vector2 vect)
        {
            //return new Point((int)Math.Floor(vect.X), (int)Math.Floor(vect.Y)); //slooow
            return new Point(floor(vect.X), floor(vect.Y));
        }
        static int floor(float a)
        {
            int floor = (int)a;
            if (a < 0) return (floor - 1);
            return floor;
        }

        static int id = 0;
        public static int GetUniqueID()
        {
            return id++;
        }
        public static void ResetIDs()
        {
            id = 0;
        }

        public static Vector2 Rotate(Vector2 v, float rot)
        {
            return Vector2.Transform(v, Matrix.CreateRotationZ(rot));
        }
        public static float GetAngle(Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }
        /// <summary>
        /// rotates the vector 90deg CCW
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Perpen(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        public static Vector2 RandDir()
        {
            var a = Rand.NextDouble() * Math.PI * 2;
            Vector2 dir = new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
            return dir;
        }

        public static Vector2 SafeNormalize(Vector2 v)
        {
            if (v.Length() > 0)
                return Vector2.Normalize(v);
            else
                return Vector2.Zero;
        }
    }
}

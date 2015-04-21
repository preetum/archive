using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Utilities
{
    public static class MathUtils
    {
        public static Random Rand = new Random();
        public static Vector2 Rotate(Vector2 v, float rot)
        {
            return Vector2.Transform(v, Matrix.CreateRotationZ(rot));
        }
        public static Vector2 RotateInPlace(this Vector2 v, float rot)
        {
            return Rotate(v, rot);
        }
        public static Vector2 RandDirection()
        {
            var a =  (float)(Rand.NextDouble() * Math.PI * 2);
            return new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
        }
        public static float GetAngle(Vector2 vect)
        {
            return (float)Math.Atan2(vect.X, -vect.Y);
        }

        public static float Distance(Vector2 point1, Vector2 point2)
        {
            return (point1 - point2).Length();
        }

        /// <summary>
        /// lerps between colors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x">interpolation parameter: between 0-1</param>
        /// <returns></returns>
        public static Color ColorLerp(Color a, Color b, float x)
        {
            return Color.FromNonPremultiplied(a.ToVector4() * (1 - x) + b.ToVector4() * x);
        }
    }
}

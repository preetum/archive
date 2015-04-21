using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Utilities
{
    public class Size
    {
        public float Width;
        public float Height;

        public float X
        {
            get { return Width; }
            set { Width = value; }
        }
        public float Y
        {
            get { return Height; }
            set { Height = value; }
        }

        public Size()
        {
        }

        public Size(float width, float height)
        {
            this.Width = width;
            this.Height = height;
        }

        public static implicit operator Size(Vector2 v)
        {
            return new Size(v.X, v.Y);
        }
        public static implicit operator Vector2(Size s)
        {
            return new Vector2(s.Width, s.Height);
        }

        public Rectangle ToRectangle(Vector2 vector2)
        {
            return new Rectangle(
                (int)vector2.X,
                (int)vector2.Y,
                (int)Width,
                (int)Height);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Omron.Framework;

namespace Omron
{
    class Bar : Control
    {
        public Bar(Vector2 loc, int w, int h)
        {
            Location = new Point((int)loc.X, (int)loc.Y);
            Width = w;
            Height = h;
            Value = 1;
            rectPoly = new RectPoly(loc, Width, Height);
            ForeColor = Color.Blue;
            BackColor = Color.Wheat;
        }

        public Color ForeColor, BackColor;

        public Point Location;
        public int Width, Height;
        public float Value;

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ResourceManager.Resources["pixel"], new Rectangle(Location.X, Location.Y, Width, Height), BackColor);
            spriteBatch.Draw(ResourceManager.Resources["pixel"], new Rectangle(Location.X, Location.Y, (int)(Value * Width), Height), ForeColor);
        }

        RectPoly rectPoly;

        public override IPolygon GetBoundingPoly()
        {
            return rectPoly;
        }
    }
}

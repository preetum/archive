using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MainGame.GameWorld
{
    class BoolField
    {
        public bool[,] field;
        public int Width { get; private set; }
        public int Height{ get; private set; }
        public BoolField()
        {
        }
        public BoolField(Texture2D source)
        {
            field = new bool[source.Width, source.Height];
            Width = source.Width;
            Height = source.Height;
            Color[] cdata = new Color[source.Width*source.Height];
            source.GetData<Color>(cdata);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    int i = x + y * source.Width;
                    field[x, y] = cdata[i].R == 255;
                }
            }
        }

        public bool TestLineOfSight(Point a, Point b)
        {
            throw new NotImplementedException();
        }

        bool withinBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool TestIntersect(Rectangle rect)
        {
            for (int i = rect.X; i < rect.X + rect.Width; i++)
            {
                if (withinBounds(i, rect.Y) && field[i, rect.Y]) return true;
                if (withinBounds(i, rect.Y + rect.Height - 1) && field[i, rect.Y + rect.Height - 1]) return true;
            }
            for (int i = rect.Y; i < rect.Y + rect.Height; i++)
            {
                if (withinBounds(rect.X, i) && field[rect.X, i]) return true;
                if ( withinBounds(rect.X + rect.Width - 1, i) &&  field[rect.X + rect.Width - 1, i]) return true;
            }
            return false;
        }

    }
}

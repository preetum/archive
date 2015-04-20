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
    class Text : Control
    {
        public Text(Vector2 minPos, string txt)
        {
            Font = ResourceManager.Resources["font1"];
            pos = minPos;
            this.TextMsg = txt;           
        }

        public Text(Vector2 minPos, string txt, SpriteFont font)
            : this(minPos, txt)
        {
            this.Font = font;
        }

        Vector2 pos;

        RectPoly rectPoly;
        public SpriteFont Font;

        public Color Color = Color.SlateGray;

        string text = "";
        public string TextMsg
        {
            get { return text; }
            set
            {
                text = value;
                Vector2 strSize = Font.MeasureString(text);
                rectPoly = new RectPoly(pos, strSize.X, strSize.Y);
            }
        }

        public override IPolygon GetBoundingPoly()
        {
            return rectPoly;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var fsize = Font.MeasureString(TextMsg);
            spriteBatch.DrawString(Font, TextMsg, pos, Color);
        }
    }
}

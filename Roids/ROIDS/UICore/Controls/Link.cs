using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utilities;

namespace UICore.Controls
{
    public class Link : TextControl
    {
        
        Color mouseDownColor;
        public Color ForeColor { get; set; }

        public Link(Vector2 location, string text, Color foreColor, Element parent)
            : base(location, text, parent)
        {
            this.MouseDown += new MouseEventHandler(ImageButton_MouseDown);
            this.MouseClick += new MouseEventHandler(Link_MouseClick);
            this.MouseEnter += new MouseEventHandler(ImageButton_MouseEnter);
            this.MouseExit += new MouseEventHandler(ImageButton_MouseExit);

            mouseDownColor = Color.Yellow;
            ForeColor = foreColor;
        }


        public Link(Vector2 location, string text, Element parent)
            : this(location, text, Color.White, parent)
        {
        }


        void Link_MouseClick(Element sender, MouseEventArgs e)
        {
            TextColor = ForeColor;
        }

        void ImageButton_MouseExit(Element sender, MouseEventArgs e)
        {
            TextColor = ForeColor;
        }

        void ImageButton_MouseEnter(Element sender, MouseEventArgs e)
        {
            if (e.isDown(MouseButtons.Left))
                TextColor = mouseDownColor;
        }

        void ImageButton_MouseDown(Element sender, MouseEventArgs e)
        {
            TextColor = mouseDownColor;
        }
    }
}

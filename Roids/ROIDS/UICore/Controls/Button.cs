using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utilities;

namespace UICore.Controls
{
    public class Button : ImageControl
    {
        Color mouseDownColor;
        public Button(Vector2 location, Size size, Texture2D image, Element parent)
            : base(location, size, image, parent)
        {
            this.MouseDown += new MouseEventHandler(ImageButton_MouseDown);
            this.MouseEnter += new MouseEventHandler(ImageButton_MouseEnter);
            this.MouseExit += new MouseEventHandler(ImageButton_MouseExit);

            mouseDownColor = Color.Blue;
        }

        void ImageButton_MouseExit(Element sender, MouseEventArgs e)
        {
            Tint = Color.White;

        }

        void ImageButton_MouseEnter(Element sender, MouseEventArgs e)
        {
            if (e.isDown(MouseButtons.Left))
                Tint = mouseDownColor;
        }

        void ImageButton_MouseDown(Element sender, MouseEventArgs e)
        {
            Tint = mouseDownColor;
        }
    }
}

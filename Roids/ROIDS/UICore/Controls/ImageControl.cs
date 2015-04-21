using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UICore.Controls
{
    public abstract class ImageControl : ParentedElement
    {
        public ImageControl(Vector2 location, Utilities.Size size, Texture2D image, Element parent)
            : base (location, size, parent)
        {
            Location = location;
            ElementSize = size;
            Image = image;
            drawInternally = true;
            Tint = Color.White;
        }

        public ImageControl(Vector2 location, Utilities.Size size, bool drawImage, Element parent)
            : base(location, size, parent)
        {
            Location = location;
            ElementSize = size;
            drawInternally = drawImage;
            Tint = Color.White;
        }

        public Texture2D Image { get; set; }
        public Color Tint { get; set; }

        protected bool drawInternally;
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (drawInternally)
                DrawImage(spriteBatch, Image, Tint);
            base.Draw(time, spriteBatch);
        }
    }
}

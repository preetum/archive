using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UICore
{
    /// <summary>
    /// A UI Element such as a button or an information bar etc.
    /// </summary>
    /// 
    public delegate void ParentedElementEventHandler(ParentedElement sender);
    public abstract class ParentedElement : Element
    {
        public Element Parent { get; private set; }

        public event ParentedElementEventHandler Selected;
        public event ParentedElementEventHandler Deselected;

        public void Select()
        {
            //if (Parent is Control)
            //    ((Control)Parent).Parent.ActiveControl = (Control)Parent;
            if (Selected != null)
                Selected(this);
        }

        public void Deselect()
        {
            //ActiveControl = null;
            if (Deselected != null)
                Deselected(this);
        }



        /// <summary>
        /// Parent can only be set once!
        /// </summary>
        public void setParent(Element parent)
        {
            if (Parent != null)
                throw new Exception("Control already has a parent!");
            Parent = parent;

        }

        public Vector2 RelativeLocation
        {
            get
            {
                return Parent.GetRelativeLocation(this.Location);
            }
            set
            {
                this.Location = Parent.GetAbsoluteLocation(value);
            }
        }

        protected void Close()
        {
            Parent.RemoveChild(this);
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            Parent.ActiveChild = this;
            base.OnMouseClick(e);
        }

        public ParentedElement(Vector2 location, Utilities.Size size, Element parent)
            : base(location, size)
        {
            parent.AddChild(this);
        }


        protected Rectangle GetSourceRectangle(Texture2D img)
        {
            return GetSourceRectangle(img.Bounds);
        }
        protected Rectangle GetSourceRectangle(Rectangle sourceRect)
        {
            var leftCuttoff = -this.RelativeLocation.X;
            var rightCuttoff = this.RelativeLocation.X + ElementSize.Width - Parent.Location.X;

            var topCuttoff = -this.RelativeLocation.Y;
            var bottomCuttoff = this.RelativeLocation.Y + ElementSize.Height - Parent.Location.Y;

            var xScale = sourceRect.Width / ElementSize.Width;
            var yScale = sourceRect.Height / ElementSize.Height;

            float x = sourceRect.X;
            float y = sourceRect.Y;
            float w = sourceRect.Width;
            float h = sourceRect.Height;
            
            if (leftCuttoff > 0)
            {
                x += leftCuttoff * xScale;
                w -= leftCuttoff * xScale;
            }

            if (rightCuttoff > 0)
            {
                w -= rightCuttoff * xScale;
            }

            if (topCuttoff > 0)
            {
                y += topCuttoff * yScale;
                h -= topCuttoff * yScale;
            }


            if (bottomCuttoff > 0)
            {
                h -= bottomCuttoff * yScale;
            }

                       
            return new Rectangle(
                (int)x, (int)y, (int)w, (int)h);
        }

        protected Rectangle GetDestRectangle()
        {
            return ElementSize.ToRectangle(Location);
        }

        protected void DrawText(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color)
        {
            var txtSize = font.MeasureString(text);
            var charSize = (int)(text.Length / txtSize.X);
            var rightCutoff = (int)(position.X + txtSize.X - Parent.Location.X + Parent.ElementSize.Width);
            if (position.Y + txtSize.Y <= Parent.Location.Y + Parent.ElementSize.Height)
            {
                if (rightCutoff > 0)
                    spriteBatch.DrawString(font, text.Substring(0, text.Length - (rightCutoff * charSize)),
                        position, color);
                else
                    spriteBatch.DrawString(font, text, position, color);
            }
        }

        protected void DrawImage(SpriteBatch spriteBatch, Texture2D image, Color color)
        {
            spriteBatch.Draw(image, GetDestRectangle(), GetSourceRectangle(image), color);
        }
        protected void DrawImage(SpriteBatch spriteBatch, Texture2D image, Rectangle sourceRectangle, Color color)
        {
            spriteBatch.Draw(image, GetDestRectangle(), GetSourceRectangle(sourceRectangle), color);
        }
        protected void DrawImage(SpriteBatch spriteBatch, Texture2D image, Rectangle destRectangle, Rectangle sourceRectangle, Color color)
        {
            spriteBatch.Draw(image, destRectangle, GetSourceRectangle(sourceRectangle), color);
        }
    }
}

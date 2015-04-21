using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utilities;

namespace UICore
{
    public abstract class TextControl : ParentedElement
    {

        public TextControl(Vector2 location, string text, Element parent)
            : this(location, new Size(), text, true, parent)
        {
        }

        public TextControl(Vector2 location, Size size, string text, bool enableTextFit, Element parent)
            : base(location, size, parent)
        {
            Location = location;
            internallyDrawString = true;
            PositionText = AllignLeft;
            TextColor = Utilities.DefaultSettings.Settings["TextColor"];
            Font = ContentRepository.Repository["BasicSmallFont"];
                //ContentRepository.Repository[
                //Utilities.DefaultSettings.Settings["Font"]];
            
            Text = text;
            EnableTextFit = enableTextFit;
        }
        private bool _enableTextFit;
        public bool EnableTextFit
        {
            get
            {
                return _enableTextFit;
            }
            set
            {
                _enableTextFit = value;
                if (_enableTextFit)
                    this.ElementSize = (Utilities.Size)Font.MeasureString(Text);
            }
        }

        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                if (EnableTextFit)
                    this.ElementSize = (Utilities.Size)Font.MeasureString(Text);
                if (TextChanged != null)
                    TextChanged(this);
            }
        }
        public Color TextColor { get; set; }

        SpriteFont _font;
        public virtual SpriteFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
                if (EnableTextFit)
                    this.ElementSize = (Utilities.Size)Font.MeasureString(Text);
                if (FontChanged != null)
                    FontChanged(this);
            }
        }

        public event ElementEventHandler TextChanged;
        public event ElementEventHandler FontChanged;

        public Func<Vector2> PositionText;

        public Vector2 AllignLeft()
        {
            return this.Location;
        }

        protected bool internallyDrawString;
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (internallyDrawString)
                spriteBatch.DrawString(Font, Text, PositionText(), TextColor);

            base.Draw(time, spriteBatch);
        }
    }
}

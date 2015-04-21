using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Utilities;
namespace UICore.Controls
{
    /// <summary>
    /// Supports AlphaNumeric Basic Punctuation Input Only
    /// </summary>
    public class AlphaNumTextBox : TextControl
    {
        private int _cursorPosition;
        public int CursorPosition
        {
            get
            {
                return _cursorPosition;
            }
            set
            {
                if (value >= 0 && value <= Text.Length)
                    _cursorPosition = value;
                if (CursorMoved != null)
                    CursorMoved(this);
            }
        }
        public Color CursorColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public float BorderThickness { get; set; }


        public const float CursorThickness = 1;

        public float KeyDelay { get; set; }
        public event ElementEventHandler CursorMoved;
        public event ElementEventHandler EntrySubmitted;

        public AlphaNumTextBox(Vector2 location, float boxWidth, SpriteFont font, Element parent)
            : this(location, boxWidth, parent)
        {
            Font = font;
        }
        public AlphaNumTextBox(Vector2 location, float boxWidth, Element parent)
            : base(location, new Size(), String.Empty, false, parent)
        {
            ElementSize = new Size(boxWidth, Font.MeasureString("A").Y);
            this.FontChanged += new ElementEventHandler(TextBox_FontChanged);
            this.MouseDown += new MouseEventHandler(TextBox_MouseDown);
            this.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            this.KeyPressDown += new KeyEventHandler(AlphaNumTextBox_KeyPressDown);
            this.KeyUp += new KeyEventHandler(AlphaNumTextBox_KeyUp);
            
            this.PositionText = () => this.Location + BorderThickness * Vector2.One;

            // Default Settings
            this.CursorColor = Color.Gray;
            this.BackgroundColor = Color.White;
            this.TextColor = Color.Red;
            this.BorderColor = Color.Gold;
            this.BorderThickness = 2f;
            this.KeyDelay = 500f;
            
        }




        private double delay; // For delayed repeat on key down
        public override void Update(GameTime time)
        {
            base.Update(time);
            if (delay > 0)
                delay -= time.ElapsedGameTime.TotalMilliseconds;
        }
        void AlphaNumTextBox_KeyPressDown(Element sender, KeyEventArgs e)
        {
            delay = KeyDelay;
        }
        void TextBox_KeyDown(Element sender, KeyEventArgs e)
        {
            // Test for Modifiers -- Can optimize further but don't think its worth it
            var shift =
                e.InterestingKeys.FirstOrDefault<Keys>(x => x == Keys.LeftShift || x == Keys.RightShift)
                != Keys.None;

            // Extract Modifiers
            Keys[] keys;
            if (shift)
                keys = e.InterestingKeys.Except<Keys>(new Keys[] { Keys.LeftShift, Keys.RightShift }).ToArray<Keys>();
            else keys = e.InterestingKeys;

            // Delayed repeat
            if (delay <= 0 || delay >= KeyDelay) // First press and subsequent upon delay are allowed
            {
                delay = 100; // delay for repeating keys is standard and less than first delay

                // Convert key to string
                if (keys.Length <= 0)
                    return;
                var key = KeyEventArgs.KeyToString(keys[keys.Length - 1], shift);

                if (key != "NotSupported")
                {
                    if (CursorPosition == Text.Length)
                        Text += key;
                    else
                        Text = Text.Insert(CursorPosition, key);
                    CursorPosition++;
                }
                else
                    switch (keys[keys.Length - 1]) // Check for special keys
                    {
                        case Keys.Left:
                            CursorPosition--;
                            break;
                        case Keys.Right:
                            CursorPosition++;
                            break;
                        case Keys.Back:
                            if (CursorPosition > 0)
                                Text.Remove(CursorPosition - 1);
                            break;
                        case Keys.Delete:
                            if (CursorPosition < Text.Length)
                                Text.Remove(CursorPosition);
                            break;
                    }
            }
        }
        void AlphaNumTextBox_KeyUp(Element sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Enter)
                && EntrySubmitted != null)
                EntrySubmitted(this);
            delay = -1; // set delay to non updating
        }

        void TextBox_MouseDown(Element sender, MouseEventArgs e)
        {
            var scalePos = Font.MeasureString("A"); // pxl per char
            CursorPosition = (int)(scalePos.X * this.GetRelativeLocation(e.CurrentPosition).X);
        }

        void TextBox_FontChanged(Element sender)
        {
            ElementSize = new Size(ElementSize.Width, Font.MeasureString("A").Y);
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {


            //// Draw Cursor
            var relCursorPos = Font.MeasureString(Text.Substring(0, CursorPosition)); // pxl/char
            Utilities.GraphicsUtils.DrawLine(
                this.Location + relCursorPos.X * Vector2.UnitX + BorderThickness*Vector2.UnitY,
                this.Location + relCursorPos - BorderThickness*Vector2.UnitY, 
                CursorColor, CursorThickness);



            // Draw Box
            Utilities.GraphicsUtils.DrawRectangle(
                this.Location.X + BorderThickness,
                this.Location.X + this.ElementSize.Width - BorderThickness,
                this.Location.Y + BorderThickness,
                this.Location.Y + this.ElementSize.Height - BorderThickness,
                BackgroundColor);

            Utilities.GraphicsUtils.DrawRectangle(
                this.Location.X,// - BorderThickness,
                this.Location.X + this.ElementSize.Width,// + BorderThickness,
                this.Location.Y,// - BorderThickness,
                this.Location.Y + this.ElementSize.Height,// + BorderThickness,
                BorderColor);

            spriteBatch.DrawString(Font, Text, PositionText(), TextColor);
        }

    }
}

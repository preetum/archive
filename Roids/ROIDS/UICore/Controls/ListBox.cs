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
    public delegate void ListBoxEventHandler(ListBox sender, string selection);
    public class ListBox : ParentedElement
    {
        private List<string> Text { get; set; }
        public SpriteFont Font;
        public Color NormalColor;
        public Color SelectionColor;

        private int _selectedItem;

        public int SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            private set
            {
                if (!(value > MaximimumElementsDisplayed + drawStart || value < drawStart))
                    _selectedItem = value;
            }
        }
        private float _cellHeight;

        public Vector2 BufferWidth
        {
            get { return _bufferWidth; }
            set
            {
                this.ElementSize = new Size(
                    this.ElementSize.Width - 2*BufferWidth.X + 2*value.X,
                    this.ElementSize.Height - 2*BufferWidth.Y + 2*value.Y);
                _bufferWidth = value;
            }
        }
        private Vector2 _bufferWidth;

        public Size TextZoneSize
        {
            get
            {
                return new Size(ElementSize.X - 2 * BufferWidth.X,
                    ElementSize.Y - 2 * BufferWidth.Y);
            }
        }
        private bool _dynamicallyResize;

        public bool DynamicallyResize
        {
            get
            {
                return _dynamicallyResize;
            }
            set
            {
                _dynamicallyResize = value;
                if (_dynamicallyResize)
                    Resize();
            }
        }

        public int MaximimumElementsDisplayed
        {
            get
            {
                var max = (int)(this.TextZoneSize.Height / _cellHeight);
                return (max >= Text.Count) ? Text.Count - 1 : max;
            }

            set
            {
                int count = value;
                if (count > Text.Count)
                    count = Text.Count;
                else if (count < 1)
                    count = 1;

                this.ElementSize = new Size(
                    ElementSize.Width,
                    _cellHeight * (value-1) + 2 * BufferWidth.Y); 
            }
        }
        public event ListBoxEventHandler SelectionMade;

        public ListBox(Vector2 location, string[] text, SpriteFont font, Color fontColor, Element parent)
            : this(location,  text, font, fontColor, new Size(), parent)
        {
            DynamicallyResize = true;
        }

        private float _updateTime;
        private float _pressedTime;

        private int drawStart;
      
        public void AddText(string text)
        {
            Text.Add(text);
            if (DynamicallyResize)
                Resize();
        }

        public void DeleteText(int i)
        {
            Text.RemoveAt(i);
            if (DynamicallyResize)
                Resize();
        }

        /// <summary>
        /// Does not account for parent's size
        /// </summary>
        private void Resize()
        {
            var width = (float)Text.Max<string>(x => (decimal)Font.MeasureString(x).X) + 2 * _bufferWidth.X;
            var height = _cellHeight * Text.Count + 2 * _bufferWidth.X;

            this.ElementSize = new Size(width, height);
        }
        public ListBox(Vector2 location, string[] text, SpriteFont font, Color fontColor, Size size, Element parent)
            : base(location, size, parent)
        {
            Text = new List<string>(text);
            SelectedItem = 0;

            Font = font;
            NormalColor = fontColor;
            SelectionColor = new Color(255 - NormalColor.R, 255 - NormalColor.G, 255 - NormalColor.B);

            _cellHeight = font.LineSpacing;
            DynamicallyResize = false;

            _bufferWidth = Vector2.Zero;

            this.KeyDown += new KeyEventHandler(ListBox_KeyDown);
            this.KeyUp += new KeyEventHandler(ListBox_KeyUp);
            this.KeyPressDown += new KeyEventHandler(ListBox_KeyPressDown);
            this.MouseClick += new MouseEventHandler(ListBox_MouseClick);
            this.MouseDown += new MouseEventHandler(ListBox_MouseDown);
        }

        void ListBox_MouseDown(Element sender, MouseEventArgs e)
        {
            SelectedItem = (int)((e.CurrentMouseState.Y - this.Location.Y) / _cellHeight);
        }

        void ListBox_MouseClick(Element sender, MouseEventArgs e)
        {
            OnSelectionMade();
        }

        void ListBox_KeyPressDown(Element sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Enter))
                OnSelectionMade();
        }

        void ListBox_KeyUp(Element sender, KeyEventArgs e)
        {
            _pressedTime = -1;

        }

        public override void Update(GameTime time)
        {
            _updateTime = (float)time.TotalGameTime.TotalMilliseconds;
            base.Update(time);
        }
        void ListBox_KeyDown(Element sender, KeyEventArgs e)
        {
            if (_pressedTime == -1 ||
                _updateTime - _pressedTime > 100)
            {
                if (e.InterestingKeys.Contains<Keys>(Keys.Down))
                {                    // Scroll Down
                    if (SelectedItem + 1 > MaximimumElementsDisplayed)
                        drawStart = SelectedItem + 1 - MaximimumElementsDisplayed;

                    SelectedItem += (SelectedItem + 1 == Text.Count) ? 0 : 1;
                    _pressedTime = _updateTime;

                }
                else if (e.InterestingKeys.Contains<Keys>(Keys.Up))
                {
                    // Scroll Up
                    if (SelectedItem - 1 < drawStart && drawStart > 0)
                        drawStart = SelectedItem - 1;
                    SelectedItem -= (SelectedItem == 0) ? 0 : 1;
                    _pressedTime = _updateTime;

                }
            }
            
        }

        private void OnSelectionMade()
        {
            if (SelectionMade != null)
                SelectionMade(this, Text[SelectedItem]);
        }

       
        public override void Draw(Microsoft.Xna.Framework.GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            var drawLocation = this.GetAbsoluteLocation(BufferWidth);
            var x = drawLocation.X;
            var y = drawLocation.Y;

            // Draw Text
            for (int i = 0; i < Text.Count - drawStart; i++)
            {
                if (i+drawStart != SelectedItem)
                    spriteBatch.DrawString(Font, Text[(i+drawStart)], 
                        new Vector2(x, y + _cellHeight * i), NormalColor);

                if (_cellHeight * (i+1) > this.TextZoneSize.Height)
                    break;
            }

            // Highlight Selection
            DrawText(spriteBatch, Font, Text[SelectedItem],
                new Vector2(x, y + _cellHeight * (SelectedItem-drawStart)), SelectionColor);

            base.Draw(time, spriteBatch);
        }
    }
}

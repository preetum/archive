using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Utilities;

namespace UICore.Controls
{
    public delegate void OptionBoxEventHandler(OptionBox sender);
    public class OptionBox : ParentedElement
    {
        class CheckBox : ParentedElement
        {
            public bool isChecked;
            public CheckBox(Vector2 location, Element parent)
                : base(location, BoxSize * Vector2.One, parent)
            {
                OuterColor = Color.Gold;
                InnerColor = Color.Blue;
            }


            public Color OuterColor { get; private set; }
            public Color InnerColor { get; private set; }
            public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
            {
                GraphicsUtils
                    .DrawBall(
                    Location + BoxSize / 2 * Vector2.One, BoxSize / 2, OuterColor, 255);

                GraphicsUtils
                    .DrawBall(
                    Location + BoxSize / 2 * Vector2.One, BoxSize / 3, InnerColor, 255);

                if (isChecked)
                    GraphicsUtils
                        .DrawBall(
                        Location + BoxSize / 2 * Vector2.One, BoxSize / 4, OuterColor, 255);
                base.Draw(time, spriteBatch);
            }
        }

        Label label;
        CheckBox box;
        const float BoxSize = 10f;

        public event OptionBoxEventHandler Checked;
        public event OptionBoxEventHandler Unchecked;
        public event OptionBoxEventHandler Toggled;

        private void FireEvent(OptionBoxEventHandler evnt)
        {
            if (evnt != null)
                evnt(this);
        }
        public bool IsChecked
        {
            get { return box.isChecked; }
            set 
            { 
                var temp = box.isChecked;
                box.isChecked = value;
                
                if (temp != value)
                {
                    FireEvent(Toggled);
                    if (value) FireEvent(Checked);
                    else FireEvent(Unchecked);
                }
            }
        }
        public string Text
        {
            get { return label.Text; }
            set { label.Text = value; }
        }
        public OptionBox(Vector2 location, string text, Element parent)
            : base(location, new Size(), parent)
        {
            label = new Label(location + (BoxSize + 2f) * Vector2.UnitX, text, this);

            box = new CheckBox(Vector2.Zero, this);

            box.RelativeLocation = new Vector2(BoxSize / 2, label.ElementSize.Height / 2);
            box.CenterAboutLocation(false);


            box.MouseClick += new MouseEventHandler(box_MouseClick);
            this.EnableAutomaticResize = true;
        }

        void box_MouseClick(Element sender, MouseEventArgs e)
        {
            IsChecked = !IsChecked;   
        }
    }
}

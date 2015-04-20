using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omron.Framework
{
    class ListBox
    {
        public ListBox(int itemW, int itemH, UIManager ui, int x, int y)
        {
            UIMan = ui;
            buts = new List<Control>();
            shown = false;
            iW = itemW;
            iH = itemH;
            X = x;
            Y = y;
            maxX = int.MaxValue;
        }

        public ListBox(int itemW, int itemH, UIManager ui, int x, int y, int mX)
            : this(itemW, itemH, ui, x, y)
        {
            maxX = mX;
        }

        int iW, iH, X, Y, maxX;

        public void SetItems(string[] items)
        {
            if (shown)
            {
                Hide();
            }
            buts.Clear();
            Vector2 lSize = new Vector2(iW, iH);
            int interval = 10;
            Vector2 nextLoc = new Vector2(X, Y);
            foreach (string lab in items)
            {
                SolidButton but = new SolidButton(nextLoc, lSize.X, lSize.Y, "UnitMenuFont");
                but.Text = lab;
                but.ButtonPressed += new ButtonPressedEventHandler(but_ButtonPressed);
                buts.Add(but);
                nextLoc.X += interval + lSize.X;
            }
        }

        public void SetItems(string[] items, Texture2D[] images)
        {
            if (shown)
            {
                Hide();
            }
            buts.Clear();
            Vector2 lSize = new Vector2(iW, iH);
            int interval = 10;
            Vector2 nextLoc = new Vector2(X, Y);
            int count = 0;
            foreach (string lab in items)
            {
                ImageButton but = new ImageButton(nextLoc, lSize.X, lSize.Y, "UnitMenuFont", images[count]);
                but.Text = lab;
                but.ButtonPressed += new ButtonPressedEventHandler(but_ButtonPressed);
                buts.Add(but);
                nextLoc.X += interval + lSize.X;
                count++;
            }
        }

        public void SetItems(Actor[] actors)
        {
            if (shown)
                Hide();
            buts.Clear();
            
            int curX = X;
            int curY = Y;
            foreach (Actor act in actors)
            {
                ActorIcon icon = new ActorIcon(curX, curY, iW, iH, act);
                icon.ActiveColor = Color.White;
                icon.InactiveColor = Color.White;
                icon.ButtonPressed += new ButtonPressedEventHandler(but_ButtonPressed);
                buts.Add(icon);
                curX += iW;
                if (curX + iW > maxX)
                {
                    curX = X;
                    curY += iH;
                }
            }
        }

        public event Omron.Actors.MenuItemPressed ItemPressed;

        void but_ButtonPressed(SolidButton sender)
        {
            int index = buts.IndexOf(sender);

            if (ItemPressed != null && index != -1)
                ItemPressed(index);
        }

        public void Show()
        {
            if (!shown)
            {
                shown = true;
                UIMan.AddRange(buts);
            }
        }

        public void Hide()
        {
            if (shown)
            {
                shown = false;
                UIMan.RemoveRange(buts);
            }
        }

        public void Clear()
        {
            Hide();
            buts.Clear();
        }

        bool shown;

        List<Control> buts;
        UIManager UIMan;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Omron.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omron
{
    public delegate void ButtonPressedEventHandler(SolidButton sender);

    public abstract class Control : ISpriteDrawable
    {
        public abstract IPolygon GetBoundingPoly();
        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void PreUpdate() { }
        public virtual void PostUpdate() { }

        /// <summary>
        /// position is relative to control
        /// </summary>
        public event MouseClickEventHandler MouseLeftDown;
        public event MouseClickEventHandler MouseRightDown;
        public event MouseClickEventHandler MouseLeftUp;
        public event MouseClickEventHandler MouseRightUp;
        public event MouseOverEventHandler MouseOver;
        public event MouseOverEventHandler MouseDrag;
        public void OnMouseLeftDown(Vector2 mPos)
        {
            if (MouseLeftDown != null)
                MouseLeftDown(mPos);
        }
        public void OnMouseRightDown(Vector2 mPos)
        {
            if (MouseRightDown != null)
                MouseRightDown(mPos);
        }
        public void OnMouseLeftUp(Vector2 mPos)
        {
            if (MouseLeftUp != null)
                MouseLeftUp(mPos);
        }
        public void OnMouseRightUp(Vector2 mPos)
        {
            if (MouseRightUp != null)
                MouseRightUp(mPos);
        }
        public void OnMouseOver(Vector2 mPos)
        {
            if (MouseOver != null)
                MouseOver(mPos);
        }
        public void OnMouseDrag(Vector2 mPos)
        {
            if (MouseDrag != null)
                MouseDrag(mPos);
        }

        bool _active = true;
        public bool Active { get { return _active; } set { _active = value; } }
    }
    public class SolidButton : Control
    {
        protected float w, h;
        protected Texture2D tex;
        protected Vector2 pos;

        /// <summary>
        /// returns the min-position (ie:upper-left)
        /// </summary>
        public Vector2 Position { get { return pos; } }
        public float Width { get { return w; } }
        public float Height { get { return h; } }

        RectPoly rectPoly;
        string fontStr;

        public Color ActiveColor = Color.LightPink;
        public Color InactiveColor = Color.Azure;
        public Color TextColor = Color.Black;
        Color currColor;


        public string Text = "";

        public override IPolygon GetBoundingPoly()
        {
            return rectPoly;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex, pos, null, currColor, 0.0f, Vector2.Zero, new Vector2(w / tex.Width, h / tex.Height), SpriteEffects.None, 0.01f);

            SpriteFont font = ResourceManager.Resources[fontStr];
            var fsize = font.MeasureString(Text);
            var cent = new Vector2(pos.X + w / 2f, pos.Y + h / 2f);
            spriteBatch.DrawString(font, Text, cent - 0.5f * fsize, TextColor);
        }

        public SolidButton(Vector2 minPos, float width, float height)
        {
            fontStr = "font1";
            w = width;
            h = height;
            pos = minPos;
            rectPoly = new RectPoly(minPos, width, height);
            tex = ResourceManager.Resources["pixel"];

            currColor = InactiveColor;

            MouseOver += new MouseOverEventHandler(Button_MouseOver);
            MouseLeftDown += new MouseClickEventHandler(SolidButton_MouseLeftDown);
        }

        public SolidButton(Vector2 minPos, float width, float height, string fnt)
            : this(minPos, width, height)
        {
            fontStr = fnt;
        }

        bool prevPressed = false;

        void SolidButton_MouseLeftDown(Vector2 mPos)
        {
            if (ButtonPressed != null)
                ButtonPressed(this);
        }

        public event ButtonPressedEventHandler ButtonPressed;

        void Button_MouseOver(Vector2 mPos)
        {
            currColor = ActiveColor;
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            currColor = InactiveColor;
        }

    }

    public delegate void MouseOverEventHandler(Vector2 mPos);
    public delegate void MouseClickEventHandler(Vector2 mPos);
    public delegate void KeyPressEventHandler(Keys key);

    public class UIManager : ISpriteDrawable
    {
        List<Control> controls;

        MouseState mPrev;
        KeyboardState kPrev;

        public bool ShiftDown;
        public bool CtrlDown;
        public bool AltDown;

        #region events
        public event MouseClickEventHandler MouseLeftDown;
        public event MouseClickEventHandler MouseRightDown;
        public event MouseClickEventHandler MouseLeftHeld;
        public event MouseClickEventHandler MouseRightHeld;
        public event MouseClickEventHandler MouseLeftUp;
        public event MouseClickEventHandler MouseRightUp;
        public event MouseOverEventHandler MouseOver;
        void OnMouseLeftDown(Vector2 mPos)
        {
            if (MouseLeftDown != null)
                MouseLeftDown(mPos);
        }
        void OnMouseRightDown(Vector2 mPos)
        {
            if (MouseRightDown != null)
                MouseRightDown(mPos);
        }
        void OnMouseLeftHeld(Vector2 mPos)
        {
            if (MouseLeftHeld != null)
                MouseLeftHeld(mPos);
        }
        void OnMouseRightHeld(Vector2 mPos)
        {
            if (MouseRightHeld != null)
                MouseRightHeld(mPos);
        }
        void OnMouseLeftUp(Vector2 mPos)
        {
            if (MouseLeftUp != null)
                MouseLeftUp(mPos);
        }
        void OnMouseRightUp(Vector2 mPos)
        {
            if (MouseRightUp != null)
                MouseRightUp(mPos);
        }
        public void OnMouseOver(Vector2 mPos)
        {
            if (MouseOver != null)
                MouseOver(mPos);
        }

        public event KeyPressEventHandler KeyHeld;
        public event KeyPressEventHandler KeyDown;
        public event KeyPressEventHandler KeyUp;
        void OnKeyHeld(Keys key)
        {
            if (KeyHeld != null)
                KeyHeld(key);
        }
        void OnKeyDown(Keys key)
        {
            if (KeyDown != null)
                KeyDown(key);
        }
        void OnKeyUp(Keys key)
        {
            if (KeyUp!= null)
                KeyUp(key);
        }
        #endregion events

        public UIManager()
        {
            controls = new List<Control>();
        }
        bool mouseWithinWindow()
        {
            var mPos = GetMousePos();
            return mPos.X >= 0 && mPos.Y >= 0 &&
                mPos.X <= ResourceManager.GraphicsDevice.PresentationParameters.BackBufferWidth &&
                mPos.Y <= ResourceManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
        }

        public void ResetScroll()
        {
            begScr = Mouse.GetState().ScrollWheelValue;
        }

        int begScr = 0;
        int scrVal = 0;
        public int ScrollValue
        {
            get { return begScr - scrVal; }
        }


        Control activeControl;
        public void Update()
        {
            if (!ResourceManager.BaseGame.IsActive) return;

            lock (controls)
            {

                foreach (Control c in controls)
                {
                    if (c.Active)
                        c.PreUpdate();
                }

                if (mPrev == null)
                    mPrev = Mouse.GetState();
                if (kPrev == null)
                    kPrev = Keyboard.GetState();

                var mCurr = Mouse.GetState();
                var kCurr = Keyboard.GetState();

                var mPos = GetMousePos();

                //if activecontrol switches, simulate-fire mouseup for previous control
                var prevControl = activeControl;
                /*var newactiveControl = controls.FirstOrDefault(c => CollisionTester.TestPointInside(mPos, c.GetBoundingPoly()) && c.Active);
                if (newactiveControl != prevControl)
                {
                    if (mCurr.LeftButton == ButtonState.Pressed)
                    {
                        if (prevControl != null)
                            prevControl.OnMouseLeftUp(mPos);
                        else
                            OnMouseLeftUp(mPos);
                    }
                    if (mCurr.RightButton == ButtonState.Pressed)
                    {
                        if (prevControl != null)
                            prevControl.OnMouseRightUp(mPos);
                        else
                            OnMouseRightUp(mPos);
                    }
                }
                activeControl = newactiveControl;*/

                

                //process real events
                if (activeControl != null)
                {
                    //fire control's events if mouse if over a control
                    if (mCurr.LeftButton == ButtonState.Pressed && mPrev.LeftButton == ButtonState.Released)
                        activeControl.OnMouseLeftDown(mPos);
                    if (mCurr.LeftButton == ButtonState.Released && mPrev.LeftButton == ButtonState.Pressed)
                        activeControl.OnMouseLeftUp(mPos);
                    if (mCurr.RightButton == ButtonState.Pressed && mPrev.RightButton == ButtonState.Released)
                        activeControl.OnMouseRightDown(mPos);
                    if (mCurr.RightButton == ButtonState.Released && mPrev.RightButton == ButtonState.Pressed)
                        activeControl.OnMouseRightUp(mPos);
                    if (mCurr.LeftButton == ButtonState.Pressed)
                        activeControl.OnMouseDrag(mPos);

                    activeControl.OnMouseOver(mPos);
                }
                else if (mouseWithinWindow())
                {
                    //if mouse isn't over a control, fire main UI events
                    if (mCurr.LeftButton == ButtonState.Pressed && mPrev.LeftButton == ButtonState.Released)
                        OnMouseLeftDown(mPos);
                    if (mCurr.LeftButton == ButtonState.Released && mPrev.LeftButton == ButtonState.Pressed)
                        OnMouseLeftUp(mPos);
                    if (mCurr.LeftButton == ButtonState.Pressed)
                        OnMouseLeftHeld(mPos);
                    if (mCurr.RightButton == ButtonState.Pressed)
                        OnMouseRightHeld(mPos);
                    if (mCurr.RightButton == ButtonState.Pressed && mPrev.RightButton == ButtonState.Released)
                        OnMouseRightDown(mPos);
                    if (mCurr.RightButton == ButtonState.Released && mPrev.RightButton == ButtonState.Pressed)
                        OnMouseRightUp(mPos);

                    OnMouseOver(mPos);
                }
                activeControl = controls.FirstOrDefault(c => CollisionTester.TestPointInside(mPos, c.GetBoundingPoly()) && c.Active);
                if (prevControl != activeControl && mCurr.LeftButton == ButtonState.Pressed)
                    activeControl = prevControl;



                scrVal = mCurr.ScrollWheelValue;

                var currKeys = kCurr.GetPressedKeys();
                var prevKeys = kPrev.GetPressedKeys();

                var downKeys = from key in currKeys where !prevKeys.Contains(key) select key;
                var upKeys = from key in prevKeys where !currKeys.Contains(key) select key;

                foreach (var key in currKeys)
                    OnKeyHeld(key);
                foreach (var key in downKeys)
                    OnKeyDown(key);
                foreach (var key in upKeys)
                    OnKeyUp(key);

                ShiftDown = kCurr.IsKeyDown(Keys.LeftShift) || kCurr.IsKeyDown(Keys.RightShift);
                CtrlDown = kCurr.IsKeyDown(Keys.LeftControl) || kCurr.IsKeyDown(Keys.RightControl);
                AltDown = kCurr.IsKeyDown(Keys.LeftAlt) || kCurr.IsKeyDown(Keys.RightAlt);

                mPrev = mCurr;
                kPrev = kCurr;

                foreach (Control c in controls)
                {
                    if (c.Active)
                        c.PostUpdate();
                }

            }
        }
        public Vector2 GetMousePos()
        {
            var mCurr = Mouse.GetState();
            return new Vector2(mCurr.X, mCurr.Y);
        }
        public void AddControl(Control control)
        {
            lock (controls)
                controls.Add(control);
        }

        public void RemoveControl(Control control)
        {
            lock (controls)
                controls.Remove(control);
        }

        public void RemoveRange(List<Control> contrs)
        {
            lock (controls)
                controls.RemoveAll(item => contrs.Contains(item));
        }

        public void AddRange(List<Control> contrs)
        {
            lock (controls)
                controls.AddRange(contrs);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            lock(controls)
                foreach (Control c in controls)
                {
                    if (c.Active)
                        c.Draw(spriteBatch);
                }
        }

        public bool ContainsControl(Control cont)
        {
            return controls.Contains(cont);
        }
    }
}

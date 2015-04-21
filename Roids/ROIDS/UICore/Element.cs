using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Utilities;

namespace UICore
{

    public abstract class Element
    {
        private MouseState _prevMouseState;
        private KeyboardState _prevKeyState;

        private ParentedElement activeChild;
        public ParentedElement ActiveChild
        {
            get
            {
                return activeChild;
            }

            set
            {
                if (activeChild != null)
                    activeChild.Deselect();
                activeChild = value;
                if (activeChild != null)
                    activeChild.Select();
            }
        }

        /// <summary>
        /// Do Not Modify This List
        /// </summary>
        public List<ParentedElement> Children { get; protected set; }

        private Vector2 location;
        public virtual Vector2 Location 
        {
            get
            {
                return location;
            }
            set
            {
                var temp = location;
                location = value;
                if (temp != location)
                    OnMoved();
            }
        }

        /// <summary>
        /// Set Location without firing event
        /// </summary>
        public void SilentlyMove(Vector2 newLocation)
        {
            location = newLocation;
        }
        /// <summary>
        /// Move the entire element, including the child elements
        /// </summary>
        public void MoveAll(Vector2 location)
        {
            Vector2 delta = location - Location;
            Location = location;
            foreach (ParentedElement child in Children)
            {
                child.MoveAll(child.Location + delta);
            }
        }

        /// <summary>
        /// Makes the Top Left corner of the element the center
        /// </summary>
        public void CenterAboutLocation(bool moveChildren)
        {
            if (moveChildren)
                MoveAll(this.Location - (Vector2)this.ElementSize / 2);
            else this.Location -= (Vector2)this.ElementSize / 2;
        }

        private Size _size;
        public virtual Size ElementSize
        {
            get
            {
                return _size;
            }
            protected set
            {
                var prev = _size;
                _size = value;
                if (prev != _size)
                    OnResized();
            }
        }

        private bool _enableAutomaticResize;
        public bool EnableAutomaticResize
        {
            get
            {
                return _enableAutomaticResize;
            }
            set
            {
                _enableAutomaticResize = value;
                if (_enableAutomaticResize)
                    ResizeToFitChildren(0f);
            }
        }

        private bool _enableAutomaticEnlarge;
        public bool EnableAutomaticEnlarge
        {
            get
            {
                return _enableAutomaticEnlarge;
            }
            set
            {
                _enableAutomaticEnlarge = value;
                if (_enableAutomaticEnlarge)
                    EnlargeToFitChildren(0f);
            }
        }

        public void ResizeToFitChildren(float bufferWidth)
        {
            var maxLeft = Children.Min<ParentedElement>(
                x => x.RelativeLocation.X);
            var maxRight = Children.Max<ParentedElement>(
                x => x.RelativeLocation.X + x.ElementSize.Width);
            var maxTop = Children.Min<ParentedElement>(
                x => x.RelativeLocation.Y);
            var maxBottom = Children.Max<ParentedElement>(
                x => x.RelativeLocation.Y + x.ElementSize.Height);

            this.Location -= (maxLeft - bufferWidth) * Vector2.UnitX;
            this.ElementSize += (maxRight - bufferWidth) * Vector2.UnitX;
            this.Location -= (maxTop - bufferWidth) * Vector2.UnitY;
            this.ElementSize += (maxBottom - bufferWidth) * Vector2.UnitY;
        }

        /// <summary>
        /// Does Not Shrink Element
        /// </summary>
        public void EnlargeToFitChildren(float bufferWidth)
        {

            foreach (ParentedElement child in Children)
            {
                if (child.RelativeLocation.X <= 0)
                    this.Location -= (child.RelativeLocation.X - bufferWidth) * Vector2.UnitX;
                if (child.RelativeLocation.X + child.ElementSize.Width > this.ElementSize.Width)
                    this.ElementSize +=
                        (child.RelativeLocation.X + child.ElementSize.Width - this.ElementSize.Width + bufferWidth)
                        * Vector2.UnitX;
                if (child.RelativeLocation.Y <= 0)
                    this.Location -= (child.RelativeLocation.Y - bufferWidth) * Vector2.UnitY;
                if (child.RelativeLocation.Y + child.ElementSize.Height > this.ElementSize.Height)
                    this.ElementSize +=
                        (child.RelativeLocation.Y + child.ElementSize.Height - this.ElementSize.Height + bufferWidth)
                        * Vector2.UnitY;
            }
        }
        /// <summary>
        /// Get absolute coordinates from the location relative to this element
        /// </summary>
        /// <returns></returns>
        public Vector2 GetAbsoluteLocation(Vector2 relativeLocation)
        {
            return relativeLocation + this.Location;
        }

        /// <summary>
        /// Get the location relative to this element
        /// </summary>
        /// <returns></returns>
        public Vector2 GetRelativeLocation(Vector2 absoluteLocation)
        {
            return absoluteLocation - this.Location;
        }

        public UIEngine UIManager { get; private set; }
        public void BindToGUIEngine(UIEngine engine)
        {
            if (UIManager != null)
                throw new Exception("Already bound to a GUIEngine");
            UIManager = engine;
        }

        public void AddChild(ParentedElement child)
        {
            AddChild(child, true);
        }
        public virtual void AddChild(ParentedElement child, bool makeActive)
        {
            Children.Add(child);
            child.setParent(this);
            this.ActiveChild = child;

            child.Resized += new ElementEventHandler(childResized);
            child.Moved += new ElementEventHandler(childMoved);

            if (makeActive)
                this.ActiveChild = child;
        }

        protected virtual void childMoved(Element sender)
        {
            if (EnableAutomaticResize)
                ResizeToFitChildren(0f);
            else if (EnableAutomaticEnlarge)
                EnlargeToFitChildren(0f);
        }

        protected virtual void childResized(Element sender)
        {
            if (EnableAutomaticResize)
                ResizeToFitChildren(0f);
            else if (EnableAutomaticEnlarge)
                EnlargeToFitChildren(0f);
        }

        /// <summary>
        /// Destroys child as well
        /// </summary>
        public void RemoveChild(ParentedElement child)
        {
            RemoveChild(child, true);
        }

        public virtual void RemoveChild(ParentedElement child, bool destroy)
        {
            if (destroy) child.Destroy();
            child.Resized -= new ElementEventHandler(childResized);
            child.Moved -= new ElementEventHandler(childMoved);
            if (ActiveChild == child)
                ActiveChild = null;
            Children.Remove(child);
        }

        public event MouseEventHandler MouseOver;
        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseExit;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseClick;
        public event MouseEventHandler MouseScroll;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyPressDown;
        public event KeyEventHandler KeyUp;
        public event ElementEventHandler Resized;
        public event ElementEventHandler Moved;

        // Event Firing
        protected virtual void OnResized()
        {
            if (Resized != null)
                Resized(this);
        }
        protected virtual void OnMoved()
        {
            if (Moved != null)
                Moved(this);
        }

        protected virtual void OnMouseOver(MouseEventArgs e)
        {
            if (MouseOver != null)
                MouseOver(this, e);
        }
        protected virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (MouseEnter != null)
                MouseEnter(this, e);
        }
        protected virtual void OnMouseExit(MouseEventArgs e)
        {
            if (MouseExit != null)
                MouseExit(this, e);
        }
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, e);
        }
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            if (MouseClick != null)
                MouseClick(this, e);
        }
        protected virtual void OnMouseScroll(MouseEventArgs e)
        {
            if (MouseScroll != null)
                MouseScroll(this, e);
        }
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (KeyDown != null)
                KeyDown(this, e);
        }
        private void OnKeyPressedDown(KeyEventArgs e)
        {
            if (KeyPressDown != null)
                KeyPressDown(this, e);
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            if (KeyUp != null)
                KeyUp(this, e);
        }
        
        // Helper Functions
        private void handleComputerEvents()
        {

            var mouseState = Mouse.GetState();
            var keyState = Keyboard.GetState();
            var childThis = this as ParentedElement;

            // Mouse Handling
            if (ContainsPoint(mouseState.X, mouseState.Y))
            {
                if (!ContainsPoint(_prevMouseState.X, _prevMouseState.Y))
                    OnMouseEnter(new MouseEventArgs(_prevMouseState, mouseState));
                OnMouseOver(new MouseEventArgs(_prevMouseState, mouseState));

                if (_prevMouseState.ScrollWheelValue != mouseState.ScrollWheelValue)
                    OnMouseScroll(new MouseEventArgs(_prevMouseState, mouseState));

                if (mouseState.LeftButton == ButtonState.Pressed ||
                    mouseState.RightButton == ButtonState.Pressed ||
                    mouseState.MiddleButton == ButtonState.Pressed)
                    OnMouseDown(new MouseEventArgs(_prevMouseState, mouseState));
                else if (isMouseClicked(mouseState))
                    OnMouseClick(new MouseEventArgs(_prevMouseState, mouseState));
            }
            else
            {
                if (childThis != null &&
                    childThis.Parent.ActiveChild == this &&
                    isMouseClicked(mouseState))
                    childThis.Parent.ActiveChild = null;
                if (ContainsPoint(_prevMouseState.X, _prevMouseState.Y))
                    OnMouseExit(new MouseEventArgs(_prevMouseState, mouseState));
            }

            // Keyboard Handling
            if (childThis == null  || childThis.Parent.ActiveChild == this)
            {
                var prevPressedKeys = _prevKeyState.GetPressedKeys();
                var pressedKeys = keyState.GetPressedKeys();
                
                if (pressedKeys.Length > 0)
                    OnKeyDown(new KeyEventArgs(pressedKeys));

                var pushedKeys = pressedKeys
                    .Where<Keys>(k => !prevPressedKeys.Contains<Keys>(k))
                    .ToArray<Keys>();

                if (pushedKeys.Length > 0)
                    OnKeyPressedDown(new KeyEventArgs(pushedKeys));

                var releasedKeys = prevPressedKeys
                    .Where<Keys>(k => !pressedKeys.Contains<Keys>(k))
                    .ToArray<Keys>();

                if (releasedKeys.Length > 0)
                    OnKeyUp(new KeyEventArgs(releasedKeys));
            }

            _prevMouseState = mouseState;
            _prevKeyState = keyState;
        }


        private bool isMouseClicked(MouseState mouseState)
        {
            return (_prevMouseState.LeftButton == ButtonState.Pressed &&
                            mouseState.LeftButton == ButtonState.Released) ||
                    (_prevMouseState.MiddleButton == ButtonState.Pressed &&
                            mouseState.MiddleButton == ButtonState.Released) ||
                    (_prevMouseState.RightButton == ButtonState.Pressed &&
                            mouseState.RightButton == ButtonState.Released);
        }
        
        /// <summary>
        /// For future(?) XBox support
        /// </summary>
        private void handleGamepadEvents()
        {
            throw new NotImplementedException();
        }
        
        public bool ContainsPoint(int x, int y)
        {
            var Dx = x - Location.X;
            var Dy = y - Location.Y;

            return
                Dx > 0 &&
                Dy > 0 &&
                Dx < ElementSize.X &&
                Dy < ElementSize.Y;
        }

        // May need to override //
        public virtual void Update(GameTime time)
        {
            handleComputerEvents();
            Children.ForEach(c => c.Update(time));
        }

        public virtual void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            Children.ForEach(c => c.Draw(time, spriteBatch));
        }
        public virtual void Destroy() { }

        public Element(Vector2 Location, Size size)
        {

            Location = location;
            ElementSize = size;
            Children = new List<ParentedElement>();
            this.EnableAutomaticResize = false;
        }
    }
}

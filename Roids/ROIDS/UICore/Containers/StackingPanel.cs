using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Utilities;
namespace UICore.Containers
{
    /// <summary>
    /// Arranges Controls one after an other
    /// </summary>
    public class StackingPanel : ParentedElement
    {
        public enum DirectionType { Up, Down, Left, Right }

        // Keeps everything in order of arrangement
        private List<ParentedElement> _childStack;

        private DirectionType _stackingDirection;
        public DirectionType StackingDirection 
        { get { return _stackingDirection;} 
            set
            {
                _stackingDirection = value;
                placeChild(_childStack[0]); // replace all 
                ElementSize = new Size();
                EnlargeToFitChildren(0f);
            }
        }

        float _buffering;
        public float Buffering
        {
            get { return _buffering;}
            set
            {
                _buffering = value;
                placeChild(_childStack[0]); // replace all
            }
        }

        public override void  AddChild(ParentedElement child, bool makeActive)
        {
            base.AddChild(child, makeActive);
            _childStack.Add(child);
            placeChild(child);
        }

        public override void RemoveChild(ParentedElement child, bool destroy)
        {
            int index = _childStack.FindIndex(x => x == child);
            _childStack.Remove(child);
            placeChild(_childStack[index]);
            base.RemoveChild(child, destroy);
        }

        public void InsertChild(ParentedElement child, int index, bool makeActive)
        {
            _childStack.Insert(index, child);
            placeChild(child);
            base.AddChild(child, makeActive);
        }


        public StackingPanel(Vector2 location, Size size, DirectionType stackingDirection, float buffering, Element parent)
            : base(location, size, parent)
        {
            this._childStack = new List<ParentedElement>();
            _stackingDirection = stackingDirection;
            _buffering = buffering;
            this.EnableAutomaticEnlarge = true;
            
        }


        protected override void childMoved(Element sender)
        {
            placeChild(_childStack[0]);
        }
        protected override void childResized(Element sender)
        {
            placeChild((ParentedElement)sender);
            base.childResized(sender);
        }

        /// <summary>
        /// Arranges Panel Properly
        /// </summary>
        /// <param name="child"></param>
        private void placeChild(ParentedElement child)
        {
            int index = _childStack.FindIndex(x => x == child);
            
            if (index == 0)
            {
                child.RelativeLocation = Vector2.Zero;
                index++;
            }

            Action<Func<ParentedElement, Vector2>> place = 
                func =>
                {
                    for (int i = index; i < _childStack.Count; i++)
                    {
                        var prev = _childStack[i - 1];
                        var cur = _childStack[i];

                        cur.SilentlyMove(func(prev));
                    }
                };

            switch (StackingDirection)
            {
                case DirectionType.Down:
                    place(prev => 
                        prev.Location + (prev.ElementSize.Y + Buffering) * Vector2.UnitY);
                    break;
                case DirectionType.Left:
                    place(prev =>
                        prev.Location + (prev.Location.X + prev.ElementSize.X + Buffering) * Vector2.UnitX);
                    break;
                case DirectionType.Up:
                    place(prev =>
                            prev.Location - (prev.Location.Y + prev.ElementSize.Y + Buffering) * Vector2.UnitY);
                    break;
                case DirectionType.Right:
                    place(prev =>
                            prev.Location - (prev.Location.X + prev.ElementSize.X + Buffering) * Vector2.UnitX);
                    break;
            }
            EnlargeToFitChildren(0f);
        }
    }
}

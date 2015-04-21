using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldCore
{
    public delegate void WorldObjectEventHandler(WorldObject obj);
    
    abstract public class WorldObject : Utilities.ISpatialEntity, IDrawable
    {
        public virtual float Rotation
        {
            get
            {
                return _orientation;
            }
            set
            {
                bool fire = (_orientation != value);
                _orientation = value;
                if (fire && PositionChanged != null)
                    PositionChanged(this);
            }
        }
        private float _orientation;

        public virtual Vector2 Position
        { 
            get
            {
                return _position;
            }
            set
            {
                bool fire = (_position != value);
                _position = value;
                if (fire && PositionChanged != null)
                    PositionChanged(this);
            }
        }
        protected Vector2 _position;
        
        /// <summary>
        /// Sets position without firing PositionChanged event
        /// </summary>
        public void Shift(Vector2 position, float orientation)
        {
            _position = position;
            _orientation = orientation;
        }

        public event Utilities.SpatialStateChangeHandler PositionChanged;
        public event WorldObjectEventHandler Destroyed;

        protected bool isDestroyed = false;
        public virtual void Destroy() 
        {
            if (Destroyed != null && !isDestroyed)
                Destroyed(this);
            isDestroyed = true;
        }

        public WorldObject(Vector2 position, float orientation)
        {
            Rotation = orientation;
            Position = position;
        }

        public abstract void Draw(GameTime time, SpriteBatch spriteBatch);
    }
}

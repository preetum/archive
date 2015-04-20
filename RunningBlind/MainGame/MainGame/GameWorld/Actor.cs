using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace MainGame.GameWorld
{
    delegate void CollisionEventHandler(Actor sender, Actor collidedInto);
    abstract class Actor
    {
        public float X { get { return Position.X; } protected set { Position = Vector2.UnitY * Position.Y + Vector2.UnitX * value; } }
        public float Y { get { return Position.Y; } protected set { Position = Vector2.UnitY * value + Vector2.UnitX * Position.X; } }
        public Vector2 Position { get; set; }
        public float Theta { get; set; }
        public abstract void Update(GameTime time);
        public virtual Rectangle BoundingBox
        {
            get 
            {
                return new Rectangle((int)this.X - this.Image.Width / 2, (int)this.Y - this.Image.Height / 2, this.Image.Width, this.Image.Height);
            }
        }
        public int CollisionClass { get; set; }

        public event CollisionEventHandler Collided;
        protected Texture2D Image;

        private Level _parentLevel;
        public Level ParentLevel
        {
            get { return _parentLevel; }
            set
            {
                if (_parentLevel == null) _parentLevel = value;
                else throw new Exception("Parent level can only added once");
            }
        }



        public bool IsDead { get; set; }
        public virtual void Fired() { IsDead = true;  }
    }
}

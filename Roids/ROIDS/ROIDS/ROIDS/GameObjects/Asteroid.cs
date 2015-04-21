using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PhysicsCore;
using WorldCore;
using Utilities;

namespace ROIDS.GameObjects
{
    abstract class Asteroid : DeviceCarrierActor, ICircleBody, IDynamicObject, IHealthable
    {
        public float Radius { get; set; }
        private float _maxHealth;
        public float MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                _maxHealth = value;
                CurrentHealth = _maxHealth;
            }
        }
        public float CurrentHealth { get; set; }
        public Color Color { get; set; }
        public const float MaxRadius = 20;
        public override Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }

        public Asteroid(Vector2 position, float radius)
            :this(position, radius, 0f)
        {
        }

        public Asteroid(Vector2 position, float radius, Color c)
            : this(position, radius, 0f)
        {
            Color = c;
        }

        public Asteroid(Vector2 position,  float radius, float orientation)
            :this(position, radius, orientation, Vector2.Zero, Color.Gray)
        {        
        }

        public Asteroid(Vector2 position, float radius, float orientation, Vector2 velocity)
            :this(position, radius, orientation, velocity, Color.Gray)
        {
        }

        public Asteroid(Vector2 position, float radius, float orientation, Vector2 velocity, Color c)
            :base(position, orientation)
        {
            DragCoefficient = 0f * RigidBodyHelper.DefaultDragCoeff;
            Radius = radius;
            Color = c;
            this.Mass = 5;
            Restitution = 1f;
            MaxHealth = radius * radius / 10;

        }

        public virtual void Hurt(float damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
                this.Destroy();
        }

        public bool DebugMode;
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsUtils.DrawBall(Position, Radius, 
                DebugMode ? Color:Color.Gray, 255);
        }

        public abstract void Update(GameTime gameTime);
    }
}

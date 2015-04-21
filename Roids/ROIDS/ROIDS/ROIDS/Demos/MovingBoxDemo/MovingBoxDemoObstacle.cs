using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicsCore;

namespace ROIDS.MovingBoxDemo
{
    class MovingBoxDemoObstacle : GameObjects.Actor, PhysicsCore.ICircleBody, IMovingBoxDemoObject
    {
        public MovingBoxDemoObstacle(string id, Vector2 position) 
            : base(position, 0) 
        {
            HasInfiniteMass = true;
            Radius = 10;
        }

        public float Radius { get; set; }
        public CollisionEventHandler Collided { get; set; }

        public override PhysicsCore.Region BoundingBox
        {
            get { return PhysicsCore.Region.FromCircle(Position, Radius); }
        }
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(
                (Texture2D)Utilities.ContentRepository.Repository["Ball"],
                new Rectangle((int)(Position.X-Radius), (int)(Position.Y-Radius), 2*(int)Radius, 2*(int)Radius), Color.Green);
        }

        public bool HasInfiniteMass
        {
            get
            {
                return base.HasInfiniteMass;
            }
            set
            {
                base.HasInfiniteMass = value;
            }
        }
    }
}

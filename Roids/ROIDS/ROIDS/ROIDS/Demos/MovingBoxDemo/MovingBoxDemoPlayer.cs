using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PhysicsCore;
namespace ROIDS.MovingBoxDemo
{
    class MovingBoxDemoPlayer : ROIDS.GameObjects.Actor, ICircleBody, IMovingBoxDemoObject
    {
        public float Radius { get { return 10; } set { } }
        public MovingBoxDemoPlayer(string id, Vector2 position, float orientation) 
            : base(position, orientation) 
        {  }
        public override PhysicsCore.Region BoundingBox
        {
            get 
            {
                return Region.FromCircle(Position, Radius);
            }
        }
        public CollisionEventHandler Collided { get; set; }
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                (Texture2D)Utilities.ContentRepository.Repository["Ball"],
                new Rectangle((int)(Position.X - Radius), (int)(Position.Y-Radius), 2 * (int)Radius, 2 * (int)Radius), Color.Red);
        }

        public int Speed { get { return 2; }  }
    }
}

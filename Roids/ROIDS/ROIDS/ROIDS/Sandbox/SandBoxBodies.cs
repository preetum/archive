using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PhysicsCore;
using Microsoft.Xna.Framework;
namespace ROIDS.Sandbox
{
    class CircleBody : GameObjects.Actor, ICircleBody
    {
        public float Radius { get;  set; }

        public override Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }
        public Region Span { get { return BoundingBox; } }

        public CircleBody(float radius, float mass, Vector2 position)
            : base(position, 0)
        {
            Mass = mass;
            Radius = radius;
        }

        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            // Invisible
        }
    }

    class VertWallBody : GameObjects.Actor, IVertWallBody
    {
        

        public float Length { get; private set; }

        public override Region BoundingBox
        {
            get
            {
                return new Region(Position.X - 30,
                    Position.X + 30,
                    Position.Y - Length / 2,
                    Position.Y + Length / 2);
            }
        }

        public VertWallBody(Vector2 position, float length)
            : base(position, 0)
        {
            Mass = 1;
            HasInfiniteMass = true;
            Length = length;
        }

        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            // Invisible
        }
    }

    class HorizWallBody : GameObjects.Actor, IHorizWallBody
    {
        public float Length { get; private set; }

        public override Region BoundingBox
        {
            get
            {
                return new Region(Position.X - Length / 2,
                    Position.X + Length / 2,
                    Position.Y - 30,
                    Position.Y + 30);
            }
        }

        public HorizWallBody(Vector2 position, float length)
            : base(position, 0)
        {
            Mass = 1;
            HasInfiniteMass = true;

            Length = length;
        }

        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            // Invisible
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using PhysicsCore;
using WorldCore;
using Utilities;

namespace ROIDS.GameObjects
{
    class Platform : Actor, ICircleBody
    {
        public float Radius { get; set; }

        public override Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }

        public Platform(Vector2 position, float orientation)
            : base(position, orientation)
        { }

        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }
    }
}

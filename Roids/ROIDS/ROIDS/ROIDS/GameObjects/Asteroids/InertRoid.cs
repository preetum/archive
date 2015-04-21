using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PhysicsCore;
using WorldCore;
using Utilities;

namespace ROIDS.GameObjects.Asteroids
{
    class InertRoid : Asteroid
    {
        public InertRoid(Vector2 position, float radius)
            : base(position, radius)
        {
        }

        public override void Update(GameTime gameTime)
        {
            var x = (float)Math.Pow((2 / Math.PI * Math.Atan(this.Velocity.Length() / 2)), 2);
            this.Color = MathUtils.ColorLerp(Color.Gray, Color.White, x);
        }
    }
}

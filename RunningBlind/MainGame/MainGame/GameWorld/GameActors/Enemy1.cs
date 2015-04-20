using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainGame.GameWorld.GameActors
{
    class Enemy1 : Actor, IDrawable
    {
        EnemyAI AI;
        public override void Fired()
        {
            this.IsDead = true;
        }
        public Enemy1(Vector2 position, Waypoint rootWaypoint)
        {
            this.Position = position;
            this.Theta = 0f;
            this.Image = ResourceManager.Resources["Guard"];

            AI = new EnemyAI(this, rootWaypoint);
        }

        void lookAt(Vector2 target)
        {
            this.Theta = (float)Math.Atan2(target.Y - this.Position.Y, target.X - this.Position.X);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime time)
        {
            AI.Update(time);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.Image, this.Position, null, Color.White, this.Theta, new Vector2(this.Image.Width / 2, this.Image.Height / 2), Vector2.One, SpriteEffects.None, 0.1f);
        }
    }
}

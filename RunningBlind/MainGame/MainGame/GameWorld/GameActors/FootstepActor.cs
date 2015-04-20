using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainGame.GameWorld.GameActors
{
    class FootstepActor : Actor, IDrawable
    {
        
        public FootstepActor(Vector2 postion, float orientation)
        {
            this.Image = ResourceManager.Resources["dots"];
            this.Theta = orientation;
            this.Position = postion;

            CollisionClass = -1;
        }
        public override void Update(Microsoft.Xna.Framework.GameTime time)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.Image, this.Position, null, Color.White, this.Theta, new Vector2(this.Image.Width / 2, this.Image.Height / 2), Vector2.One, SpriteEffects.None, 0.1f);
        }
    }
}

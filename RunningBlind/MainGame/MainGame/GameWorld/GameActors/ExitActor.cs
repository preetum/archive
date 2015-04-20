using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace MainGame.GameWorld.GameActors
{
    class ExitActor : Actor, IDrawable
    {
        public ExitActor(Vector2 postion)
        {
            this.Image = ResourceManager.Resources["Goal"];
            this.Position = postion;
        }
        public override void Update(Microsoft.Xna.Framework.GameTime time)
        {
        }
        public override void Fired()
        {
            SoundManager.Sonar();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.Image, this.Position, Color.White);
        }

    }
}

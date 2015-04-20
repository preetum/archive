using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainGame.GameWorld
{
    delegate void GraphicOverlayEventHandler(GraphicOverlay sender);
    abstract class GraphicOverlay
    {
        protected Texture2D texture;
        public event GraphicOverlayEventHandler GraphicCompleted;
        private bool _completed;
        public bool Completed
        {
            get { return _completed; }
            protected set
            {
                _completed = value;
                if (value && GraphicCompleted != null)
                    GraphicCompleted(this);
            }
        }

        public abstract void Update(GameTime time);
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, this.Position, null, Color.White, this.Theta, new Vector2(this.texture.Width / 2, this.texture.Height / 2), Vector2.One, SpriteEffects.None, 0.1f);

        }


        public Vector2 Position { get; protected set; }
        public float Theta { get; protected set; }
    }
}

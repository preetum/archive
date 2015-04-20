using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Omron.Framework
{
    public abstract class GameState
    {
        public bool IsOpaque = true;

        public GraphicsDevice GraphicsDevice { get { return ResourceManager.GraphicsDevice; } }
        public ContentManager ContentManager { get { return ResourceManager.ContentManager; } }

        public GameEngine GameEngine;


        public abstract void Init();

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldCore
{
    public interface IDrawable
    {
        void Draw(GameTime time, SpriteBatch spriteBatch);
    }
}

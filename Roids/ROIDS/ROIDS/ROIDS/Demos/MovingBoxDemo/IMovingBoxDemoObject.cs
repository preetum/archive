using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROIDS.MovingBoxDemo
{
    interface IMovingBoxDemoObject
    {
        void Draw(Microsoft.Xna.Framework.GameTime time,
            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using GameCore;
using UICore;
namespace ROIDS.GameStates
{
    class BlueState : GameState
    {
        UIEngine _eng;

        public override void Load()
        {
            _eng = new UIEngine();
            _eng.AddAndLoad(new UIFrames.BasicFrame());
        }
        public override void Update(GameTime time)
        {
            if (!_eng.Update(time))
                this.Exit();
        }
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            _eng.Draw(time, spriteBatch);
        }
    }
}

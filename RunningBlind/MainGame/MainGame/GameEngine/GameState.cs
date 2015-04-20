using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace MainGame.GameEngine
{
    abstract class GameState
    {
        GameStateManager _gsManager;
        public GameStateManager GameStateManager
        {
            get { return _gsManager; }
            set
            {
                if (_gsManager == null)
                    _gsManager = value;
                else throw new Exception("Cannot Add Game Manager Twice");
            }
        }

        public GameState(GameStateManager gsManager)
        {
            _gsManager = gsManager;
        }
        public abstract void Update(GameTime time);
        public abstract void Draw(GameTime time, SpriteBatch spriteBatch);
        public virtual void Destroy() { }

        public void Exit()
        {
            if (_gsManager.ActiveState == this) _gsManager.CloseState();
        }

    }
}

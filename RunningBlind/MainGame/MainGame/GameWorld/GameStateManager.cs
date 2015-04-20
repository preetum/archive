using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainGame.GameEngine
{
    class GameStateManager
    {
        Stack<GameState> _gameStates;
        public XnaGame CurrentGame { get; private set; }
        public bool IsActive { get { return _gameStates.Count > 0; } }
        public GameState ActiveState 
        { 
            get
            {
                if (this.IsActive) return _gameStates.Peek();
                return null;
            } 
        }
        public GameStateManager(XnaGame xnaGame)
        {
            CurrentGame = xnaGame;
            _gameStates = new Stack<GameState>();
            
        }

        public void OpenState(GameState state)
        {
            _gameStates.Push(state);
        }

        public void CloseState(int count)
        {
            for (int i = 0;i < count; i++)
                _gameStates.Pop();
        }
        public void CloseState() { this.CloseState(1); }

        public void CloseAll()
        {
            _gameStates.Clear();
        }

        public void Update(GameTime time)
        {
            if (ActiveState != null)
                ActiveState.Update(time);
        }

        public void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (ActiveState != null)
                ActiveState.Draw(time, spriteBatch);
        }
    }
}

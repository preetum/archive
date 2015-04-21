using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameCore
{
    public class GameEngine
    {

        List<GameState> _gameStates;

        public GameState ActiveState
        {
            get
            {
                return (_gameStates.Count > 0) ? _gameStates[_gameStates.Count - 1] : null;
                
            }
        }

        /// <summary>
        /// Return gamestate when pred returns true
        /// </summary>
        /// <param name="pred"></param>
        /// <returns></returns>
        public GameState FindGameState(Predicate<GameState> pred)
        {
            GameState active = null;
            for (int i = _gameStates.Count - 1; i >= 0; i--)
                if (pred(_gameStates[i]))
                {
                    active = _gameStates[i];
                    break;
                }
            return active;
        }

        public GameState TopState
        {
            get
            {
                return _gameStates[_gameStates.Count - 1];
            }
        }
        private static GameEngine _singleton;
        public static GameEngine Singleton
        {
            get
            {
                if (_singleton == null)
                    _singleton = new GameEngine();
                return _singleton;
            }
        }
        private GameEngine()
        {
            _gameStates = new List<GameState>();
        }

        
        public void AddState(GameState state)
        {
            _gameStates.Add(state);
        }
        public void AddAndLoad(GameState state)
        {
            _gameStates.Add(state);
            state.Load();
        }


        public bool Update(GameTime time)
        {
            while (ActiveState != null && ActiveState.isDead)
            {

                ActiveState.Destroy();
                _gameStates.Remove(ActiveState);
            }

            if (_gameStates.Count > 0)
                ActiveState.Update(time);
            else return false;
            return true;
        }
        public bool Draw(GameTime time, SpriteBatch spriteBatch)
        {

            if (_gameStates.Count > 0)
            {
                var startDrawingAt = _gameStates.Count - 1;
                for (int i = startDrawingAt; i >= 0; i--)
                {
                    if (!_gameStates[i].Transparent)
                    {
                        startDrawingAt = i;
                        break;
                    }
                }
                for (int i = startDrawingAt; i < _gameStates.Count; i++)
                    _gameStates[i].Draw(time, spriteBatch);
            }
            else
                return false;
            return true;
        }

        public void DestroyAll()
        {
            foreach (GameState state in _gameStates)
                state.Exit();
        }
    }
}

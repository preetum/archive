using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Omron.Framework
{
    public class GameEngine
    {
        Stack<GameState> gameStates;

        public GameEngine()
        {
            gameStates = new Stack<GameState>();
        }
        public void PushState(GameState state)
        {
            state.GameEngine = this;
            state.Init();
            gameStates.Push(state);
        }
        public GameState PopState()
        {
            return gameStates.Pop();
        }
        public GameState GetActiveState()
        {
            return gameStates.Peek();
        }
        public void Update(GameTime gameTime)
        {
            var activeState = this.GetActiveState();
            if (activeState != null)
                activeState.Update(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            var states = gameStates.ToArray(); //index 0 = most recently pushed

            int i_max = 0;
            while (!states[i_max].IsOpaque && i_max < states.Length - 1)
                i_max++;

            for (int i = i_max; i >= 0; i--)
                states[i].Draw(gameTime);
        }
        public void EndGame()
        {
            ResourceManager.BaseGame.Exit();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MainGame.GameWorld;
using MainGame.GameWorld.Levels;
using MainGame.UIFrame;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace MainGame.GameEngine.GameStates
{
    class BasicDemo : GameState
    {
        Stack<Level> gameLevels;
        Level currentLevel;
        GameController controller;

        Vector2 lastStepPulse;

        public BasicDemo(GameStateManager gsManager) : base(gsManager)
        {
            controller = new GameController();
            controller.Pause += new GameControlEventHandler(controller_Pause);
            gameLevels = new Stack<Level>();

            //gameLevels.Push(new Level4());
            gameLevels.Push(new Level3());
            gameLevels.Push(new Level2());
            gameLevels.Push(new Level1());
            gameLevels.Push(new TitleLevel());

            loadLevel(gameLevels.Peek());
        }

        void controller_Pause()
        {
            this.Exit();
        }

        void loadLevel(Level level)
        {
            currentLevel = level;
            currentLevel.LoadLevel();
            currentLevel.Player.connectController(controller);
            currentLevel.LevelEnded += new LevelEventHandler(LevelEnded);
            currentLevel.LevelRestart += new LevelEventHandler(currentLevel_LevelRestart);
            //currentLevel.pulseMan.StartPulse(Vector2.One * 500, 300f);

            lastStepPulse = currentLevel.Player.Position;
        }

        void currentLevel_LevelRestart()
        {
            loadLevel((Level)currentLevel.GetType().GetConstructor(new Type[] { }).Invoke(new object[] {}));
        }

        void LevelEnded()
        {
            if (gameLevels.Count == 1)
            {
                this.Exit();
            }
            else
            {
                gameLevels.Pop();
                loadLevel(gameLevels.Peek());
            }
        }

        public override void Draw(GameTime gtime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            currentLevel.Draw(gtime, spriteBatch);

            float barLevel = currentLevel.Player.GetRecharge();
            float yBase = 600;
            float barHeight = 100;


            if (barLevel != 1f)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(ResourceManager.Resources["pixel"], new Rectangle(50, (int)(yBase - barHeight * barLevel), 20, (int)(barHeight * barLevel)), Color.White);
                spriteBatch.End();
            }
        }


        
        public override void Update(Microsoft.Xna.Framework.GameTime time)
        {
            controller.HandleKeyboardEvents();
            controller.HandleMouseEvents();
            currentLevel.Update(time);

            if ((currentLevel.Player.Position - lastStepPulse).Length() > 50)
            {
                //currentLevel.pulseMan.StartPulse(currentLevel.Player.Position, 100f);
                lastStepPulse = currentLevel.Player.Position;
            }


        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using Omron.Framework;

namespace Omron.GameStates
{
    public class MainMenu : GameState
    {
        UIManager UIMan;

        SpriteBatch spriteBatch;

        Vector2 cent;

        public MainMenu()
        {
            UIMan = new UIManager();
        }
        public override void Init()
        {
            ResourceManager.BaseGame.IsMouseVisible = true;

            spriteBatch = new SpriteBatch(this.GraphicsDevice);

            cent = new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth / 2, GraphicsDevice.PresentationParameters.BackBufferHeight / 2);

            //
            //UI
            //

            UIMan.KeyDown += new KeyPressEventHandler(UIMan_KeyDown);

            SolidButton b1 = new SolidButton(new Vector2(100, 100), 300, 100);
            b1.Text = "map test state";
            b1.MouseLeftDown += new MouseClickEventHandler(b1_MouseLeftDown);

            SolidButton b2 = new SolidButton(new Vector2(100, 250), 300, 100);
            b2.Text = "test LOS state";
            b2.MouseLeftDown += new MouseClickEventHandler(b2_MouseLeftDown);

            SolidButton mainBut = new SolidButton(new Vector2(100, 400), 330, 100);
            mainBut.Text = "god mode";
            mainBut.ButtonPressed += new ButtonPressedEventHandler(mainBut_ButtonPressed);

            SolidButton sBut = new SolidButton(new Vector2(100, 550), 100, 100);
            sBut.Text = "server";
            sBut.MouseLeftDown += new MouseClickEventHandler(sBut_MouseLeftDown);

            SolidButton cBut = new SolidButton(new Vector2(250, 550), 100, 100);
            cBut.Text = "lobby";
            cBut.MouseLeftDown += new MouseClickEventHandler(cBut_MouseLeftDown);


            UIMan.AddControl(b2);
            UIMan.AddControl(mainBut);
            UIMan.AddControl(b1);
            UIMan.AddControl(sBut);
            UIMan.AddControl(cBut);
        }

        void UIMan_KeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.End:
                    GameEngine.EndGame();
                    break;
            }
        }

        void cBut_MouseLeftDown(Vector2 mPos)
        {
            GameEngine.PushState(new LobbyGameState());
        }

        void sBut_MouseLeftDown(Vector2 mPos)
        {
            GameEngine.PushState(new ServerGameState());
        }

        void b1_MouseLeftDown(Vector2 mPos)
        {
            GameEngine.PushState(new MapTestState());
        }

        void mainBut_ButtonPressed(SolidButton sender)
        {
            GameEngine.PushState(new OmronMainStageV1());
        }


        void b2_MouseLeftDown(Vector2 mPos)
        {
            GameEngine.PushState(new TestLOState());
        }

        public override void Update(GameTime gameTime)
        {
            ResourceManager.BaseGame.IsMouseVisible = true;
            UIMan.Update();
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            UIMan.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}

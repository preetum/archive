using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MainGame.UIFrame
{
    public delegate void GameControlEventHandler();
    public delegate void GameSpacialEventHandler(Vector2 postion);
    class GameController
    {
        // Game Play Controls
        public event GameControlEventHandler MoveForward;
        public event GameControlEventHandler MoveBackward;
        public event GameControlEventHandler MoveRight;
        public event GameControlEventHandler MoveLeft;

        public event GameSpacialEventHandler FireGun;
        public event GameSpacialEventHandler AimGun;

        public event GameControlEventHandler Rest;
        // Game System Controls
        public event GameControlEventHandler Pause;

        private void fireEvent(GameControlEventHandler e)
        {
            if (e != null) e();
        }

        MouseState _prevousMouseState;
        public void HandleMouseEvents()
        {
            var mouseState = Mouse.GetState();
            if (AimGun != null) // && mouseState.X != _prevousMouseState.X && mouseState.Y != _prevousMouseState.Y
                AimGun(new Vector2(mouseState.X, mouseState.Y));
            if (FireGun != null && mouseState.LeftButton == ButtonState.Pressed &&
                (_prevousMouseState == null || _prevousMouseState.LeftButton == ButtonState.Released))
                FireGun(new Vector2(mouseState.X, mouseState.Y));
            _prevousMouseState = mouseState;
        }
        public void HandleKeyboardEvents()
        {

            KeyboardState keys = Keyboard.GetState();
            var pressed = keys.GetPressedKeys();
            foreach (Keys k in pressed)
            {
                switch (k)
                {
                    case Keys.W: fireEvent(MoveForward);  break;
                    case Keys.A: fireEvent(MoveLeft); break;
                    case Keys.S: fireEvent(MoveBackward); break;
                    case Keys.D: fireEvent(MoveRight); break;
                    case Keys.Escape: fireEvent(Rest); fireEvent(Pause); break;
                    default: fireEvent(Rest); break;
                }
            }
            if (pressed.Length == 0) fireEvent(Rest);
        }
    }
}

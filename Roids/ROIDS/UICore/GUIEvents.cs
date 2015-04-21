using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace UICore
{
    public delegate void MouseEventHandler(Element sender, MouseEventArgs e);
    public delegate void KeyEventHandler(Element sender, KeyEventArgs e);
    public delegate void ElementEventHandler(Element sender);

    public enum MouseButtons
    {
        Left,
        Right,
        Middle
    }

    public class MouseEventArgs : EventArgs
    {
        public MouseState PreviousMouseState { get; private set; }
        public MouseState CurrentMouseState { get; private set; }

        public Vector2 CurrentPosition
        {
            get { return new Vector2(CurrentMouseState.X, CurrentMouseState.Y); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button">the mouse button (left,right,center)</param>
        /// <returns></returns>
        public bool isClicked(MouseButtons b)
        {
            switch (b)
            {
                case MouseButtons.Left: return PreviousMouseState.LeftButton == ButtonState.Pressed &&
                    CurrentMouseState.LeftButton == ButtonState.Released;
                case MouseButtons.Right: return PreviousMouseState.RightButton == ButtonState.Pressed &&
                    CurrentMouseState.RightButton == ButtonState.Released;
                case MouseButtons.Middle: return PreviousMouseState.MiddleButton == ButtonState.Pressed &&
                    CurrentMouseState.MiddleButton == ButtonState.Released;
                default:
                    throw new Exception("Unrecognized Button");
            }
        }

        public bool isPressed(MouseButtons b)
        {
            switch (b)
            {
                case MouseButtons.Left: return PreviousMouseState.LeftButton == ButtonState.Released &&
                    CurrentMouseState.LeftButton == ButtonState.Pressed;
                case MouseButtons.Right: return PreviousMouseState.RightButton == ButtonState.Released &&
                    CurrentMouseState.RightButton == ButtonState.Pressed;
                case MouseButtons.Middle: return PreviousMouseState.MiddleButton == ButtonState.Released &&
                    CurrentMouseState.MiddleButton == ButtonState.Pressed;
                default:
                    throw new Exception("Unrecognized Button");
            }
        }

        public bool isDown(MouseButtons b)
        {
            switch (b)
            {
                case MouseButtons.Left: return CurrentMouseState.LeftButton == ButtonState.Pressed;
                case MouseButtons.Right: return CurrentMouseState.RightButton == ButtonState.Pressed;
                case MouseButtons.Middle: return CurrentMouseState.MiddleButton == ButtonState.Pressed;
                default:
                    throw new Exception("Unrecognized Button");
            }
        }

        public MouseEventArgs(MouseState previousMouseState, MouseState currentMouseState)
            : base()
        {
            CurrentMouseState = currentMouseState;
            PreviousMouseState = previousMouseState;
        }

    }

    public class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// The keys that are applicable to the event
        /// </summary>
        public Keys[] InterestingKeys { get; private set; }
        public KeyEventArgs(Keys[] interestinKeys)
            : base()
        {
            InterestingKeys = interestinKeys;
        }
        
        /// <summary>
        /// Only Supports AlphaNumeric Keys or Basic Punctuation. 
        /// Otherwise Returns "NotSupported"
        /// </summary>
        public static string KeyToString(Keys key)
        {
            return KeyToString(key, false);
        }

        /// <summary>
        /// Only Supports AlphaNumeric Keys or Basic Punctuation. 
        /// Otherwise Returns "NotSupported"
        /// </summary>
        /// <param name="shift">if shift key is pressed</param>
        public static string KeyToString(Keys key, bool shift)
        {
            var keyType = GetKeyType(key);
            var name = Enum.GetName(typeof(Keys), key);
            switch (keyType)
            {
                case KeyType.Letter:
                    if (shift)
                        return name;
                    else return name.ToLower();
                case KeyType.Number:
                    if (shift)
                    {
                        switch (key)
                        {
                            case Keys.D1:
                                return "!";
                            // Add more if you wish
                        }
                    }
                    else if (name[0] == 'D')
                        return name[1].ToString();
                    else return name[6].ToString();
                    break;
                case KeyType.Symbol:
                    switch (key)
                    {
                        case Keys.OemComma:
                            if (!shift) return ",";
                            break;
                        case Keys.OemPeriod:
                            if (!shift) return ".";
                            break;
                        case Keys.OemQuestion:
                            return (shift) ? "/" : "?";
                        case Keys.OemSemicolon:
                            return (shift) ? ":" : ";";
                        case Keys.OemQuotes:
                            return (shift) ? "\"" : "'";
                        case Keys.OemPipe:
                            return (shift) ? "\\" : "|";
                        // Add more if you wish
                    }
                    break;
            }
            return "NotSupported";
        }
        public enum KeyType { Letter, Number, Symbol, Special }
        public static KeyType GetKeyType(Keys key)
        {
            string name = Enum.GetName(typeof(Keys), key);
            if (name.Length == 1 && Char.IsLetter(name[0]))
                return KeyType.Letter;
            else if (name.StartsWith("D") &&
                Char.IsNumber(name[1]))
                return KeyType.Number;
            else if (name.StartsWith("NumPad") &&
                Char.IsNumber(name[6]))
                return KeyType.Number;
            else if (name.StartsWith("Oem"))
                return KeyType.Symbol;
            else return KeyType.Special;
        }
    }
}

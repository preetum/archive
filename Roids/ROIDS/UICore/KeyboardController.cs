using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
using Utilities;
namespace UICore
{
    public delegate void KeyControlEventHandler(int control);
    public class KeyboardController
    {
        public Dictionary<Keys, int> KeyMapping;


        public event KeyControlEventHandler KeyControlEngaged;
        public event KeyControlEventHandler KeyControlJustEngaged;
        public event KeyControlEventHandler KeyControlDisengaged;

        private void Fire(KeyControlEventHandler controlEvent, int control)
        {
            if (controlEvent != null)
                controlEvent(control);
        }

        public KeyboardController(Element input, Dictionary<Keys, int> keyMapping)
        {
            input.KeyDown += new KeyEventHandler(input_KeyDown);
            input.KeyPressDown += new KeyEventHandler(input_KeyPressDown);
            input.KeyUp += new KeyEventHandler(input_KeyUp);
            KeyMapping = keyMapping;
        }

        void input_KeyPressDown(Element sender, KeyEventArgs e)
        {
            foreach (Keys key in e.InterestingKeys)
                if (KeyMapping.ContainsKey(key))
                {
                    Fire(KeyControlJustEngaged, KeyMapping[key]);
                }
                
        }

        void input_KeyDown(Element sender, KeyEventArgs e)
        {
            foreach (Keys key in e.InterestingKeys)
                if (KeyMapping.ContainsKey(key))
                    Fire(KeyControlEngaged, KeyMapping[key]);
        }
        void input_KeyUp(Element sender, KeyEventArgs e)
        {
            foreach (Keys key in e.InterestingKeys)
                if (KeyMapping.ContainsKey(key))
                {
                    Fire(KeyControlDisengaged, KeyMapping[key]);
                }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework.Input;
using UICore;

namespace ROIDS.Demos
{
    class DemoLoaderFrame : UIFrame
    {
        public override void Load()
        {
            this.KeyUp += new KeyEventHandler(DemoLoaderFrame_KeyUp);
            this.KeyDown += new KeyEventHandler(DemoLoaderFrame_KeyDown);
        }

        void DemoLoaderFrame_KeyDown(GUIElement sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        void DemoLoaderFrame_KeyUp(GUIElement sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

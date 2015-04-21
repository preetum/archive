using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
using UICore;

namespace ROIDS.UIFrames
{
    class BasicFrame : Frame
    {
        public override void Load()
        {
            this.KeyUp += new KeyEventHandler(BlueFrame_KeyUp);
            base.Load();
        }

        void BlueFrame_KeyUp(Element sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Escape))
                this.Close();
        }
    }
}

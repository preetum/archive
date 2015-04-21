using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Utilities;
using GameCore;

using UICore;
using UICore.Containers;
using UICore.Controls;
namespace ROIDS.Demos
{
    [Demo]
    class GUIDemo : GameState
    {
        UIEngine engine;
        public override void Load()
        {
            engine = new UIEngine();

            var frame = new Frame();

            var panel = new StackingPanel(
                Vector2.Zero,
                new Size(),
                StackingPanel.DirectionType.Down,
                0f, frame);

            var txtBox = new AlphaNumTextBox(Vector2.Zero, 200, panel);
            //var optBox = new OptionBox(Vector2.Zero, "Check the Box", panel);

            var lin = new Link(Vector2.Zero, "Click Me", panel);
            lin.MouseClick += new MouseEventHandler(
                (sender, e) => this.Exit());

            for (int i = 0; i < 10; i++)
                new Label(Vector2.Zero, "Look At Me!", panel);

            engine.AddAndLoad(frame);
        }

        public override void Update(GameTime time)
        {
            if (!engine.Update(time))
                this.Exit();
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            engine.Draw(time, spriteBatch);
        }
    }
}

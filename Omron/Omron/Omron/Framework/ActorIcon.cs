using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omron.Framework
{
    class ActorIcon : ImageButton
    {
        public ActorIcon(int xLoc, int yLoc, int w, int h, Actor actor)
            : base(new Vector2(xLoc, yLoc), w, h, actor.GetActiveAnimation().GetCurrentFrame().Image)
        {
            Actor = actor;
            int barHeight = h / 5;
            hpBar = new Bar(new Vector2(xLoc, yLoc) + new Vector2(0, h - barHeight), w, barHeight);
            hpBar.BackColor = Color.Red;
            hpBar.ForeColor = Color.Green;
        }

        public Actor Actor;
        Bar hpBar;

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            tex = Actor.GetActiveAnimation().GetCurrentFrame().Image;
            base.Draw(spriteBatch);
            hpBar.Value = Actor.HealthRatio;
            hpBar.Draw(spriteBatch);
        }
    }
}

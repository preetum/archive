using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omron.Framework
{
    class ImageButton : SolidButton
    {
        public ImageButton(Vector2 minPos, float wid, float hei, Texture2D img)
            : base(minPos, wid, hei)
        {
            tex = img;
            TextColor = Color.LightBlue;
        }

        public ImageButton(Vector2 minPos, float wid, float hei, string fnt, Texture2D img)
            : base(minPos, wid, hei, fnt)
        {
            tex = img;
            TextColor = Color.LightBlue;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ResourceManager.Resources["pixel"], new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height), Color.Brown);
            base.Draw(spriteBatch);
        }

        public Texture2D Image
        {
            get { return tex; }
            set { tex = value; }
        }
    }
}

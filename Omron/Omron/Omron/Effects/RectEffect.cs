using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lidgren.Network;


namespace Omron.Effects
{
    class RectEffect : GraphicsEffect
    {
        RectPoly rect;

        public RectEffect(RectPoly r)
        {
            rect = r;
        }

        float elapsedTime = 0;
        public float LifetTime = 1;

        public override void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTime > LifetTime)
                IsActive = false;

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsHelper.DrawRectangleInv(spriteBatch, rect.Center, rect.Width, rect.Height, rect.Rotation, Color.HotPink);

        }

        public override void WriteOutInitialData(NetOutgoingMessage om)
        {
            
        }
        public override void ReadInInitialData(NetIncomingMessage im)
        {
            
        }

        public override Vector2 MainPos
        {
            get { return rect.Center; }
        }
    }
}

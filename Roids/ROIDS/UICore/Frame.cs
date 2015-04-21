using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Utilities;

namespace UICore
{
    /// <summary>
    /// A GUI Frame in which ui elements exist, as well as the game itself
    /// </summary>
    /// 
    public delegate void FrameEventHandler(Frame sender);
    public class Frame : Element
    {
        public bool Transparent { get;  protected set; }
        public bool PleaseDestroy { get; private set;  }
        public Texture2D BackgroundImage { get; set; }

        public event FrameEventHandler GainControl;
        public event FrameEventHandler LoseControl;

        public void OnLoseControl(Frame sender)
        {
            if (LoseControl != null)
                LoseControl(sender);
        }
        public void OnGainControl(Frame sender)
        {
            if (GainControl != null)
                GainControl(sender);
        }
        public Frame(Vector2 location, Size size, bool isTransparent)
            : base(location, size)
        {
            Location = location;
            ElementSize = size;
            Transparent = isTransparent;
            BackgroundImage = null;
            PleaseDestroy = false;
        }

        public Frame()
            : this(Vector2.Zero,
            (Size)DefaultSettings.Settings["WindowSize"],
            false)
        { }

        /// <summary>
        /// Setup Frame Here
        /// </summary>
        public virtual void Load() { }
        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            if (BackgroundImage != null)
                spriteBatch.Draw(BackgroundImage,
                    new Rectangle((int)Location.X,
                        (int)Location.Y,
                        (int)ElementSize.X,
                        (int)ElementSize.Y),
                        Color.White);
            base.Draw(time, spriteBatch);
        }

        public void Close()
        {
            this.PleaseDestroy = true;
            this.OnLoseControl(this);
        }
    }
}

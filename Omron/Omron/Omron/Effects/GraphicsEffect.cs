using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using Omron.Framework;
using Omron.Framework.Networking;
using Lidgren.Network;
using Lidgren.Network.Xna;

namespace Omron.Effects
{
    public abstract class GraphicsEffect : ISpriteDrawable, INetworkInitialize
    {
        public World world;

        public string Type;

        public bool IsActive = true;

        public abstract Vector2 MainPos { get; }

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);

        public abstract void WriteOutInitialData(NetOutgoingMessage om);
        public abstract void ReadInInitialData(NetIncomingMessage im);
    }

    public class EffectData
    {
        public string Type;
        public string Image;
    }

    public class Explosion : GraphicsEffect
    {
        Vector2 pos;
        float rad;

        public override Vector2 MainPos
        {
            get { return pos; }
        }

        Animation anim;
        string image;

        public Explosion()
        {
            Type = "Explosion";
        }
        public Explosion(Vector2 position, float radius, string img)
            :this()
        {
            pos = position;
            rad = radius;
            image = img;
            initAnim();
        }
        void initAnim()
        {
            AnimationData data = new AnimationData();
            data.ImageName = image;
            Texture2D firstTex = ResourceManager.Resources[data.ImageName][0];
            data.FPS = 10f;
            data.Loop = false;
            data.Reverse = false;
            data.SclHeight = firstTex.Width / (2 * rad);
            data.SclWidth = firstTex.Height / (2 * rad);
            data.Offset.HeightScale = 1f;
            data.Offset.WidthScale = 1f;
            anim = new Animation(data);
            anim.Start();
        }

        public override void Update(GameTime gameTime)
        {
            anim.Update(gameTime);

            if (anim.IsComplete)
                IsActive = false;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            anim.GetCurrentFrame().Draw(spriteBatch, pos, 0f, Color.White);
        }


        public override void WriteOutInitialData(NetOutgoingMessage om)
        {
            om.Write(pos);
            om.Write(rad);
            om.Write(image);
        }
        public override void ReadInInitialData(NetIncomingMessage im)
        {
            pos = im.ReadVector2();
            rad = im.ReadFloat();
            image = im.ReadString();
            initAnim();
        }
    }
    public class TractorBeam : GraphicsEffect
    {
        Actor source, targ;
        float lifeTime, elapsedTime;
        float girth;
        Vector2 spos, tpos;
        string image;
        Animation anim;

        public override Vector2 MainPos
        {
            get { return (spos + tpos) / 2f; }
        }

        public TractorBeam()
        {
            Type = "TractorBeam";
            IsActive = true;
            elapsedTime = 0f;
        }
        public TractorBeam(Actor source, Actor target, float lifeTime, float girth, string img)
            :this()
        {
            this.source = source;
            this.targ = target;
            this.lifeTime = lifeTime;
            this.girth = girth;
            image = img;

            initAnim();
        }
        void initAnim()
        {
            AnimationData data = new AnimationData();
            data.ImageName = image;
            Texture2D firstTex = ResourceManager.Resources[data.ImageName][0];
            data.FPS = 10f;
            data.Loop = false;
            data.Reverse = false;
            data.Offset.HeightScale = 1f;
            data.Offset.WidthScale = 1f;
            anim = new Animation(data);
            anim.Start();
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            anim.Update(gameTime);
            if (elapsedTime > lifeTime)
                IsActive = false;

            if (source != null)
                spos = source.Position;
            if (targ != null)
                tpos = targ.Position;

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            FrameData frame = anim.GetCurrentFrame();
            frame.SclWidth = frame.Image.Width / (tpos - spos).Length();
            frame.SclHeight = frame.Image.Height / girth;
            frame.Draw(spriteBatch, (tpos + spos) / 2,
                MathHelper.GetAngle(tpos - spos), Color.White);
        }

        public override void WriteOutInitialData(NetOutgoingMessage om)
        {
            om.Write(lifeTime);
            om.Write(girth);
            om.Write(source.ActorID);
            om.Write(targ.ActorID);
            om.Write(source.Position);
            om.Write(targ.Position);
            om.Write(image);
        }
        public override void ReadInInitialData(NetIncomingMessage im)
        {
            lifeTime = im.ReadFloat();
            girth = im.ReadFloat();
            source = world.GetActorByID(im.ReadUInt16());
            targ = world.GetActorByID(im.ReadUInt16());
            spos = im.ReadVector2();
            tpos = im.ReadVector2();
            image = im.ReadString();

            initAnim();
        }
    }
}

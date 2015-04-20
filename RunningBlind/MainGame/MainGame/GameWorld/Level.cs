using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using MainGame.GameWorld.GameActors;
namespace MainGame.GameWorld
{
    delegate void LevelEventHandler();
    class Level
    {
        protected Texture2D levelTex, levelFieldTex;
        public BoolField levelField { get; protected set; }

        public List<Actor> Actors { get; private set; }
        public PlayerActor Player { get; private set; }
        protected GameActors.ExitActor Exit { get; private set; }


        public event LevelEventHandler LevelEnded;
        public event LevelEventHandler LevelRestart;

        private List<IDrawable> _drawables;
        private List<IDirectDrawable> _directDrawables;

        Effect maskEffect;

        public PulseManager pulseMan;
        RenderTarget2D levelRender;

        public List<GraphicOverlay> GraphicOverlays { get; protected set; }

        public void Draw(GameTime gtime, SpriteBatch spriteBatch)
        {
            var GraphicsDevice = spriteBatch.GraphicsDevice;


            Texture2D mask = pulseMan.RenderMask();

            //render level + enemies to render target



            GraphicsDevice.SetRenderTarget(levelRender);
            GraphicsDevice.Clear(Color.Black);


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, null, null);
            spriteBatch.Draw(levelTex, Vector2.Zero, Color.White);

            foreach (var actor in _drawables)
                actor.Draw(spriteBatch);

            spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);



            

            //mask with generated pulses
            GraphicsDevice.Clear(Color.Black);

            //spriteBatch.Begin();
            //spriteBatch.Draw(levelRender, Vector2.Zero, Color.White);
            //spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
            maskEffect.Parameters["MaskTexture"].SetValue(mask);
            maskEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(levelRender, Vector2.Zero, Color.White);
            spriteBatch.End();



            //draw on top of level
            spriteBatch.Begin();
            foreach (IDirectDrawable drawable in _directDrawables) drawable.Draw(spriteBatch);

            foreach (var graphic in GraphicOverlays)
                graphic.Draw(spriteBatch);
            spriteBatch.End();
        }

        public bool CollidesField(Actor actor)
        {
            return levelField.TestIntersect(actor.BoundingBox);
        }

        Texture2D getPulsedTexture()
        {
            throw new NotImplementedException();
        }
        public void AddActor(Actor actor)
        {
            actor.ParentLevel = this;
            if (actor is ExitActor)
            {
                Exit = (ExitActor)actor;
                _drawables.Add((IDrawable)actor);
            }
            else if (actor is IDrawable) _drawables.Add((IDrawable)actor);
            else if (actor is PlayerActor)
            {
                Player = (PlayerActor)actor;
                _directDrawables.Add((IDirectDrawable)actor);
            }
            else if (actor is IDirectDrawable) _directDrawables.Add((IDirectDrawable)actor);
        }
        public virtual void LoadLevel()
        {
            foreach (Actor actor in Actors)
            {
                AddActor(actor);
            }
            Exit.Collided += new CollisionEventHandler(Exit_Collided);


            levelField = new BoolField(levelFieldTex);

            pulseMan = new PulseManager(levelTex.GraphicsDevice, levelTex.Width, levelTex.Height);

            levelRender = new RenderTarget2D(
                levelTex.GraphicsDevice,
                levelTex.Width,
                levelTex.Height,
                false,
                levelTex.GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
        }

        void Exit_Collided(Actor sender, Actor collededInto)
        {
            if (collededInto == Player && LevelEnded != null) LevelEnded();
        }

        public virtual void Update(GameTime time)
        {
            var list = Actors.ToList<Actor>();

            for (int i = 0; i < GraphicOverlays.Count; i++)
            {
                GraphicOverlays[i].Update(time);
                if (GraphicOverlays[i].Completed)
                    GraphicOverlays.RemoveAt(i);
            }
            for(int i = 0; i < Actors.Count; i++)
            {
                Actor actor = Actors[i];
                if (actor.IsDead)
                {
                    list.Remove(actor);
                    if (actor is IDrawable) _drawables.Remove((IDrawable)actor);
                }
                else actor.Update(time);
            }
            Actors = list;
            pulseMan.Update(time);
            if (Player.IsDead && LevelRestart != null) LevelRestart();
            if (Player.BoundingBox.Intersects(Exit.BoundingBox) && LevelEnded != null)
                LevelEnded();
        }

        public void Destroy()
        {
            LevelEnded = null;
            LevelRestart = null;
        }
        public Level()
        {
            Actors = new List<Actor>();
            _drawables = new List<IDrawable>();
            _directDrawables = new List<IDirectDrawable>();
            GraphicOverlays = new List<GraphicOverlay>();
            maskEffect = ResourceManager.Resources["maskEffect"];
        }

        internal void AddGraphicOverlay(GraphicOverlay graphic)
        {
            GraphicOverlays.Add(graphic);
        }
    }
}

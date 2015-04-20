using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MainGame.UIFrame;

namespace MainGame.GameWorld.GameActors
{
    class PlayerActor : Actor, IDirectDrawable
    {
        GameController _gameController;

        float speed = 2f;
        int halfSize = -1;//5;

        float lastFire;
        bool canFire = true;

        float TimeLimit = 3.0f;
        float Charge = 1.0f;

        public float GetRecharge()
        {
            return Charge;
        }
 
        public PlayerActor(Vector2 position, float orientation)
        {
            this.Position = position;
            this.Theta = orientation;
            this.Image = ResourceManager.Resources["avatar"];

            halfSize = this.Image.Width / 2;
        }
        public void connectController(UIFrame.GameController gameController)
        {
            _gameController = gameController;

            _gameController.MoveBackward += new GameControlEventHandler(_gameController_MoveBackward);
            _gameController.MoveLeft += new GameControlEventHandler(_gameController_MoveLeft);
            _gameController.MoveRight += new GameControlEventHandler(_gameController_MoveRight);
            _gameController.MoveForward += new GameControlEventHandler(_gameController_MoveForward);

            _gameController.AimGun += new GameSpacialEventHandler(_gameController_AimGun);
            _gameController.FireGun += new GameSpacialEventHandler(_gameController_FireGun);

        }

        void _gameController_FireGun(Vector2 postion)
        {
            if (Charge >= 1f)
            {
                Bullet b = new Bullet(this, true);
                b.Fire(postion);
                Charge = 0f;
            }
        }

        void _gameController_AimGun(Vector2 postion)
        {
            this.Theta = (float)Math.Atan2(postion.Y - this.Position.Y, postion.X - this.Position.X);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime time)
        {
            Charge += (float)time.ElapsedGameTime.TotalSeconds / TimeLimit;

            if (Charge > 1)
                Charge = 1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.Image, this.Position, null, Color.White, this.Theta+135*(float)Math.PI/180, new Vector2(this.Image.Width/2, this.Image.Height/2), Vector2.One, SpriteEffects.None, 0.1f);
        }

        Vector2 lastFootstep;
        Vector2 lastCollidePos;
        bool tryChangePosition(Vector2 del)
        {
            var oldPos = this.Position;
            this.Position  += del;

            if (this.ParentLevel.CollidesField(this))
            {
                this.Position = oldPos;

                if ((this.Position - lastCollidePos).Length() > 4)
                    ParentLevel.pulseMan.StartPulse(this.Position, 70f);

                lastCollidePos = this.Position;
                return false;
            }

            //if ((this.Position - lastFootstep).Length() > 20)
            //{
            //    ParentLevel.AddActor(new FootstepActor(this.Position, this.Theta+(float)Math.PI/2));
            //    lastFootstep = this.Position;
            //}

            return true;
        }

        void _gameController_MoveForward()
        {
            tryChangePosition(-Vector2.UnitY * speed);
        }

        void _gameController_MoveRight()
        {
            tryChangePosition(Vector2.UnitX * speed);
        }

        void _gameController_MoveLeft()
        {
            tryChangePosition(-Vector2.UnitX * speed);
        }

        void _gameController_MoveBackward()
        {
            tryChangePosition(Vector2.UnitY * speed);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PhysicsCore;
using WorldCore;
using Microsoft.Xna.Framework;

using Utilities;

namespace ROIDS.GameObjects
{
    public delegate void PlayerEventHandler(dynamic argument);
    class Player : Actor, ICircleBody, IPlayer, IHealthable
    {
        private float _maxHealth;
        public float MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                _maxHealth = value;
                CurrentHealth = _maxHealth;
            }
        }
        public float CurrentHealth { get; set; }
        public float SpeedScale { get; set; }
        public float Radius { get; set; }
        private Color _playerColor { get; set; }

  
        public override Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }

        public Player(Vector2 position, float orientation)
            : base(position, orientation)
        {
            Mass = 1;
            Radius = 30;
            _playerColor = Color.Blue;
            SpeedScale = 2000;
            this.Collided += new CollisionEventHandler(Player_Collided);
        }

        void Player_Collided(IRigidBody impactedBody)
        {
            if (impactedBody is Bomb)
                this.Hurt(20);
        }

        public event PlayerEventHandler GotHurt;
        public void Hurt(float damage)
        {
            if (GotHurt != null)
                GotHurt(damage);
        }      

        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            //Utilities.GraphicsUtils.DrawBall(Position, Radius, _playerColor, 255);
            GraphicsUtils.DrawBallTex(ContentRepository.Repository["shipbrick"], Position, Radius, this.Rotation, 255);
        }

        public void Update(GameTime time)
        {
            //Nothing

        }
    }
}

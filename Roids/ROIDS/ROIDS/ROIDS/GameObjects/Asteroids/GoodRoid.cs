using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PhysicsCore;
using WorldCore;
using Utilities;

namespace ROIDS.GameObjects.Asteroids
{
    class GoodRoid : Asteroid
    {
        public float FiringDelay { get; set; }
        float tdelay = 0.0f;
        public bool Shoot { get; set; }
        public float VelThreshold { get; set; }

       public GoodRoid(Vector2 position, float radius, float velThresh, float fdelay, Vector2 velocity)
            : base(position, radius)
        {
            Velocity = velocity;
            VelThreshold = velThresh;
            FiringDelay = fdelay;
            this.Collided += new CollisionEventHandler(GoodRoid_Collided);
            Shoot = false;
        }

        void GoodRoid_Collided(IRigidBody impactB)
        {
            //
        }

        public override void Update(GameTime gameTime)
        {
            if (Shoot)
            {
                tdelay -= gameTime.ElapsedGameTime.Milliseconds / 1000f;

                if (tdelay <= 0)
                {
                    shootCharge();
                    tdelay = FiringDelay;
                }
            }

            var x = (float)Math.Pow((2 / Math.PI * Math.Atan(this.Velocity.Length() / 5)), 2);
            this.Color = MathUtils.ColorLerp(Color.Gray, Color.Cyan, x);
        }

        private void shootCharge()
        {
            var game = (GameStates.PlayableState)GameCore.GameEngine.Singleton.FindGameState(x => x is GameStates.PlayableState);

            var player = (Player)game.ActiveMap.Player;

            var mdir = Vector2.Normalize(player.Position - this.Position);
            var launchPort = this.Position + mdir * (this.Radius + 5); //launch stuff from slightly in front of player
            float launchVel = 500f + player.Velocity.Length();

            var carrier = new Bomb(launchPort, 5, 1/1000f, this);
            carrier.Velocity = launchVel * mdir;


            game.SpawnObject<Actor>(carrier);

            var c = new GameObjects.Devices.Charge(10, 1);
            game.SpawnObject<Device>(c);
            carrier.AddDevice(c);
        }

        public override void Hurt(float damage)
        {
            base.Hurt(damage);
        }
    }
}

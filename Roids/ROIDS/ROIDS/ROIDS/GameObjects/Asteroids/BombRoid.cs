using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PhysicsCore;
using WorldCore;
using Utilities;
using ROIDS.GameObjects.Devices;
using GameCore;
using ROIDS.GameStates;

namespace ROIDS.GameObjects.Asteroids
{
    class BombRoid : Asteroid
    {
        bool fuseOn = false;
        public float FuseTime = 0.0f; //default = no delay
        public const float damageConstant = 1 / 50f;
        Charge _charge;

        public BombRoid(Vector2 position, float radius)
            : base(position, radius)
        {
            this.Collided += new CollisionEventHandler(Astroid_Collided);
            this.Color = Color.Red;
            this.MaxHealth = radius * radius / 50;

            _charge = new Charge(1.5f * Radius, 2f);
            this.AddDevice(_charge);
        }

        void Astroid_Collided(IRigidBody impactBody)
        {
            if (impactBody is Player)// /*|| impactBody is BombRoid || impactBody is GoodRoid*/ || impactBody is Bomb)
                this.TriggerFuse(FuseTime);
            
        }

        public override void Update(GameTime gameTime)
        {
            if (fuseOn) 
            {
                FuseTime -= gameTime.ElapsedGameTime.Milliseconds / 1000f;
                if (FuseTime <= 0)
                {
                    _charge.Detonate(damageConstant);
                }
            }

        }
        public void TriggerFuse(float time)
        {
            fuseOn = true;
            FuseTime = time;
        }

        public override void Destroy()
        {
           base.Destroy();
        }
    }
}

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

namespace ROIDS.GameObjects
{
    class Bomb : DeviceCarrierActor, ICircleBody
    {

        public float Radius { get; set; }
        public override Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }

        Actor _launcher;
        public float DamageConstant { get; private set; }
        public Bomb(Vector2 pos, float rad, float damageConstant, Actor launcher)
            : base(pos, rad)
        {
            this.Mass = .1f;
            this.DragCoefficient = .001f;

            Position = pos;
            Radius = rad;
            this.Collided += new CollisionEventHandler(Bomb_Collided);
        }

        protected void Bomb_Collided(IRigidBody impactB)
        {
            if (impactB == _launcher)
                return;
            else if (impactB is Asteroid)
            {
                this.Destroy();
            }
            else
            {
                ((Charge)this.GetDevices()[0]).Detonate(DamageConstant);
            }
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsUtils.DrawBall(Position, Radius, Color.Teal, 255);
        }
    }
        
}

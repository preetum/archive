using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PhysicsCore;
using WorldCore;
using Utilities;

namespace ROIDS.GameObjects
{
    public class DeviceTransporter : DeviceCarrierActor, ICircleBody
    {

        public float Radius { get; set; }
        public override Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }

        public DeviceTransporter(Vector2 pos, float rad)
            : base(pos, 0f)
        {
            this.Mass = .1f;
            this.DragCoefficient = .001f;

            Position = pos;
            Radius = rad;
            this.Collided += new CollisionEventHandler(DeviceTransporter_Collided);
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsUtils.DrawBall(Position, Radius, Color.Teal, 255);
        }

        protected virtual void DeviceTransporter_Collided(IRigidBody impactB)
        {
            if (impactB is DeviceTransporter)
            {
                //lol. explode maybe?
            }
            else if (impactB is Bomb)
            {
                //loololol!
            }
            else if (impactB is DeviceCarrierActor)
            {
                //transfer payload to other body upon impact
                var target = (DeviceCarrierActor)impactB;

                foreach (Device device in GetDevices())
                {
                    target.AddDevice(device);
                    device.Suppressed = false;
                }
                this.ClearDevices();

                selfdestruct();
            }
        }
        public override void AddDevice(Device d)
        {
            base.AddDevice(d);
            d.Suppressed = true;
        }

        void selfdestruct()
        {
            //animation?
            this.Destroy();
        }

    }
}

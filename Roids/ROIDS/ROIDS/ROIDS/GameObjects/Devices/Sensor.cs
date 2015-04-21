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
using ROIDS.GameObjects.Asteroids;

namespace ROIDS.GameObjects.Devices
{
    class Sensor : Device, IDynamicObject

    {
        public float Radius { get; set; }

        private Color _sensorColor;

        public void Update(GameTime time)
        {
            if (this.Parent == null) return;

            if (!Suppressed)
            {
                float prad;
                if (this.Parent is ICircleBody)
                    prad = ((ICircleBody)this.Parent).Radius;
                else
                    prad = this.Parent.BoundingBox.Width / 2;

                var range = Region.FromCircle(this.Position, prad + Radius);

                var PE = ((PlayableState)GameEngine.Singleton
                              .FindGameState(x => x is PlayableState))
                              .PhysicsManager;

                var near = PE.QTbodies.Query(range);

                int numRoids = near.Where(b => (b is BadRoid) && ((this.Position - b.Position).Length() <= ((BadRoid)b).Radius + prad + Radius)).Count();

                /*foreach (IRigidBody body in near)
                {
                    if (body is BadRoid)
                    {
                        var d = (this.Position - body.Position).Length(); //MathUtils.Distance(Position, body.Position); 
                        if (d <= body.BoundingBox.Width/2 + this.Parent.BoundingBox.Width / 2 + Radius)
                        {
                            //Console.WriteLine(d);
                            numRoids++;
                        }

                    }
                }*/

                _sensorColor = Color.Lerp(Color.Green, Color.Red, (float)numRoids / 3); //Color.FromNonPremultiplied(i * 50, 100, i * 50, 255);
            }
        }

        public override void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            GraphicsUtils.DrawBall(Position, 5, Color.Indigo, 255, 0.1f);

            if (!Suppressed)
                GraphicsUtils.DrawBall(Position, this.Parent.BoundingBox.Width / 2 + Radius, _sensorColor, 50, 0.0f);
        }

        public override bool Suppressed
        {
            get
            {
                return base.Suppressed;
            }
            set
            {
                base.Suppressed = value;
            }
        }
        public Sensor(float radius)
        {
            _sensorColor = Color.LimeGreen;
            Radius = radius;
        }
    }
}

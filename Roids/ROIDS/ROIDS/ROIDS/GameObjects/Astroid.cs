using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ROIDS;
using GameCore;
using PhysicsCore;
using WorldCore;
using Utilities;

using Microsoft.Xna.Framework;

namespace ROIDS.GameObjects
{
    class Astroid : Actor, ICircleBody, IDynamicObject
    {
        private Color _color;
        public override PhysicsCore.Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }
        public float Radius { get; set; }
        public bool isRoid { get; set; }
        
        public void Update(GameTime time)
        {
            //
        }


        public Astroid(Vector2 position, float radius, float orientation, bool roid)
            : base(position, orientation)
        {
            this.Collided += new CollisionEventHandler(Astroid_Collided);
            this.Radius = radius;
            _color = Color.Gray;
            isRoid = roid;

            this.DragCoefficient = 0.1f * RigidBodyHelper.DefaultDragCoeff;
        }

        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Color c = isRoid ? Color.Red : Color.Gray; // kind of defeats the purpose of having a _color field doesn't it. yeah, basically. all of this shit is hacks until we get graphics figured out anyway, though.
            Utilities.GraphicsUtils.DrawBall(Position, Radius, c, 255);
        }
        void Astroid_Collided(IRigidBody impactB)
        {
            if ((impactB is Player || impactB is Astroid) && isRoid)
            {
                var PE = ((GameStates.IPlayable)GameEngine.Singleton
                            .FindGameState(x => x is GameStates.IPlayable))
                            .PhysicsManager;

                this.Destroy();

                var brad = this.Radius * 1.2f;
                var explosion = ParticleSystemFactory.GetDirtyBomb(this.Position, brad);
                PE.AddParticleSystems(explosion);
                PE.AddInstantaneousForceField(new InstantaneousForceField(this.Position, brad, DefaultForces.GenerateExplosiveField(brad, 1)));
            }
            
        }
    }
}

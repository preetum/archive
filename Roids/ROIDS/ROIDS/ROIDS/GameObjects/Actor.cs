using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WorldCore;
using PhysicsCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ROIDS.GameObjects
{
    /// <summary>
    /// Main ROIDS Game Object
    /// </summary>
    /// 
    public delegate void ActorEventHandler(Actor sender);

    public abstract class Actor: WorldObject, IRigidBody
    {

        /*
         * IRigidBody Reqs
         */
        public dynamic Tag { get; set; }
        public virtual bool HasInfiniteMass { get; protected set; }
        public float Mass { get; set; }

        public float DragCoefficient { get; set; }

        public virtual Vector2 Velocity { get; set; }
        public virtual float AngularVelocity { get; set; }

        public float RotationalInertia { get; set; }

        public float Restitution { get; protected set; }

        public abstract Region BoundingBox { get; }
        public Region Span { get { return BoundingBox; } }

        public virtual Vector2 FNET { get; set; }
        public float TNET { get; set; }

        public event CollisionEventHandler Collided;
        public void OnCollided(IRigidBody impactB)
        {
            if (Collided != null)
                Collided(impactB);
        }
        /*
         * Graphics Reqs
         */
        //public abstract void Draw(GameTime time, SpriteBatch spriteBatch);

        /*
         * Constructor
         */

        public Actor(Vector2 position, float orientation) 
            : base(position, orientation)
        {
            // Physics Initialize
            Mass = 1;
            Position = position;

            Restitution = RigidBodyHelper.DefaultRestitution;
            DragCoefficient = RigidBodyHelper.DefaultDragCoeff;

            Velocity = Vector2.Zero;
            AngularVelocity = 0f;
            Rotation = 0f;
            Tag = false;

            FNET = Vector2.Zero;
        }

        public override void Destroy()
        {
            var game = (GameStates.PlayableState)GameCore.GameEngine.Singleton
                .FindGameState(x => x is GameStates.PlayableState);
            game.KillObject(this);     
       
            base.Destroy();
        }
    }
}

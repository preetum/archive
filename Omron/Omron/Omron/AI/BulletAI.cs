using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Omron.Actors;
using Omron.Framework;
using Omron;

namespace Omron.AI
{
    public class BulletAI : IUnitAI
    {
        FatherUnit actor;
        Vector2 dir;
        float speed;
        UnitAIState state;
        public UnitAIState State { get { return state; } }

        public BulletAI(FatherUnit actor, float maxSpeed)
        {
            this.actor = actor;
            speed = maxSpeed;
            state = UnitAIState.Holding;
            actor.Collided += new CollisionEventHandler(actor_Collided);
        }

        void actor_Collided(Actor collideactor)
        {
            if (!actor.IsDead && actor.Faction.GetRelationship(collideactor.Faction) != FactionRelationship.Allied)
            {
                actor.IssueMeleeAttack(actor.Position);
                actor.Die();
            }
        }
        public void Build(FatherBuilding building)
        {
            throw new Exception("loool, bullets can't build!");
        }
        public void SetTargetAggressive(Vector2 target)
        {
            SetTarget(target);
        }

        public void SetTarget(Vector2 target)
        {
            state = UnitAIState.Tracking;
            dir = Vector2.Normalize(target - actor.Position);
            updateRot();
        }
        public void Engage(Actor enemy)
        {
            SetTarget(enemy.Position);
        }
        public void updateRot()
        {
            actor.DesiredRotation = MathHelper.GetAngle(dir);// -(float)Math.PI / 2f;
            actor.Rotation = actor.DesiredRotation; //ONLY SAFE because bullet will be isolated actor.
        }
        public void UpdateMotionFast(GameTime gameTime)
        {
            if (state == UnitAIState.Holding)
            {
                actor.DesiredVelocity = Vector2.Zero;
            }
            else if (state == UnitAIState.Tracking)
            {
                var currPos = actor.Position;
                actor.DesiredVelocity = speed * dir;
            }
        }
        public void UpdateSlow(GameTime gameTime)
        {
        }
    }
}

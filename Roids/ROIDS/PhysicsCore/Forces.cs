using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace PhysicsCore
{
    public delegate Vector2 UniversalForceGen(IRigidBody target);

    public delegate float UniversalTorqueGen(IRigidBody target);

    public delegate Vector2 ForceFieldGen(Vector2 relPos);

    public struct UniversalForce
    {
        public UniversalForceGen GetForce;

        public UniversalForce(UniversalForceGen forceGen)
        {
            GetForce = forceGen;
        }
    }

    public struct UniversalTorque
    {
        public UniversalTorqueGen GetTorque;

        public UniversalTorque(UniversalTorqueGen torqueGen)
        {
            GetTorque = torqueGen;
        }
    }

    public struct BoundForceField
    {
        public ForceFieldGen GetForce;
        public float Radius;
        public IRigidBody SourceBody;

        public Region AreaInfluenced
        {
            get { return Region.FromCircle(SourceBody.Position, Radius); }
        }

        public BoundForceField(IRigidBody source, float radius, ForceFieldGen forceGen)
        {
            SourceBody = source;
            Radius = radius;
            GetForce = forceGen;
        }
    }


    public struct InstantaneousForceField
    {
        public ForceFieldGen GetForce;
        public float Radius;
        public Vector2 SourcePos;

        public Region AreaInfluenced
        {
            get { return Region.FromCircle(SourcePos, Radius); }
        }

        public InstantaneousForceField(Vector2 pos, float radius, ForceFieldGen forceGen)
        {
            SourcePos = pos;
            Radius = radius;
            GetForce = forceGen;
        }
    }

    public struct InstantaneousForce
    {
        public IRigidBody TargetBody;
        public Vector2 Force;
        public InstantaneousForce(IRigidBody target, Vector2 force)
        {
            TargetBody = target;
            Force = force;
        }
    }

    public static class DefaultForces
    {
        public static float DRAGCOEFF = 1f;
        public static float WEAKDRAGCOEFF = .15f;
        public static float LINDRAGCOEFF = 50f;
        public static float DRAGDENSITY = .1f;
        public static float TORQUEDRAGCOEFF = .1f;

        public static UniversalForce Gravity = new UniversalForce(b => 100f * Vector2.UnitY * b.Mass);

        public static UniversalForce Drag = new UniversalForce(b => 
            {
                if (b.Velocity.Length() != 0)
                    return -Vector2.Normalize(b.Velocity) * DRAGDENSITY * DRAGCOEFF  *b.DragCoefficient * b.Velocity.LengthSquared();
                else
                    return Vector2.Zero;
            });

        public static UniversalForce WeakDrag = new UniversalForce(b =>
        {
            if (b.Velocity.Length() != 0)
                return -Vector2.Normalize(b.Velocity) * DRAGDENSITY * WEAKDRAGCOEFF *b.DragCoefficient * b.Velocity.LengthSquared();
            else
                return Vector2.Zero;
        });

        public static UniversalForce LinearDrag = new UniversalForce(b =>
        {
            if (b.Velocity.Length() != 0)
            {
                return -Vector2.Normalize(b.Velocity) * DRAGDENSITY * LINDRAGCOEFF * b.DragCoefficient * b.Velocity.Length();
            }
            else
                return Vector2.Zero;
        });

        public static UniversalTorque FrictionalTorque = new UniversalTorque(b => -TORQUEDRAGCOEFF * b.AngularVelocity);

        public static ForceFieldGen StrongAttractor = (rPos => -5000.0f * Vector2.Normalize(rPos) / rPos.LengthSquared());

        public static ForceFieldGen WeakAttractor = (rPos => -100.0f * Vector2.Normalize(rPos) / rPos.LengthSquared());

        public static ForceFieldGen GenerateDirectionalField(Vector2 force, float rad)
        {
            return (rPos =>
                {
                    var rnorm = rPos / rad;
                    return force / (1 + rnorm.LengthSquared() * rnorm.LengthSquared());
                });
        }

        public static ForceFieldGen GenerateExplosiveField(float rad, float intensity)
        {
            return (rPos =>
                {
                    var x = rPos.Length() / rad;
                    var f = 1 / (1 + (float)Math.Pow(2 * x, 2));
                    return 20000f * intensity * f * Vector2.Normalize(rPos);
                });
        }
    }
}

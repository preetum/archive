using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PhysicsCore
{

    public delegate void CollisionEventHandler(IRigidBody impactB);

    public interface IRigidBody:IRegion, Utilities.ISpatialEntity
    {
        dynamic Tag { get; set; }

        bool HasInfiniteMass { get; }
        // float InvMass { get; }

        float Mass { get; }

        float DragCoefficient { get; set; }

        // Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        float AngularVelocity { get; set; }
        // float Rotation { get; set; }
        float RotationalInertia { get; set; }

        float Restitution { get; }

        Region BoundingBox { get; }

        //float DragCoef { get; }  //WE NEED THIS!!!
        Vector2 FNET { get; set; }
        float TNET { get; set; }

        event CollisionEventHandler Collided;
        void OnCollided(IRigidBody targetB);

        //public RigidBody(float mass, Vector2 pos)
        //{
        //    Mass = mass;
        //    Position = pos;

        //    Restitution = 0.2f;

        //    Velocity = Vector2.Zero;
        //    AngularVelocity = 0f;
        //    Rotation = 0f;

        //    Flag = false;
        //}
        
    }

   
    public interface ICircleBody : IRigidBody
    {
        float Radius { get; set; }

        //public override Region BoundingBox
        //{
        //    get { return Region.FromCircle(Position, Radius); }
        //}
        //public CircleBody(float rad, float mass, Vector2 pos)
        //    : base(mass, pos)
        //{
        //    Radius = rad;
        //}
    }
    

    public interface IPolyBody : IRigidBody
    {

    }

    public interface IVertWallBody : IRigidBody
    {
        float Length { get; }

        //public override Region BoundingBox
        //{
        //    get { return new Region(Position.X - 30, Position.X + 30, Position.Y - Length / 2, Position.Y + Length / 2); }
        //}

        //public VertWallBody(Vector2 pos, float length)
        //    : base(1, pos)
        //{
        //    HasInfiniteMass = true;
        //    Length = length;
        //}
    }

    public interface IHorizWallBody : IRigidBody
    {
        float Length { get; }

        //public override Region BoundingBox
        //{
        //    get { return new Region(Position.X - Length / 2, Position.X + Length / 2, Position.Y - 30, Position.Y + 30); }
        //}

        //public HorizWallBody(Vector2 pos, float length)
        //    : base(1, pos)
        //{
        //    HasInfiniteMass = true;
        //    Length = length;
        //}
    }

    public static class RigidBodyHelper
    {
        public static float InvMass(this IRigidBody rigidBody)
        {
            if (rigidBody.HasInfiniteMass)
                return 0.0f;
            else return 1.0f / rigidBody.Mass;
        }

        public static float DefaultRestitution { get { return 0.5f; } }
        public static float DefaultDragCoeff { get { return 1.0f; } }
    }
    
}

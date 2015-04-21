using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using System.Collections.Concurrent;

namespace PhysicsCore
{
    public struct Contact
    {
        public Vector2 ConPoint;
        public Vector2 Normal;
        public float Disp;

        /// <summary>
        /// The body which is being hit. Normal is with respect to this body. (outward from TargetBody)
        /// </summary>
        public IRigidBody TargetBody;
        
        /// <summary>
        /// The body impacting TargetBody.
        /// </summary>
        public IRigidBody ImpactBody;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetB">The body which is being hit. Normal is with respect to this body. (outward from TargetBody)</param>
        /// <param name="impactB">The body impacting TargetBody.</param>
        /// <param name="conpt"></param>
        /// <param name="norm">with respect to this body. (outward from TargetBody)</param>
        /// <param name="disp"></param>
        public Contact(IRigidBody targetB, IRigidBody impactB, Vector2 conpt, Vector2 norm, float disp)
        {
            TargetBody = targetB;
            ImpactBody = impactB;
            ConPoint = conpt;
            Normal = norm;
            Disp = disp;
        }
    }

    public struct CollisionData
    {
        public IRigidBody TargetBody;

        public Vector2 dPos;
        public Vector2 dVel;
        public float dAngVel;

        public static CollisionData operator +(CollisionData a, CollisionData b)
        {
            CollisionData ret = a;
            ret.dPos += b.dPos;
            ret.dVel += b.dVel;
            ret.dAngVel += b.dAngVel;
            return ret;
        }
        public static CollisionData operator /(CollisionData a, float b)
        {
            CollisionData ret = a;
            ret.dPos /= b;
            ret.dVel /= b;
            ret.dAngVel /= b;
            return ret;
        }

        public CollisionData(IRigidBody targetB, Vector2 dpos, Vector2 dvel, float dangvel)
        {
            TargetBody = targetB;

            dPos = dpos;
            dVel = dvel;
            dAngVel = dangvel;
        }
    }

    public static class CollisionEngine
    {

        public static float HARDNESS = 0.2f;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetB"></param>
        /// <param name="impactB"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static bool TestNarrowCollision(IRigidBody targetB, IRigidBody impactB, out Contact contact)
        {
            contact.TargetBody = targetB;
            contact.ImpactBody = impactB;

            contact.Disp = 0; //default vals so compiler doesn't complain
            contact.Normal = Vector2.Zero;
            contact.ConPoint = Vector2.Zero;

            if (targetB is ICircleBody && impactB is ICircleBody)
            {
                return TestNarrowCircleCircle((ICircleBody)targetB, (ICircleBody)impactB, out contact);
            }
            else if (targetB is ICircleBody && impactB is IPolyBody)
            {
                return TestNarrowCirclePoly((ICircleBody)targetB, (IPolyBody)impactB, out contact);

            }
            else if (targetB is IPolyBody && impactB is ICircleBody)
            {
                var b = TestNarrowCirclePoly((ICircleBody)impactB, (IPolyBody)targetB, out contact);

                //invert all contact attributes, since we passed inverted targetB/impactB pair
                var tmp = contact.TargetBody;
                contact.TargetBody = contact.ImpactBody;
                contact.ImpactBody = tmp;
                contact.Disp *= -1;
                contact.Normal *= -1;

                return b;
            }
            else if (targetB is IPolyBody && impactB is IPolyBody)
            {
                return TestNarrowPolyPoly((IPolyBody)targetB, (IPolyBody)impactB, out contact);
            }

            //EVERYTHING BELOW THIS POINT IS HAX UNTIL WE HAVE POLY
            else if (targetB is ICircleBody && impactB is IVertWallBody)
            {
                var targetC = (ICircleBody)targetB;
                var impactW = (IVertWallBody)impactB;
                var BB = impactW.BoundingBox;


                if (targetC.Position.Y + targetC.Radius > BB.YMin && targetC.Position.Y - targetC.Radius < BB.YMax)
                {
                    if (targetC.Position.X - targetC.Radius < BB.XMax && targetC.Position.X - targetC.Radius > BB.XMin) //right hand wall
                    {
                        contact.Disp = -(BB.XMax - (targetC.Position.X - targetC.Radius));
                        contact.Normal = -Vector2.UnitX; //from circle to wall
                        return true;
                    }
                    else if (targetC.Position.X + targetC.Radius > BB.XMin && targetC.Position.X + targetC.Radius < BB.XMax) //left hand wall
                    {
                        contact.Disp = -((targetC.Position.X + targetC.Radius) - BB.XMin);
                        contact.Normal = Vector2.UnitX;
                        return true;
                    }
                    contact.ConPoint = targetC.Position + contact.Normal * (targetC.Radius + contact.Disp / 2);
                }

                return false;

            }
            else if (targetB is ICircleBody && impactB is IHorizWallBody)
            {
                var targetC = (ICircleBody)targetB;
                var impactW = (IHorizWallBody)impactB;
                var BB = impactW.BoundingBox;


                if (targetC.Position.X + targetC.Radius > BB.XMin && targetC.Position.X - targetC.Radius < BB.XMax)
                {
                    if (targetC.Position.Y - targetC.Radius < BB.YMax && targetC.Position.Y - targetC.Radius > BB.YMin) //bottom wall
                    {
                        contact.Disp = -(BB.YMax - (targetC.Position.Y - targetC.Radius));
                        contact.Normal = -Vector2.UnitY; //from circle to wall
                        return true;
                    }
                    else if (targetC.Position.Y + targetC.Radius > BB.YMin && targetC.Position.Y + targetC.Radius < BB.YMax) //top wall
                    {
                        contact.Disp = -((targetC.Position.Y + targetC.Radius) - BB.YMin);
                        contact.Normal = Vector2.UnitY;
                        return true;
                    }

                    contact.ConPoint = targetC.Position + contact.Normal * (targetC.Radius + contact.Disp / 2);


                }
                return false;

            }
            else
            {
                throw new NotImplementedException();

                //SAT

                //sub-contact detection

                //...etc.
            }
        }
        public static bool TestNarrowCircleCircle(ICircleBody targetC, ICircleBody impactC, out Contact contact)
        {
            contact.TargetBody = targetC;
            contact.ImpactBody = impactC;

            contact.Disp = 0;
            contact.Normal = Vector2.Zero;
            contact.ConPoint = Vector2.Zero;


            float disp = (impactC.Position - targetC.Position).Length() - targetC.Radius - impactC.Radius;

            if (disp <= 0)
            {
                var norm = Vector2.Normalize(impactC.Position - targetC.Position);
                var conpt = targetC.Position + norm * (targetC.Radius + disp / 2); //go out to edge, then in halfway (disp is negative)

                contact.Disp = disp;
                contact.Normal = norm;
                contact.ConPoint = conpt;

                return true;
            }
            else
                return false;
        }

        public static bool TestNarrowCirclePoly(ICircleBody targetC, IPolyBody impactP, out Contact contact)
        {
            contact.TargetBody = targetC;
            contact.ImpactBody = impactP;

            contact.Disp = 0;
            contact.Normal = Vector2.Zero;
            contact.ConPoint = Vector2.Zero;

            throw new NotImplementedException();
        }

        public static bool TestNarrowPolyPoly(IPolyBody targetP, IPolyBody impactP, out Contact contact)
        {
            contact.TargetBody = targetP;
            contact.ImpactBody = impactP;

            contact.Disp = 0;
            contact.Normal = Vector2.Zero;
            contact.ConPoint = Vector2.Zero;

            throw new NotImplementedException();
        }

        public static CollisionData CalcCollision(Contact con)
        {
            var cdata = new CollisionData();
            cdata.TargetBody = con.TargetBody;
            cdata.dPos = Vector2.Zero;
            cdata.dVel = Vector2.Zero;
            cdata.dAngVel = 0;

            var A = con.TargetBody;
            var B = con.ImpactBody;

            //penetration resolution
            var totalInvMass = A.InvMass() + B.InvMass();
            cdata.dPos = (con.Disp * A.InvMass() / (totalInvMass)) * con.Normal; //more massive-> less weight. objects in collision share burden of displacing proportional to 1/M.

            //collision response
            var sepVel = Vector2.Dot(B.Velocity - A.Velocity, con.Normal); //separating velocity: relative velocity of impact body along normal.
            if (sepVel >= 0) return cdata; //return no change if bodies are separating

            float rest = (A.Restitution+B.Restitution)/2;
            float impulse = sepVel * (1 + rest) / (totalInvMass);
            cdata.dVel = (impulse * A.InvMass()) * con.Normal;


            return cdata;
        }


        public static List<CollisionData> TestAndGetAllCollisionData(IRigidBody target, List<IRigidBody> potHits)
        {
            var colData = new List<CollisionData>();

            foreach (var p in potHits)
            {
                Contact c;
                if (TestNarrowCollision(target, p, out c))
                {
                    colData.Add(CalcCollision(c));

                    target.OnCollided(p);
                }
            }

            return colData;
        }

        

        public static CollisionData AggrCollisionData(List<CollisionData> cdata)
        {
            var aggr = new CollisionData();
            if (cdata.Count > 0)
            {
                aggr.TargetBody = cdata.First().TargetBody;

                foreach (var cd in cdata)
                    aggr += cd;

                //aggr /= cdata.Count;
                aggr.dPos *= HARDNESS;
            }
            return aggr;
        }
        public static void ApplyCollisionData(CollisionData cdata)
        {
            if (cdata.TargetBody == null) return;

            cdata.TargetBody.Position += cdata.dPos;
            cdata.TargetBody.Velocity += cdata.dVel;
            cdata.TargetBody.AngularVelocity += cdata.dAngVel;
        }


        public static void ResolveAll(List<IRigidBody> bodies, List<IRigidBody> mapbodies, QuadTree<IRigidBody> QTbodies, QuadTree<IRigidBody> QTmap)
        {

            var collisionData = new ConcurrentBag<CollisionData>();

            //foreach (var b in bodies)
            bodies.AsParallel().ForAll(b =>
            {
                var potHits = QTbodies.Query(b.BoundingBox); //broad-phase
                potHits.Remove(b); //the body itself will be included in query results.
                potHits.AddRange(QTmap.Query(b.BoundingBox));

                if (potHits.Count > 0)
                {
                    var allCollData = TestAndGetAllCollisionData(b, potHits); //narrow-phase

                    if (allCollData.Count > 0)
                    {
                        CollisionData aggrColData = AggrCollisionData(allCollData);
                        collisionData.Add(aggrColData);
                    }
                }
            });


            foreach (var cd in collisionData) ApplyCollisionData(cd); //apply all aggregated collision responses.
        }
    }
}

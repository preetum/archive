using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using System.Collections.Concurrent;

namespace PhysicsCore
{
    public class PhysicsEngine
    {
        public List<IRigidBody> ActiveBodies;
        public List<IRigidBody> MapBodies;

        public QuadTree<IRigidBody> QTbodies;
        public QuadTree<IRigidBody> QTmap;

        public List<UniversalForce> uforces;
        public List<UniversalTorque> utorques;
        public List<BoundForceField> bffields;
        public List<InstantaneousForceField> iffields;
        public List<InstantaneousForce> iforces;

        public List<ParticleSystem> PSystems;

        ConcurrentBag<IRigidBody> toDelete;

        public static int MAXBUCKET;
        public static Region MAXSPAN;
        


        public PhysicsEngine(Region maxspan, int maxbucket)
        {
            MAXSPAN = maxspan;
            MAXBUCKET = maxbucket;

            ActiveBodies = new List<IRigidBody>();
            MapBodies = new List<IRigidBody>();

            uforces = new List<UniversalForce>();
            utorques = new List<UniversalTorque>();
            bffields = new List<BoundForceField>();
            iffields = new List<InstantaneousForceField>();
            iforces = new List<InstantaneousForce>();

            PSystems = new List<ParticleSystem>();

            toDelete = new ConcurrentBag<IRigidBody>();

            QTbodies = new QuadTree<IRigidBody>(MAXSPAN, maxbucket, 10);
            QTmap = new QuadTree<IRigidBody>(MAXSPAN, maxbucket, 10);
        }
        public static QuadTree<IRigidBody> GenerateQT(List<IRigidBody> bodies, int maxbucket)
        {
            var QT = new QuadTree<IRigidBody>(MAXSPAN, maxbucket, 10);
            bodies.ForEach(b => QT.AddNode(b));
            return QT;
        }

        void updateQTs()
        {
            QTbodies = GenerateQT(ActiveBodies, MAXBUCKET);
            QTmap = GenerateQT(MapBodies, 1);
        }
        public void Update(float dt)
        {
            deleteQueued();


            updateQTs();


            foreach (var b in ActiveBodies)
            {
                //reset forces + torques...
                b.FNET = Vector2.Zero;
                b.TNET = 0.0f;
                
                //add forces + torques...
                foreach (var uf in uforces)
                    b.FNET += uf.GetForce(b);
                foreach (var ut in utorques)
                    b.TNET += ut.GetTorque(b);
            }

            bffields.ForEach(bff =>
                {
                    var pots = QTbodies.Query(bff.AreaInfluenced);
                    pots.Remove(bff.SourceBody);
                    pots.ForEach(b => 
                        {
                            var f = bff.GetForce(b.Position - bff.SourceBody.Position);
                            b.FNET += f;
                            bff.SourceBody.FNET += -f; //newton's third law force
                        });
                }
            );


            iffields.ForEach(iff =>
                {
                    var pots = QTbodies.Query(iff.AreaInfluenced);
                    pots.ForEach(b =>
                        {
                            var f = iff.GetForce(b.Position - iff.SourcePos);
                            b.FNET += f;
                        });
                }
            );
            iffields.Clear();

            iforces.ForEach(iforce => iforce.TargetBody.FNET += iforce.Force);
            iforces.Clear();


            foreach (var b in ActiveBodies)
            {
                var a = b.FNET * b.InvMass();
                
                b.Velocity += a * dt;
                b.Position += b.Velocity * dt;
                
                b.Rotation += b.AngularVelocity * dt;
            }

            CollisionEngine.ResolveAll(ActiveBodies, MapBodies, QTbodies, QTmap);

            int numXtra = 1;
            for (int i = 0; i < numXtra; i++) //extra rounds of collision resolution.
            {
                updateQTs();
                CollisionEngine.ResolveAll(ActiveBodies, MapBodies, QTbodies, QTmap);
            }

            //handle particle systems...
            updateParticleSys(dt);
        }
        void updateParticleSys(float dt)
        {
            PSystems.RemoveAll(ps => ps.IsDead());

            foreach (var psys in PSystems)
            {
                foreach (var part in psys.Particles)
                {
                    part.FNET = Vector2.Zero;
                    part.TNET = 0.0f;

                    foreach (var uf in uforces)
                        part.FNET += uf.GetForce(part);
                    foreach (var ut in utorques)
                        part.TNET += ut.GetTorque(part);
                }
            }



            foreach (var psys in PSystems)
            {
                foreach (var part in psys.Particles)
                {
                    var a = part.FNET * part.InvMass();
                    part.Velocity += a * dt;

                    part.Position += part.Velocity * dt;
                    part.Rotation += part.AngularVelocity * dt;
                }
            }

            PSystems.ForEach(psys => psys.UpdateLives(dt));
        }
        public void AddParticleSystem(ParticleSystem psys)
        {
            PSystems.Add(psys);
        }
        public void AddParticleSystems(IList<ParticleSystem> psys)
        {
            PSystems.AddRange(psys);
        }
        public void SafeDelete(IRigidBody b)
        {
            toDelete.Add(b);
        }
        void deleteQueued()
        {
            while (!toDelete.IsEmpty)
            {
                IRigidBody b;
                if (toDelete.TryTake(out b))
                    ActiveBodies.Remove(b);
                else
                    Console.WriteLine("toDelete bag withdrawal failed. trying again...");
                    
            }
        }
        public void AddUniversalForce(UniversalForce force)
        {
            uforces.Add(force);
        }
        public void RemoveUniversalForce(UniversalForce force)
        {
            uforces.Remove(force);
        }
        public void AddBoundForceField(BoundForceField bff)
        {
            bffields.Add(bff);
        }
        public void RemoveBoundForceField(BoundForceField bff)
        {
            bffields.Remove(bff);
        }
        public void AddInstantaneousForceField(InstantaneousForceField iff)
        {
            iffields.Add(iff);
        }
        public void AddInstantaneousForce(InstantaneousForce iforce)
        {
            iforces.Add(iforce);
        }
    }
}

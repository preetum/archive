using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using Utilities;

namespace PhysicsCore
{
    public class ParticleSystem
    {
        public List<Particle> Particles;
        public float InitRadius;

        Vector2 _cent;

        Texture2D tex;

        public ParticleSystem(Vector2 cent, float blastRad, int numParts, float maxLife, float initRad, float pmass, float maxangspd)
        {
            init(cent, blastRad, numParts, maxLife, initRad, pmass, maxangspd);
        }
        public ParticleSystem(Vector2 cent, float blastRad, int numParts, float maxLife, float initRad, float pmass, float maxangspd, Texture2D tex)
        {
            this.tex = tex;
            init(cent, blastRad, numParts, maxLife, initRad, pmass, maxangspd);
        }
        void init(Vector2 cent, float blastRad, int numParts, float maxLife, float initRad, float pmass, float maxangspd)
        {
            _cent = cent;
            Particles = new List<Particle>();
            InitRadius = initRad;

            for (int i = 0; i < numParts; i++)
            {
                var life = maxLife / 2 + (float)MathUtils.Rand.NextDouble() * maxLife / 2;
                var p = new Particle(InitRadius, pmass, life);
                p.Position = cent + (float)MathUtils.Rand.NextDouble() * blastRad * MathUtils.RandDirection();
                p.Rotation = (float)(MathUtils.Rand.NextDouble() * 2 * Math.PI);
                p.AngularVelocity = (float)(2 * maxangspd * MathUtils.Rand.NextDouble() - maxangspd);
                Particles.Add(p);
            }
        }
        public void InitVelocities(float minVel, float maxVel)
        {
            Random rand = new Random();
            foreach (var p in Particles)
            {
                var n = Vector2.Normalize(p.Position - _cent);
                p.Velocity = (minVel + (maxVel - minVel) * (float)rand.NextDouble()) * n;
            }
        }
        public void UpdateLives(float dt)
        {
            Particles.RemoveAll(p => p.Lifetime > p.MaxLifetime);

            Particles.ForEach(p => p.Radius += dRad(p));

            Particles.ForEach(p => p.Age(dt));
        }
        float dRad(Particle p)
        {
            var l = p.Lifetime / p.MaxLifetime;
            var initRate = InitRadius / 25;
            return initRate * (1 + l) * (1 - l);
        }
        float alpha(Particle p)
        {
            var l = p.Lifetime / p.MaxLifetime;
            var a =  4*l * (1 - l);
            if (a < 0) a = 0;
            if (a > 1) a = 1;
            return a;
        }
        public void Draw()
        {
            if (tex == null)
            {
                foreach (var p in Particles)
                {
                    byte a = (byte)(255 * alpha(p));
                    GraphicsUtils.DrawBall(p.Position, p.Radius, Color.OrangeRed, a);
                }
            }
            else
            {
                Draw(tex);
            }
        }
        public void Draw(Texture2D tex)
        {
            foreach (var p in Particles)
            {
                byte a = (byte)(255 * alpha(p));
                GraphicsUtils.DrawBallTex(tex, p.Position, p.Radius, p.Rotation, a);
            }
        }
        public bool IsDead()
        {
            return Particles.Count == 0;
        }
 
    }


    public class Particle : ICircleBody
    {
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }

        public float DragCoefficient { get; set; }

        public float Radius { get; set; }

        /*
         * IRigidBody Reqs
         */
        public dynamic Tag { get; set; }

        public float Rotation { get; set; }
        public Vector2 Position { get; set; }     

        public virtual bool HasInfiniteMass { get; protected set; }
        public float Mass { get; set; }

        public Vector2 Velocity
        {
            get;
            set;
        }
        public float AngularVelocity { get; set; }
        public float RotationalInertia { get; set; }

        public float Restitution { get; protected set; }

        public Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }
        public Region Span { get { return BoundingBox; } }

        public Vector2 FNET { get; set; }
        public float TNET { get; set; }

        public event CollisionEventHandler Collided;
        public void OnCollided(IRigidBody impactB)
        {
            if (Collided != null)
                Collided(impactB);
        }

        public Particle(float rad, float mass, float maxlife)
        {
            Radius = rad;
            Mass = mass;

            Lifetime = 0.0f;
            MaxLifetime = maxlife;

            DragCoefficient = .5f;
        }
        public void Age(float dt)
        {
            Lifetime += dt;
        }
    }
}

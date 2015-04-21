using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ROIDS;
using PhysicsCore;
using WorldCore;

using Microsoft.Xna.Framework;
namespace ROIDS.Demos.Swarm
{
    class Droid : GameObjects.Actor, ICircleBody, IDynamicObject
    {

        public Droid(Vector2 position, float orientation, int alertness)
            : base(position, orientation)
        {
            Radius = 10;
            normalSightRadiusSq = (float)Math.Pow(70, 2);
            alertSightRadiusSq = (float)Math.Pow(200, 2);
            conversiontRadiusSq = (float)Math.Pow(50, 2);
            
            _activeColor = Color.Orange;
            _angryColor = Color.Red;
            _inactiveColor = Color.Green;

            _chaseSpeed = 1f;
            _driftSpeed = .1f;

            _damage = 1;
            _massOfGreenDroid = 1000;

            Alertness = alertness;

            //CollisionEngine.Collided += new LegacyCollisionEventHandler(CollisionEngine_Collided);

            this.Collided += new CollisionEventHandler(Droid_Collided);
        }



        /*
         * Physics Reqs
         */
        public override PhysicsCore.Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }
        public float Radius { get; set; }


        /*
         * Graphics Reqs
         */
        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            var color = _inactiveColor;
            if (_alertness == 1)
                color = _activeColor;
            else if (_alertness == 2)
                color = _angryColor;
            Utilities.GraphicsUtils.DrawBall(Position, Radius, color, 255);
        }

        Color _activeColor;
        Color _angryColor;
        Color _inactiveColor;
        /*
         * World Reqs
         */
        public void Update(Microsoft.Xna.Framework.GameTime time)
        {
            if (_alertness > 0)
            {
                Map map = ((SwarmGame)GameCore.GameEngine.Singleton.ActiveState).ActiveMap;
                bool foundSomething = false;
                foreach (WorldObject obj in map.WorldObjects)
                {
                    if (this.Alertness == 2 && obj is Droid &&
                       ((Droid)obj).Alertness == 0 && isWithinSight(obj, conversiontRadiusSq))
                        ((Droid)obj).Alertness = 1;

                    else if (obj is Player && isWithinSight(obj, SightRadiusSq))
                    {
                        this.Velocity = pointTo(obj.Position) * _chaseSpeed;
                        Alertness = 2;
                        foundSomething = true;
                    }
                    else if (!foundSomething && obj is Treasure && isWithinSight(obj, SightRadiusSq))
                    {
                        this.Velocity = pointTo(obj.Position) * _driftSpeed;
                        foundSomething = true;
                    }
                }
                if (!foundSomething)
                    Alertness = 1;
            }
        
        }

        private Vector2 pointTo(Vector2 point)
        {
            var x1 = _position.X;
            var y1 = _position.Y;

            var x2 = point.X;
            var y2 = point.Y;

            var hyp = (float)Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));

            return new Vector2((x2 - x1) / hyp, (y2 - y1) / hyp);
        }


        /*
         * Droid Logic
         */
        int _alertness;

        public float SightRadiusSq { get; set; }

        private float normalSightRadiusSq;
        private float alertSightRadiusSq;
        private float conversiontRadiusSq;

        private float _chaseSpeed;
        private float _driftSpeed;

        private int _damage;
        private float _massOfGreenDroid;

        private bool isWithinSight(WorldObject worldObject, float sightRadiusSq)
        {
            return 
                Math.Pow(worldObject.Position.X - _position.X, 2) + Math.Pow(worldObject.Position.Y - _position.Y, 2)
                < sightRadiusSq;
        }

        public int Alertness
        {
            get { return _alertness; }
            set
            {
                _alertness = value;
                Mass = 1;
                SightRadiusSq = normalSightRadiusSq;
                if (_alertness == 0)
                {
                    Mass = _massOfGreenDroid;
                    Velocity = Vector2.Zero;
                    AngularVelocity = 0;
                }
                else if (_alertness == 2) SightRadiusSq = alertSightRadiusSq;
            }
        }



        void Droid_Collided(IRigidBody impactB)
        {
            if (_alertness > 0)
            {
                if (impactB is Player)
                {
                    ((SwarmGame)GameCore.GameEngine.Singleton.ActiveState).PlayerManger.Health -= _damage;
                    Alertness = 0;

                    Map map = ((SwarmGame)GameCore.GameEngine.Singleton.ActiveState).ActiveMap;
                    foreach (WorldObject obj in map.WorldObjects)
                    {
                        if (obj is Droid && ((Droid)obj).Alertness > 0 && isWithinSight(obj, conversiontRadiusSq))
                            ((Droid)obj).Alertness = 0;
                    }
                }
                else if (impactB is Droid)
                    Alertness = 1;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}

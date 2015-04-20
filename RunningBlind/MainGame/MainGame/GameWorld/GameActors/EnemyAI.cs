using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MainGame.GameWorld.GameActors
{

    class Waypoint
    {
        public Vector2 Point;
        public Waypoint NextWaypoint;
        public Waypoint PrevWaypoint;

        public Waypoint(Vector2 pt, Waypoint next, Waypoint prev)
        {
            Point = pt;
            NextWaypoint = next;
            PrevWaypoint = prev;
        }
        public Waypoint(Vector2 pt, Waypoint next)
        {
            Point = pt;
            NextWaypoint = next;
            NextWaypoint.PrevWaypoint = this;
        }
        public Waypoint(Vector2 pt)
        {
            Point = pt;
        }
    }

    enum EnemyState
    {
        Patrolling,
        Attacking
    }

    class EnemyAI
    {
        Waypoint rootWaypt;
        Actor host;

        float speed = 1;

        Waypoint currWaypt;

        EnemyState state = EnemyState.Patrolling;

        bool forward = true;

        DateTime lastActivated;

        float fireAccum = 0f;

        public EnemyAI(Actor host, Waypoint rootWaypt)
        {
            this.host = host;
            this.rootWaypt = rootWaypt;

            host.Position = rootWaypt.Point;
            currWaypt = rootWaypt;
        }

        bool withinThresh(Vector2 pos, Vector2 targ)
        {
            return (pos - targ).LengthSquared() <= speed;
        }

        public void Activate()
        {
            state = EnemyState.Attacking;
            lastActivated = DateTime.Now;
        }

        bool waveActivated()
        {
            foreach (var pulse in host.ParentLevel.pulseMan.pulses)
            {
                if (pulse.PlayerOriginated && (host.Position - pulse.Root).Length() <= pulse.GetRadius(DateTime.Now))
                    return true;
            }
            return false;
        }

        void lookAt(Vector2 targ)
        {
            Vector2 dir = targ - host.Position;
            host.Theta = (float)Math.Atan2(dir.X, -dir.Y);
        }

        void moveToward(Vector2 targ)
        {
            if (withinThresh(host.Position, targ))
                host.Position = targ;
            else
                host.Position += Vector2.Normalize(targ - host.Position) * speed;
        }

        public void Update(Microsoft.Xna.Framework.GameTime time)
        {
            Vector2 dir;
            switch (state)
            {
                case EnemyState.Patrolling:

                    if (forward)
                    {
                        dir = Vector2.Normalize(currWaypt.NextWaypoint.Point - currWaypt.Point);
                        host.Position += dir * speed;

                        if (withinThresh(host.Position, currWaypt.NextWaypoint.Point))
                        {
                            host.Position = currWaypt.NextWaypoint.Point;
                            currWaypt = currWaypt.NextWaypoint;

                            if (currWaypt.NextWaypoint == null)
                                forward = false;
                        }

                        try
                        {
                            lookAt(currWaypt.NextWaypoint.Point);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        dir = Vector2.Normalize(currWaypt.PrevWaypoint.Point - currWaypt.Point);
                        host.Position += dir * speed;

                        if (withinThresh(host.Position, currWaypt.PrevWaypoint.Point))
                        {
                            host.Position = currWaypt.PrevWaypoint.Point;
                            currWaypt = currWaypt.PrevWaypoint;

                            if (currWaypt.PrevWaypoint == null)
                                forward = true;
                        }

                        try
                        {
                            lookAt(currWaypt.PrevWaypoint.Point);
                        }
                        catch
                        {
                        }
                    }

                    


                    if (waveActivated())
                        Activate();
                    


                    break;

                case EnemyState.Attacking:
                    if (waveActivated())
                        Activate(); //update lastActivated
                    if ( (host.ParentLevel.Player.Position - host.Position).Length() > 200 &&  (DateTime.Now - lastActivated).TotalSeconds > 3)
                    {
                        state = EnemyState.Patrolling;
                        fireAccum = 100f;
                    }

                    fireAccum += (float)time.ElapsedGameTime.TotalSeconds;
                    float freq = 1.5f;
                    if (!host.IsDead && fireAccum > freq)
                    {
                        fireAccum = 0;
                        Bullet bul = new Bullet(host, false);
                        bul.Fire(host.ParentLevel.Player.Position);
                    }

                    try
                    {
                        lookAt(host.ParentLevel.Player.Position);
                    }
                    catch
                    {
                    }

                    //todo: fix this to be better
                    float maxLen;
                    if (forward)
                    {
                        dir = Vector2.Normalize(currWaypt.NextWaypoint.Point - currWaypt.Point);
                        maxLen = (currWaypt.NextWaypoint.Point - currWaypt.Point).Length();
                    }
                    else
                    {
                        dir = Vector2.Normalize(currWaypt.PrevWaypoint.Point - currWaypt.Point);
                        maxLen = (currWaypt.PrevWaypoint.Point - currWaypt.Point).Length();
                    }


                    //Vector2 norm = new Vector2(-dir.Y, dir.X);

                    float a = Vector2.Dot(dir, (host.ParentLevel.Player.Position - currWaypt.Point));

                    if (a < 0) a = 0;
                    if (a > maxLen) a = maxLen;

                    Vector2 targ = currWaypt.Point + dir * a;

                    

                    moveToward(targ); 



                    break;
            }

            
        }
    }
}

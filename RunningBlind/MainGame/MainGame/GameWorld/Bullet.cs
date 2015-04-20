using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainGame.GameWorld.GameActors
{
    class BulletGraphic : GraphicOverlay
    {
        private const float Speed = 20;

        private Vector2 velocity;
        private Vector2 Finish;
        private Vector2 Source;
        public BulletGraphic(Vector2 source, Vector2 finish)
        {
            Finish = finish;
            Position = source;
            Source = source;
            velocity = finish - source;
            velocity.Normalize();
            velocity *= Speed;

            this.texture = ResourceManager.Resources["bullet"];

            var dir = Vector2.Normalize(Finish - Source);
            this.Theta = (float)(Math.Atan2(dir.Y, dir.X) + Math.PI/2);
        }
        public override void Update(GameTime time)
        {
            var dir = Vector2.Normalize(Finish - Source);


            if ((Position - Source).Length() > (Finish - Source).Length())
                this.Completed = true;
            if (!this.Completed)
                Position += velocity;
        }
    }
    class Bullet
    {
        BoolField field;
        Actor _parentActor;

        Vector2 position;
        Vector2 aim;

        bool playerOriginated;

        Vector2 endPoint;

        BulletGraphic bulletGraphic;

        bool hitWall = true;
        Actor hitActor = null;

        public Bullet(Actor actor, bool playerOriginated)
        {
            _parentActor = actor;

            field = _parentActor.ParentLevel.levelField;
            position = _parentActor.Position;
            this.playerOriginated = playerOriginated;
        }

        public Vector2 Fire(Vector2 aimPoint)
        {
            aim = aimPoint;

            endPoint = this.FindCollisionPoint();
            
            // Make Ripple
            bulletGraphic = new BulletGraphic(this.position, endPoint);
            bulletGraphic.GraphicCompleted += new GraphicOverlayEventHandler(bulletGraphic_GraphicCompleted);
            _parentActor.ParentLevel.AddGraphicOverlay(bulletGraphic);
            return endPoint;
        }

        void bulletGraphic_GraphicCompleted(GraphicOverlay sender)
        {
            _parentActor.ParentLevel.pulseMan.StartPulse(endPoint, 200, playerOriginated);

            if (!hitWall && !_parentActor.IsDead)
            {
                hitActor.Fired();
            }

            if (hitWall)
                SoundManager.Sonar();
        }
        private float normalize(float Theta)
        {
            if (Theta < 0) return 2 * (float)Math.PI + Theta;
            else return Theta;
        }

        private Vector2 FindCollisionPoint()
        {
            Vector2 point2 = searchBoolFields();
            return point2;

        }




        private Vector2 searchBoolFields()
        {
            Vector2 d = (aim - position);
            d.Normalize();

                Vector2 currP = position;
                float t = 0;
            
            while(currP.X >= 0 && currP.X < field.Width && currP.Y >= 0 && currP.Y < field.Height)
            {
                currP = position + d * t;
                t += 0.1f;

                int x = (int)currP.X;
                int y = (int)currP.Y;

                var v = currP; //cnew Vector2(x, y);
                if (

                    //field.field[(int)x, (int)y] || field.field[(int)x-1, (int)y] || field.field[(int)x+1, (int)y] ||
                    //field.field[(int)x, (int)y+1] || field.field[(int)x, (int)y-1]

                    field.TestIntersect(new Rectangle(x - 2, y - 2, 4, 4))


                    )
                {
                    hitWall = true;
                    return v;
                }
                else
                {
                    foreach (Actor actor in _parentActor.ParentLevel.Actors)
                    {
                        if (actor.CollisionClass != -1 && actor != _parentActor &&
                            (actor.Position - v).Length() < actor.BoundingBox.Width)
                        {
                            hitWall = false;
                            hitActor = actor;
                            return v;

                        }
                    }
                }
            } 
            throw new Exception("Unbounded Map");
        }
    }
}

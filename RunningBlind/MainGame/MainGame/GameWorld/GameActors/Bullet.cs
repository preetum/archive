using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MainGame.GameWorld.GameActors
{
    class Bullet
    {
        BoolField field;
        Actor _parentActor;

        Vector2 position;
        Vector2 aim;
        public Bullet(Actor actor)
        {
            _parentActor = actor;

            field = _parentActor.ParentLevel.levelField;
            position = _parentActor.Position;
            
        }

        public Vector2 Fire(Vector2 aimPoint)
        {
            aim = aimPoint;

            var point = this.FindCollisionPoint();
            
            // Make Ripple
            _parentActor.ParentLevel.pulseMan.StartPulse(point, 200);

            return point;
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
            
            
            for (float y = position.Y, x = position.X; x < (float)field.Width && y < (float)field.Height; x += d.X)
            {
                y += d.Y;
                var v = new Vector2(x, y);
                if (field.field[(int)x, (int)y]) return v;
                else
                {
                    foreach (Actor actor in _parentActor.ParentLevel.Actors)
                    {
                        if (actor.CollisionClass != -1 && actor != _parentActor &&
                            (actor.Position - v).Length() < actor.BoundingBox.Width)
                            return v;
                    }
                }
            } 
            throw new Exception("Unbounded Map");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TrackerXNA
{
    class Camera3D
    {
        public Vector3 Position;

        Vector3 direction;
        public Vector3 Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = Vector3.Normalize(value);
            }
        }

        Vector3 up;
        Vector3 left
        {
            get { return Vector3.Cross(up, Direction); }
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(Position, Position + Direction, up);
            }
        }
        public Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, viewport.AspectRatio, near, far);
            }
        }

        float near = 0.1f;
        float far = 100f;
        public Viewport viewport;
        public Camera3D(Viewport viewport)
        {
            this.viewport = viewport;
            
            Position = Vector3.Zero;
            up = Vector3.UnitY;
            Direction = Vector3.UnitZ;
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }
        public void LookAt(Vector3 targ)
        {
            Direction = Vector3.Normalize(targ - Position);
        }
        public void Yaw(float angle)
        {
            var axis = up;
            Direction = Vector3.Transform(Direction, Matrix.CreateFromAxisAngle(axis, angle));
        }
        public void Pitch(float angle)
        {
            var axis = left;
            up = Vector3.Transform(up, Matrix.CreateFromAxisAngle(axis, angle));
            Direction = Vector3.Transform(Direction, Matrix.CreateFromAxisAngle(axis, angle));
        }

        public void YawAround(Vector3 target, float angle)
        {
            var axis = up;
            Position = Vector3.Transform((Position - target), Matrix.CreateFromAxisAngle(axis, angle)) + target;
            LookAt(target);
        }
        public void PitchAround(Vector3 target, float angle)
        {
            var axis = left;
            Position = Vector3.Transform((Position - target), Matrix.CreateFromAxisAngle(axis, angle)) + target;
            up = Vector3.Transform(up, Matrix.CreateFromAxisAngle(axis, angle));
            LookAt(target);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omron.Framework
{
    public class Camera
    {
        public Viewport baseview;

        public float Zoom;

        public Vector2 Target;
        public float Rotation;

        /// <summary>
        /// the width of the camera's view, in pixels
        /// </summary>
        /// <returns></returns>
        public float GetWidth()
        {
            return baseview.Width / Zoom;
        }
        /// <summary>
        /// the height of the camera's view, in pixels
        /// </summary>
        /// <returns></returns>
        public float GetHeight()
        {
            return baseview.Height / Zoom;
        }

        public Camera(Viewport baseview)
        {
            this.baseview = baseview;
            Zoom = 1.0f;
            Target = Vector2.Zero;
            Rotation = 0.0f;
        }


        public Matrix GetTransform()
        {
            var offset = new Vector2(baseview.Width / 2 + baseview.X, baseview.Height / 2 + baseview.Y);
            return Matrix.CreateTranslation(-Target.X, -Target.Y, 0.0f) * Matrix.CreateScale(Zoom, Zoom, 1.0f) * Matrix.CreateRotationZ(-Rotation) * Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(offset.X, offset.Y, 0.0f);
        }
        public Matrix GetUntransform()
        {
            return Matrix.Invert(GetTransform());
        }
    }
}

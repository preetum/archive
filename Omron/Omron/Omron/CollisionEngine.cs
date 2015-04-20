using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omron
{
    public interface IPolygon
    {
        Vector2[] GetVertices();
        Vector2[] GetAxes();
        float Rotation { get; set; }
        float MaxRadius { get; }
        Vector2 Center { get; set; }
        float Width { get; }
        float Height { get; }
    }

    public class RectPoly : IPolygon
    {
        public Vector2 Center { get; set; }

        Vector2 axis1;
        Vector2 axis2;
        float axis1HalfWidth;
        float axis2HalfWidth;

        public float Width
        {
            get { return axis1HalfWidth * 2f; }
            set { axis1HalfWidth = value / 2f; }
        }
        public float Height
        {
            get { return axis2HalfWidth * 2f; }
            set { axis2HalfWidth = value / 2f; }
        }

        float _mrad;
        public float MaxRadius
        {
            get { return _mrad; }
        }

        float _rot;
        public float Rotation
        {
            get { return _rot; }
            set
            {
                if (_rot == value) return;

                _rot = value;
                computeAxes(_rot);
            }
        }
        void computeAxes(float _rot)
        {
            //axis1 = MathHelper.Rotate(Vector2.UnitX, _rot);
            //axis2 = MathHelper.Rotate(Vector2.UnitY, _rot);
            axis1 = new Vector2((float)Math.Cos(_rot), (float)Math.Sin(_rot));
            axis2 = new Vector2(-(float)Math.Sin(_rot), (float)Math.Cos(_rot));
        }
        public RectPoly(Vector2 cent, float width, float height, float rot)
        {
            Center = cent;
            //axis1HalfWidth = width / 2;
            //axis2HalfWidth = height / 2;
            this.Width = width;
            this.Height = height;

            computeAxes(rot);
            this.Rotation = rot;

            _mrad = (float)Math.Sqrt(width * width / 4 + height * height / 4);
        }
        public RectPoly(Vector2 min, Vector2 max)
            : this((min + max) / 2f, max.X - min.X, max.Y - min.Y, 0.0f)
        {
        }
        public RectPoly(Vector2 min, float width, float height)
            : this(min, min + Vector2.UnitX * width + Vector2.UnitY * height)
        {
        }
        public Vector2[] GetVertices()
        {
            return new Vector2[]{
                Center + axis1 * axis1HalfWidth + axis2 * axis2HalfWidth,
                Center + axis1 * axis1HalfWidth - axis2*axis2HalfWidth,
                Center - axis1 * axis1HalfWidth + axis2 * axis2HalfWidth,
                Center - axis1 * axis1HalfWidth - axis2*axis2HalfWidth
            };
        }
        public Vector2[] GetAxes()
        {
            return new Vector2[] { axis1, axis2 };
        }
    }

    public class HexPoly : IPolygon
    {
        Vector2[] rawVerts;

        public float Rotation { get; set; }
        public Vector2 Center { get; set; }

        public float Width
        {
            get { return 2 * MaxRadius; }
        }
        public float Height
        {
            get { return MathHelper.Constants.sqrt3 * MaxRadius; }
        }

        float _mrad;
        public float MaxRadius
        {
            get { return _mrad; }
        }

        public HexPoly(Vector2 cent, float sideLen, float rot)
        {
            Center = cent;
            Rotation = rot;

            rawVerts = new Vector2[6];
            Vector2 disp = Vector2.UnitX;
            for (int i = 0; i < 6; i++)
            {
                rawVerts[i] = disp * sideLen;
                disp = MathHelper.Rotate(disp, ((float)Math.PI / 3f));
            }

            _mrad = sideLen;
        }

        public Vector2[] GetVertices()
        {
            Vector2[] cookedVerts = new Vector2[6];
            Matrix trans;
            if (Rotation == 0)
                trans = Matrix.CreateTranslation(Center.X, Center.Y, 0.0f);
            else
                trans = Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Center.X, Center.Y, 0.0f);
            Vector2.Transform(rawVerts, ref trans, cookedVerts);
            return cookedVerts;
        }
        public Vector2[] GetAxes()
        {
            Vector2 R, G, B;
            G = MathHelper.Constants.HexAxis1;
            B = MathHelper.Constants.HexAxis2;
            R = MathHelper.Constants.HexAxis3;

            if (Rotation == 0.0f)
                return new Vector2[] { R, G, B };

            var trans = Matrix.CreateRotationZ(Rotation);
            Vector2[] cookedAxes = new Vector2[3];
            Vector2.Transform(new Vector2[] { R, G, B }, ref trans, cookedAxes);
            return cookedAxes;
        }
    }

    public struct Interval
    {
        public float min;
        public float max;
        public Interval(float min, float max)
        {
            this.max = max;
            this.min = min;
        }
        public static bool Intersects(Interval a, Interval b)
        {
            return a.max >= b.min && a.min <= b.max;
        }
    }


    public static class CollisionTester
    {
        public static Interval Project(Vector2[] vertices, Vector2 axis)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (var vert in vertices)
            {
                var d = Vector2.Dot(axis, vert);
                if (d < min) min = d;
                if (d > max) max = d;
            }
            return new Interval(min, max);
        }
        public static bool TestCollision(IPolygon a, IPolygon b)
        {
            if (!TestBroadCollision(a, b)) return false;

            var aVerts = a.GetVertices();
            var bVerts = b.GetVertices();

            var axes = a.GetAxes().Concat(b.GetAxes());
            foreach (var axis in axes)
            {
                Interval intvA = Project(aVerts, axis);
                Interval intvB = Project(bVerts, axis);
                if (!Interval.Intersects(intvA, intvB))
                    return false;
            }
            return true;
        }
        public static bool TestPointInside(Vector2 point, IPolygon poly)
        {
            var verts = poly.GetVertices();
            foreach (var axis in poly.GetAxes())
            {
                Interval polyIntv = Project(verts, axis);
                float ptProject = Vector2.Dot(point, axis);
                if (ptProject < polyIntv.min || ptProject > polyIntv.max)
                    return false;
            }
            return true;
        }
        public static bool TestBroadCollision(IPolygon a, IPolygon b)
        {
            return (a.Center - b.Center).LengthSquared() <= (a.MaxRadius + b.MaxRadius) * (a.MaxRadius + b.MaxRadius);
        }
    }
}

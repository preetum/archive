using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TrackerXNA
{
    public struct VertexPositionNormalColor : IVertexType
    {
        Vector3 position;
        Vector3 normal;
        Color color;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public VertexPositionNormalColor(Vector3 pos, Vector3 norm, Color c)
        {
            position = pos;
            normal = norm;
            color = c;
        }
    }

    class QuadBuilder
    {
        public QuadBuilder()
        {
            verts = new List<VertexPositionNormalColor>(1000000);
            indices = new List<int>(1000000);
        }

        public void Reset()
        {
            verts.Clear();
            indices.Clear();
        }

        List<VertexPositionNormalColor> verts;
        List<int> indices;
        public void AddQuad(Vector3 cent, Vector3 normal, Color color, float semirad)
        {
            normal.Normalize();

            Vector3 up = Vector3.UnitY;

            Vector3 u, v;

            if (normal != up)
            {
                u = Vector3.Normalize(Vector3.Cross(up, normal));
                v = Vector3.Cross(normal, u);
            }
            else
            {
                u = Vector3.UnitZ;
                v = Vector3.UnitX;
            }

            u *= semirad;
            v *= semirad;

            UInt16 i0 = (UInt16)verts.Count;

            verts.Add(new VertexPositionNormalColor(cent - u - v, normal, color));
            verts.Add(new VertexPositionNormalColor(cent + u - v, normal, color));
            verts.Add(new VertexPositionNormalColor(cent + u + v, normal, color));
            verts.Add(new VertexPositionNormalColor(cent - u + v, normal, color));

            indices.Add(i0);
            indices.Add((int)(i0 + 2));
            indices.Add((int)(i0 + 1));

            indices.Add(i0);
            indices.Add((int)(i0 + 3));
            indices.Add((int)(i0 + 2));
        }
        public VertexPositionNormalColor[] GetVertices()
        {
            return verts.ToArray();
        }
        public int[] GetIndices()
        {
            return indices.ToArray();
        }
    }
}

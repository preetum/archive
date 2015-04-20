using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Omron
{
    
    public class HexGrid<T>
    {

        #region constants
        static Vector2 R = new Vector2(0.5f, -MathHelper.Constants.sqrt3 / 2f);
        static Vector2 G = new Vector2(0.5f, MathHelper.Constants.sqrt3 / 2f);
        static Vector2 B = new Vector2(-1.0f, 0.0f);

        //U,V expressed in R,G coordinates
        //static Vector2 U = new Vector2(1, 2);
        //static Vector2 V = new Vector2(2, 1);
        #endregion constants

        float _sideLen;
        public float HexSideLen { get { return _sideLen; } }


        static Matrix Mrg, Mgb, Mbr;

        T[,] Cells;

        public T this[int u, int v]
        {
            get
            {
                return Cells[u, v];
            }
            set
            {
                Cells[u, v] = value;
            }
        }
        public T this[Point uv]
        {
            get { return this[uv.X, uv.Y]; }
            set { this[uv.X, uv.Y] = value; }
        }

        int ulen, vlen;
        public int U_length { get { return ulen; } }
        public int V_length { get { return vlen; } }

        public IEnumerable<Point> GetNeighborUVs(Point root)
        {
            Point[] neighbors = new Point[]{
                new Point(root.X, root.Y + 1),
                new Point(root.X, root.Y - 1),
                new Point(root.X + 1, root.Y),
                new Point(root.X - 1, root.Y),
                new Point(root.X+1, root.Y-1),
                new Point(root.X-1, root.Y+1)
            };

            foreach (var uv in neighbors)
            {
                if (IsValidUV(uv))
                    yield return uv;
            }
        }
        public IEnumerable<T> GetNeighborCells(Point root)
        {
            Point[] neighbors = new Point[]{
                new Point(root.X, root.Y + 1),
                new Point(root.X, root.Y - 1),
                new Point(root.X + 1, root.Y),
                new Point(root.X - 1, root.Y),
                new Point(root.X+1, root.Y-1),
                new Point(root.X-1, root.Y+1)
            };
            
            foreach (var uv in neighbors)
            {
                if (IsValidUV(uv))
                    yield return this[uv];
            }
        }

        /// <summary>
        /// converts a radius from screen/game to hexUV. ie: takes a float radius and outputs an int, representing an approximate hex radius (to be used with GetDiskUV)
        /// </summary>
        /// <param name="sRad"></param>
        /// <returns></returns>
        public int ScreenToUVRad(float sRad)
        {
            float hRad = sRad / HexSideLen;

            // y = floor((x/(sqrt(3)/2)+1)/2)
            return (int)Math.Floor(hRad / MathHelper.Constants.sqrt3 + 1f/2f);
        }

        /// <summary>
        /// gets the points within a radius of a root. (0 rad = only root. 1 rad = root and immediate neighbors)
        /// </summary>
        /// <param name="root"></param>
        /// <param name="rad"></param>
        /// <param name="novalidate">set to true to not check for IsValidUV</param>
        /// <returns></returns> 
        public HashSet<Point> GetDiskUV(Point root, int rad, bool novalidate)
        {
            HashSet<Point> disk = new HashSet<Point>();

            if (novalidate || IsValidUV(root))
                disk.Add(root);
            if (rad == 0)
                return disk;

            for (int dir = 0; dir < 6; dir++)
            {
                Point nextP = root;
                for (int dist = 0; dist < rad; dist++)
                {
                    nextP = GetNext(nextP, dir, 1);
                    if (novalidate || IsValidUV(nextP))
                        disk.Add(nextP);
                    GetLine(nextP, (dir + 1) % 6, rad - 1 - dist, disk, novalidate);
                }
            }
            return disk;
        }

        /// <summary>
        /// returns the cells within a radius of the root
        /// </summary>
        /// <param name="root"></param>
        /// <param name="rad"></param>
        /// <returns></returns>
        public IEnumerable<T> GetDiskCells(Point root, int rad)
        {
            var uvs = GetDiskUV(root, rad, false);
            foreach (var uv in uvs)
                yield return this[uv];
        }

        void GetLine(Point start, int dir, int len, HashSet<Point> array, bool novalidate)
        {
            for (int i = 0; i < len; i++)
            {
                start = GetNext(start, dir, 1);
                if (novalidate || IsValidUV(start))
                    array.Add(start);
            }
        }

        public IEnumerable<Point> GetRadiusUV(Point root, int rad)
        {
            HashSet<Point> radUVs = new HashSet<Point>();

            for (int dir = 0; dir < 6; dir++)
            {
                Point nextP = root;
                for (int dist = 0; dist < rad; dist++)
                {
                    nextP = GetNext(nextP, dir, 1);
                    radUVs.Add(GetNext(nextP, (dir + 1) % 6, rad - 1 - dist));
                }
            }

            return radUVs;
        }

        Point GetNext(Point cur, int dir, int len)
        {
            switch (dir)
            {
                case 0:
                    return new Point(cur.X + len, cur.Y);
                case 1:
                    return new Point(cur.X, cur.Y + len);
                case 2:
                    return new Point(cur.X - len, cur.Y + len);
                case 3:
                    return new Point(cur.X - len, cur.Y);
                case 4:
                    return new Point(cur.X, cur.Y - len);
                default:
                    return new Point(cur.X + len, cur.Y - len);
            }
        }
       
        public IEnumerable<T> GetRadiusCells(Point root, int rad)
        {
            return GetRadiusUV(root, rad).Select(uv => this[uv]);
        }

        public bool IsValidUV(Point uv)
        {
            return uv.X >= 0 && uv.X < this.U_length && uv.Y >= 0 && uv.Y < this.V_length;
        }
        public bool IsValidUV(int u, int v)
        {
            return u >= 0 && u < this.U_length && v >= 0 && v < this.V_length;
        }

        public HexGrid(int usize, int vsize, float sideLen)
        {
            _sideLen = sideLen;
            Cells = new T[usize, vsize];
            ulen = usize;
            vlen = vsize;

            if (Mrg == default(Matrix))
                initTransforms(); //run once only
        }
 

        static void initTransforms()
        {
            Mrg = getBasisTransform(R, G);
            Mgb = getBasisTransform(G, B);
            Mbr = getBasisTransform(B, R);
        }
        static Matrix getBasisTransform(Vector2 a, Vector2 b)
        {
            Matrix m = Matrix.Identity;
            m.M11 = a.X;
            m.M12 = b.X;
            m.M21 = a.Y;
            m.M22 = b.Y;
            m = Matrix.Transpose(m); //because XNA's Vector2.Transform does rowVector*Matrix, instead of Matrix*columnVector
            return Matrix.Invert(m);
        }
        
        /// <summary>
        /// converts an R2 point in screen space to a Z2 point in R-G (hex) space
        /// </summary>
        /// <param name="screenPt"></param>
        /// <returns></returns>
        Point ScreenToHex(Vector2 screenPt)
        {
            screenPt /= HexSideLen; //normalize so all side lengths are 1

            Point rgPos = new Point();

            //transform screenPt into 3 different bases
            Vector2 Vrg, Vgb, Vbr;
            Vector2.Transform(ref screenPt, ref Mrg, out Vrg); //change to R-G basis
            Vector2.Transform(ref screenPt, ref Mgb, out Vgb); //G-B basis
            Vector2.Transform(ref screenPt, ref Mbr, out Vbr); //B-R basis
            Point Prg = Vrg.Floor();
            Point Pgb = Vgb.Floor();
            Point Pbr = Vbr.Floor();

            if ((Prg.X + Prg.Y) % 3 == 0)
            {
                rgPos = Prg;
            }
            else if ((Pgb.X + Pgb.Y) % 3 == 0)
            {
                rgPos.X = -Pgb.Y;
                rgPos.Y = Pgb.X - Pgb.Y; 
            }
            else if((Pbr.X + Pbr.Y) % 3 == 0)
            {
                rgPos.X = -Pbr.X + Pbr.Y;
                rgPos.Y = -Pbr.X;
            }
            else
            {
                //might be on an intersection point
                //throw new Exception("spanish inquisition");
                rgPos = Prg; //default to Prg
            }

            return rgPos;
        }


        /// <summary>
        /// converts a Z2 point in R-G (hex) space to an R2 point in screen space 
        /// </summary>
        /// <param name="hexPt"></param>
        /// <returns></returns>
        Vector2 HexToScreen(Point hexPt)
        {
            return HexSideLen * (R * hexPt.X + G * hexPt.Y);
        }

        Point HexToUV(Point hexPt)
        {
            Point uv = new Point();
            //transform using inverse of U,V column matrix.
            uv.X = -hexPt.X + 2 * hexPt.Y;
            uv.Y = 2 * hexPt.X - hexPt.Y;
            uv.X /= 3;
            uv.Y /= 3;

            return uv;
        }
        Point UVToHex(Point uvPoint)
        {
            return new Point(uvPoint.X + 2 * uvPoint.Y, 2 * uvPoint.X + uvPoint.Y);
        }


        public Vector2 UVToScreen(Point uvPoint)
        {
            return HexToScreen(UVToHex(uvPoint));
        }
        public Point ScreenToUV(Vector2 screenPt)
        {
            return HexToUV(ScreenToHex(screenPt));
        }
    }
}

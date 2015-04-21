using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

namespace PhysicsCore
{
    public struct Region
    {
        public float XMin, XMax, YMin, YMax;

        public float Width { get { return XMax - XMin; } }
        public float Height { get { return YMax - YMin; } }

        public Region(float xmin, float xmax, float ymin, float ymax)
        {
            XMin = xmin;
            XMax = xmax;
            YMin = ymin;
            YMax = ymax;
        }

        public static Region FromCircle(Vector2 cent, float rad)
        {
            return new Region(cent.X - rad, cent.X + rad, cent.Y - rad, cent.Y + rad);
        }

        public bool FullyContains(Region r)
        {
            return r.XMin > this.XMin &&
                r.XMax < this.XMax &&
                r.YMin > this.YMin &&
                r.YMax < this.YMax;
        }

        public bool Intersects(Region r)
        {
            return r.YMax >= this.YMin &&
                r.XMax >= this.XMin &&
                r.YMin <= this.YMax &&
                r.XMin <= this.XMax;
        }
        /// <summary>
        /// corrects xmin > xmax type errors
        /// </summary>
        public void FixBoundOrder()
        {
            swapIfNeeded(ref XMin, ref XMax);
            swapIfNeeded(ref YMin, ref YMax);
        }
        private void swapIfNeeded(ref float min, ref float max)
        {
            if (min > max)
            {
                var t = min;
                min = max;
                max = t;
            }
        }

        public Region Q1
        {
            get { return new Region((XMin + XMax) / 2, XMax, YMin, (YMin + YMax) / 2); }
        }
        public Region Q2
        {
            get { return new Region(XMin, (XMin + XMax) / 2, YMin, (YMin + YMax) / 2); }
        }
        public Region Q3
        {
            get { return new Region(XMin, (XMin + XMax) / 2, (YMin + YMax) / 2, YMax); }
        }
        public Region Q4
        {
            get { return new Region((XMin + XMax) / 2, XMax, (YMin + YMax) / 2, YMax); }
        }


        public Vector2 V1
        {
            get { return new Vector2(XMax, YMin); }
        }
        public Vector2 V2
        {
            get { return new Vector2(XMin, YMin); }
        }
        public Vector2 V3
        {
            get { return new Vector2(XMin, YMax); }
        }
        public Vector2 V4
        {
            get { return new Vector2(XMax, YMax); }
        }

    }

    public interface IRegion
    {
        Region Span { get; }
    }


    public class QuadTree<T> where T:IRegion
    {
        public Region Span;
        public List<T> Nodes;
        public QuadTree<T>[] SubTrees;

        public int MaxBucket;
        public int MaxDepth;

        public bool IsPartitioned
        {
            get { return SubTrees != null; }
        }

        public QuadTree(Region span, int maxbucket, int maxdepth)
        {
            Span = span;
            Nodes = new List<T>();

            MaxBucket = maxbucket;
            MaxDepth = maxdepth;
        }

        /// <summary>
        /// returns the quadrant of span that entirely contains test. if none, return 0.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private int partition(Region span, Region test)
        {
            if (span.Q1.FullyContains(test)) return 1;
            if (span.Q2.FullyContains(test)) return 2;
            if (span.Q3.FullyContains(test)) return 3;
            if (span.Q4.FullyContains(test)) return 4;

            return 0;
        }

        public void AddNode(T node)
        {


            if (! IsPartitioned)
            {

                if (Nodes.Count >= MaxBucket && MaxDepth > 0) //bin is full and can still subdivide
                {
                    //
                    //partition into quadrants and sort existing nodes amonst quads.
                    //
                    Nodes.Add(node); //treat new node just like other nodes for partitioning

                    SubTrees = new QuadTree<T>[4];
                    SubTrees[0] = new QuadTree<T>(this.Span.Q1, MaxBucket, MaxDepth - 1);
                    SubTrees[1] = new QuadTree<T>(this.Span.Q2, MaxBucket, MaxDepth - 1);
                    SubTrees[2] = new QuadTree<T>(this.Span.Q3, MaxBucket, MaxDepth - 1);
                    SubTrees[3] = new QuadTree<T>(this.Span.Q4, MaxBucket, MaxDepth - 1);

                    var remNodes = new List<T>(); //nodes that are not fully contained by any quadrant

                    foreach (var n in Nodes)
                    {
                        switch (partition(this.Span, n.Span))
                        {
                            case 1: //quadrant 1
                                SubTrees[0].AddNode(n);
                                break;
                            case 2:
                                SubTrees[1].AddNode(n);
                                break;
                            case 3:
                                SubTrees[2].AddNode(n);
                                break;
                            case 4:
                                SubTrees[3].AddNode(n);
                                break;
                            case 0:
                                remNodes.Add(n);
                                break;
                        }
                    }

                    Nodes = remNodes;
                }
                else
                {
                    Nodes.Add(node); //if bin is not yet full or max depth has been reached, just add the node without subdividing
                }

            }
            else //we already have children nodes
            {
                //
                //add node to specific sub-tree
                //
                switch (partition(this.Span, node.Span))
                {
                    case 1: //quadrant 1
                        SubTrees[0].AddNode(node);
                        break;
                    case 2:
                        SubTrees[1].AddNode(node);
                        break;
                    case 3:
                        SubTrees[2].AddNode(node);
                        break;
                    case 4:
                        SubTrees[3].AddNode(node);
                        break;
                    case 0:
                        Nodes.Add(node);
                        break;

                }
            }
     

        }

        public void QueryR(Region searchR, ref List<T> hits)
        {
            if (searchR.FullyContains(this.Span))
            {
                GetAllNodesR(ref hits);
            }
            else if (searchR.Intersects(this.Span))
            {
                foreach (var n in Nodes)
                    if (searchR.Intersects(n.Span)) hits.Add(n);

                if (IsPartitioned)
                    foreach (var st in SubTrees) st.QueryR(searchR, ref hits);
            }
        }
        public List<T> Query(Region searchR)
        {
            var hits = new List<T>();
            QueryR(searchR, ref hits);
            return hits.ToList();
        }

        public void GetAllNodesR(ref List<T> nodes)
        {
            foreach (var n in Nodes) nodes.Add(n);

            if (IsPartitioned)
                foreach (var st in SubTrees) st.GetAllNodesR(ref nodes);
        }


    }
}

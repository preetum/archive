using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using MathNet.Numerics.LinearAlgebra.Double;

namespace KTLib
{
    public class BallDetector
    {
        public Image<Bgr, byte> DetectorOverlay;
        public Image<Bgr, byte> debugOut;


        double avgDepth(ushort[] depthFrameMM, int xcent, int ycent, int rad)
        {
            int w = KinectInterface.w;
            int h = KinectInterface.h;

            int num = 0;
            int sum = 0;
            for (int x = xcent - rad; x <= xcent + rad; x++)
                for (int y = ycent - rad; y <= ycent + rad; y++)
                {
                    if (x < w && x >= 0 && y < h && y >= 0)
                    {
                        int i = x + w * y;

                        ushort depth = depthFrameMM[i];
                        if (depth != ushort.MaxValue)
                        {
                            sum += depth;
                            num++;
                        }
                    }
                }
            return (double)sum / num;
        }


        public List<Ball3D> Detect(KinectInterface kinect)
        {
            float threshDepth = 2.7f;
            float expectedRadius = 0.0315f;
            float radThres = 0.01f;

            var balls = new List<Ball3D>();

            int w = KinectInterface.w;
            int h = KinectInterface.h;
            int sw = w / 2;
            int sh = h / 2;

            byte depthByte = (byte)((int)threshDepth*1000 >> 4);

            var dsmall = kinect.FullDepth.PyrDown();
            var depthMask = dsmall.CopyBlank();

            CvInvoke.cvThreshold(dsmall.Ptr, depthMask.Ptr, depthByte, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY_INV);

            var depthMaskBlock = depthMask.Erode(1).Dilate(1);
            var depthMaskOverlay = depthMaskBlock.Convert<Bgr, Byte>();

            var edges = depthMaskBlock.Canny(new Gray(180), new Gray(120));

            debugOut = edges.Convert<Bgr, Byte>();

            MemStorage storage = new MemStorage(); //allocate storage for contour approximation

            for (Contour<System.Drawing.Point> contours = edges.FindContours(); contours != null; contours = contours.HNext)
            {
                //var ptsRaw = contours.Select(pt => new System.Drawing.PointF(pt.X, pt.Y)).ToArray();
                //var centroid = new System.Drawing.PointF(
                //    ptsRaw.Sum(p => p.X) / ptsRaw.Length,
                //    ptsRaw.Sum(p => p.Y) / ptsRaw.Length); //TODO: fix this method to be actually correct

                //var cPts = ptsRaw.Select(p => new System.Drawing.PointF(
                //    p.X - centroid.X,
                //    p.Y - centroid.Y)).ToArray();


                int bbxcent = contours.BoundingRectangle.X + contours.BoundingRectangle.Width / 2;
                int bbycent = contours.BoundingRectangle.Y + contours.BoundingRectangle.Height / 2;
                byte bbcentVal = depthMaskBlock.Data[bbycent, bbxcent, 0];


                int minDim = Math.Min(contours.BoundingRectangle.Width, contours.BoundingRectangle.Height);
                if (bbcentVal == 255 && minDim > 5)//contour is filled in & greater than some pixel size
                {
                    //var defects = approxContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                    MCvBox2D box = contours.GetMinAreaRect(storage);

                    float xc = (box.center.X * (w / sw));
                    float yc = (box.center.Y * (h / sh));
                    float rMin = (w / sw)*(float)(Math.Min(box.size.Width, box.size.Height) / 2);

                    double dAvg = avgDepth(kinect.depthMM, (int)xc, (int)yc, (int)(rMin * 2 / 3));

                    double zproj = 0.001 * dAvg; //in meters


                    //project
                    var projectedPosV3 = kinect.UnprojectDepth((float)dAvg, xc, yc);

                    var projectedBound = kinect.UnprojectDepth((float)dAvg, xc + rMin, yc);
                    float actualRadius = (projectedPosV3 - projectedBound).Length();

                    if (actualRadius < expectedRadius + radThres && actualRadius > expectedRadius - radThres)
                    {
                        //RotationMatrix2D<float> rot = new RotationMatrix2D<float>(new System.Drawing.PointF(0, 0), box.angle, 1); //not -box.angle, because stupidly matrix rotations are counter-clockwise but box.angle is measured clockwise...
                        //rot.RotatePoints(cPts);

                        //var cnormPts = cPts.Select(p => new System.Drawing.PointF(
                        //    p.X / (box.size.Width / 2),
                        //    p.Y / (box.size.Height / 2)));


                        //var variance = cnormPts.Sum(p =>
                        //{
                        //    var d = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                        //    return (d - 1) * (d - 1);
                        //}) / cnormPts.Count();



                        //if (variance * rApprox * rApprox < 2.5f)
                        if (contours.Area >= box.size.Width * box.size.Height * Math.PI / 4 * 0.9)
                        {
                            Emgu.CV.Structure.Ellipse ellipse = new Emgu.CV.Structure.Ellipse(box.center, new System.Drawing.SizeF(box.size.Height, box.size.Width), box.angle);
                            depthMaskOverlay.Draw(ellipse, new Bgr(0, 255, 0), 3);
                            depthMaskOverlay.Draw(new Cross2DF(box.center, 10, 10), new Bgr(0, 0, 255), 1);


                            Ball3D ball = new Ball3D() { Position = projectedPosV3.ToLinV(), Radius = actualRadius };
                            balls.Add(ball);

                        }
                        else
                        {
                            //depthMaskOverlay.Draw(box, new Bgr(0, 0, 255), 2);
                        }
                    }

                }
            }

            storage.Dispose();

            DetectorOverlay = depthMaskOverlay;

            return balls;
        }
    }
}

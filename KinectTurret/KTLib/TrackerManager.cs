using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;

namespace KTLib
{
    public class TrackerManager
    {
        KinectInterface kinect;
        public BallDetector detector;
        BallTracker tracker;

        public Image<Bgr, byte> DisplayOut;

        public bool Ready = false;

        public BallTrackData ActiveBall;

        public TrackerManager(KinectInterface kinect)
        {
            this.kinect = kinect;
            kinect.OnDepthFrame += new Action(kinect_OnDepthFrame);

            detector = new BallDetector();
            tracker = new BallTracker();
        }

        void kinect_OnDepthFrame()
        {
            DateTime time = DateTime.Now;

            if (!Ready)
            {
                Console.WriteLine("");
            }
            var balls = detector.Detect(kinect);
            foreach (var ball in balls)
            {
                Frame3D frame = new Frame3D() { Ball = ball, Time = time };
                tracker.Push(frame);
            }

            Ready = false;

            DisplayOut = detector.DetectorOverlay.Copy();
            drawPrediction();

            Ready = true;
        }

        public void Update()
        {
            tracker.Update();

            ActiveBall = tracker.GetMostActiveBallProj();
        }

        void drawPrediction()
        {
            if (ActiveBall != null)
            {
                List<System.Drawing.Point> line = new List<System.Drawing.Point>();
                double predictSec = 2;
                for (double t = 0; t < predictSec; t += 0.01)
                {
                    var tsamp = DateTime.Now.AddSeconds(t);
                    Vector2 unproj;

                    if (kinect.ProjectToPx(ActiveBall.ProjFit.PredictPos(tsamp).ToV3(), out unproj)
                        && unproj.X >= 0 && unproj.X < KinectInterface.w
                        && unproj.Y >= 0 && unproj.Y < KinectInterface.h
                        )
                    {
                        unproj *= 0.5f;


                        System.Drawing.PointF pt = new System.Drawing.PointF(unproj.X, unproj.Y);
                        //depthMaskOverlay.Draw(new Cross2DF(pt, 10, 10), new Bgr(0, 0, 255), 3);
                        line.Add(new System.Drawing.Point((int)unproj.X, (int)unproj.Y));
                        //line.Add(new System.Drawing.Point((int)(t*100), (int)(t*100) - 100));
                    }
                }
                DisplayOut.DrawPolyline(line.ToArray(), false, new Bgr(0, 0, 255), 2);

                foreach (var f in ActiveBall.Frames)
                {
                    Vector2 unproj;

                    if (kinect.ProjectToPx(f.Ball.Position.ToV3(), out unproj))
                    {
                        unproj *= 0.5f;

                        var pt = new System.Drawing.Point((int)unproj.X, (int)unproj.Y);

                        DisplayOut.Draw(new Cross2DF(pt, 5, 5), new Bgr(255, 255, 0), 1);
                    }
                }

            }

   
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra.Double;

namespace KTLib
{
    public class ProjManager
    {
        double FOV_x, FOV_y, ElevAngle;
        public ProjManager(double FOV_x, double FOV_y, double elevAngle)
        {
            this.FOV_x = FOV_x;
            this.FOV_y = FOV_y;
            this.ElevAngle = elevAngle;
        }

        public Vector Project(Vector v)
        {
            var projM = ProjZ(v[2]);
            var rotM = RotX(ElevAngle);
            var M = rotM * projM;
            return (Vector)(M * v);
        }
        public Vector UnProject(Vector v)
        {
            var unrot = RotX(-ElevAngle);
            var unproj = ProjZ((unrot * v)[2]).Inverse();
            var invM = unproj * unrot;
            return (Vector)(invM * v);
        }

        DenseMatrix ProjZ(double initZ)
        {
            double tmax = FOV_x / 2;
            double pmax = FOV_y / 2;

            DenseMatrix A = new DenseMatrix(3, 3);
            A[0, 0] = initZ * Math.Tan(tmax);
            A[1, 1] = initZ * Math.Tan(pmax);
            A[2, 2] = 1;
            return A;
        }
        DenseMatrix RotX(double angle)
        {
            DenseMatrix A = new DenseMatrix(3, 3);
            A[0, 0] = 1;
            A[1, 1] = Math.Cos(angle);
            A[1, 2] = -Math.Sin(angle);
            A[2, 1] = Math.Sin(angle);
            A[2, 2] = Math.Cos(angle);
            return A;
        }
    }
    public struct Ball3D
    {
        public Vector Position;
        public float Radius;
    }
    public struct Frame3D
    {
        public Ball3D Ball;
        public DateTime Time;
    }
    public interface IProjFit
    {
        bool Trained { get; }
        Vector PredictPos(DateTime time);
        void Train(List<Frame3D> frames);
        double CalcError();
    }
    public class ProjFitLin : IProjFit
    {
        public double x0, y0, z0, vx, vy, vz;
        public DateTime BaseTime;

        public bool Trained { get { return frames != null; } }

        List<Frame3D> frames;

        public Vector PredictPos(DateTime time)
        {
            Vector p = new DenseVector(3);
            double t = (time - BaseTime).TotalSeconds;

            p[0] = x0 + vx * t;
            p[1] = y0 + vy * t;
            p[2] = z0 + vz * t;
            return p;
        }
        public void Train(List<Frame3D> frames)
        {
            this.frames = frames;
            this.BaseTime = frames.Min(f => f.Time);
            var btime = BaseTime;

            var xframes = frames.Select(f => new Tuple<double, double>(f.Ball.Position[0], (f.Time - btime).TotalSeconds)).ToList();
            var yframes = frames.Select(f => new Tuple<double, double>(f.Ball.Position[1], (f.Time - btime).TotalSeconds)).ToList();
            var zframes = frames.Select(f => new Tuple<double, double>(f.Ball.Position[2], (f.Time - btime).TotalSeconds)).ToList();

            var xfit = MathHelper.LinFit(xframes);
            var zfit = MathHelper.LinFit(zframes);
            var yfit = MathHelper.LinFit(yframes);

            this.x0 = xfit.x0;
            this.vx = xfit.vx;
            this.y0 = yfit.x0;
            this.vy = yfit.vx;
            this.z0 = zfit.x0;
            this.vz = zfit.vx;
        }
        public double CalcError()
        {
            return frames.Sum(f => (PredictPos(f.Time) - f.Ball.Position).Norm(2));
        }
    }
    public class ProjFitQuad : IProjFit
    {
        public double x0, y0, z0, vx, vy, vz;
        public DateTime BaseTime;

        public bool Trained { get { return frames != null; } }

        List<Frame3D> frames;

        public Vector PredictPos(DateTime time)
        {
            Vector p = new DenseVector(3);
            double t = (time - BaseTime).TotalSeconds;

            p[0] = x0 + vx * t;
            p[1] = y0 + vy * t + MathHelper.g / 2 * t * t;
            p[2] = z0 + vz * t;
            return p;
        }


        public void Train(List<Frame3D> frames)
        {
            this.frames = frames;

            this.BaseTime = frames.Min(f => f.Time);
            var btime = BaseTime;

            var xframes = frames.Select(f => new Tuple<double, double>(f.Ball.Position[0], (f.Time - btime).TotalSeconds)).ToList();
            var yframes = frames.Select(f => new Tuple<double, double>(f.Ball.Position[1], (f.Time - btime).TotalSeconds)).ToList();
            var zframes = frames.Select(f => new Tuple<double, double>(f.Ball.Position[2], (f.Time - btime).TotalSeconds)).ToList();

            var xfit = MathHelper.LinFit(xframes);
            var zfit = MathHelper.LinFit(zframes);
            var yfit = MathHelper.QFit(yframes);

            this.x0 = xfit.x0;
            this.vx = xfit.vx;
            this.y0 = yfit.x0;
            this.vy = yfit.vx;
            this.z0 = zfit.x0;
            this.vz = zfit.vx;
        }
      
        public double CalcError()
        {
            return frames.Sum(f => (PredictPos(f.Time) - f.Ball.Position).Norm(2));
        }


    }
    struct FitParams
    {
        public double x0, vx;
    }

    public class BallTrackData
    {
        public List<Frame3D> Frames;
        public IProjFit ProjFit;
        public int ID;
        public BallTrackData()
        {
            Frames = new List<Frame3D>();
        }
    }

    public class BallTracker
    {

        const double distThresh = 0.200; //.1
        const double timeThresh = 2.0; //.5
        const double timeThresh2 = 0.25;
        const int minPointForInterp = 5;

        const double minActivity = .5;

        public List<BallTrackData> trackData;
        public BallTracker()
        {
            trackData = new List<BallTrackData>();
        }

        void retrainAll()
        {
            foreach (var btd in trackData)
            {
                if (btd.ProjFit != null)
                    btd.ProjFit.Train(btd.Frames);
            }
        }
        int i = 0;
        public void Update()
        {
            //purge old frames
            foreach (var btd in trackData)
            {
                btd.Frames.RemoveAll(f => (DateTime.Now - f.Time).TotalSeconds > timeThresh);
            }
            trackData.RemoveAll(d => d.Frames.Count == 0);

            retrainAll();
        }
        public void Push(Frame3D frame)
        {
            //try to fit to existing frames

            foreach (var btd in trackData)
            {

                Vector ppos;
                double dThresh = 0.0f;

                if (btd.ProjFit != null)
                {
                    ppos = btd.ProjFit.PredictPos(frame.Time); //use predicted pos

                    if (btd.ProjFit is ProjFitQuad)
                    {
                        dThresh = distThresh;
                    }
                    else if (btd.ProjFit is ProjFitLin)
                    {
                        dThresh = distThresh * 1;
                    }
                }
                else
                {
                    ppos = btd.Frames.Last().Ball.Position; //just use the only known pos
                    dThresh = distThresh * 1;
                }


                double dist = (ppos - frame.Ball.Position).Norm(2);
                if (dist <= dThresh)
                {
                    btd.Frames.Add(frame);

                    if (btd.Frames.Count >= minPointForInterp) //if we have a decent # of data points
                    {
                        btd.ProjFit = new ProjFitQuad();
                        btd.ProjFit.Train(btd.Frames);
                    }
                    else if (btd.Frames.Count >= 3)
                    {
                        btd.ProjFit = new ProjFitLin();
                        btd.ProjFit.Train(btd.Frames.OrderByDescending(f => f.Time.Ticks).Take(2).ToList());
                    }

                    return;
                }


                

            }


            //ball not found in predictions of existing balls --  must be new 
            BallTrackData new_btd = new BallTrackData();
            new_btd.ID = i++;
            new_btd.Frames.Add(frame);
            trackData.Add(new_btd);
        }

        double ballTrackDataActivity(BallTrackData btd)
        {
            Vector lastPos = btd.Frames.First().Ball.Position;
            double dsum = 0;
            foreach (var f in btd.Frames)
            {
                dsum += (f.Ball.Position - lastPos).Norm(2);
                lastPos = f.Ball.Position;
            }
            return dsum;
        }
        public BallTrackData GetMostActiveBallProj()
        {
            return trackData
                .Where(td => td.ProjFit != null && (DateTime.Now - td.Frames.Last().Time).TotalSeconds < timeThresh2)
                .Where(td => td.Frames.Count > 5)
                .Where(btd => ballTrackDataActivity(btd) / (btd.Frames.Max(f => f.Time) - btd.Frames.Min(f => f.Time)).TotalSeconds > minActivity)
                .OrderByDescending(btd => ballTrackDataActivity(btd))
                .FirstOrDefault();
        }


    }

}

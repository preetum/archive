using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace KTLib
{
    static class MathHelper
    {
        public static double g = -9.8;

        //tuple <data, sec>
        public static FitParams QFit(List<Tuple<double, double>> data)
        {
            FitParams qparams = new FitParams();

            Matrix A = new DenseMatrix(2, 2);
            Vector b = new DenseVector(2);

            var sumTi = data.Sum(d => d.Item2);
            var sumTiSq = data.Sum(d => d.Item2 * d.Item2);
            var sumTiCub = data.Sum(d => d.Item2 * d.Item2 * d.Item2);

            A[0, 0] = data.Count;
            A[0, 1] = sumTi;
            A[1, 0] = sumTi;
            A[1, 1] = sumTiSq;

            b[0] = data.Sum(d => d.Item1) - g / 2 * sumTiSq;
            b[1] = data.Sum(d => d.Item1 * d.Item2) - g / 2 * sumTiCub;

            var pvect = A.Inverse() * b;
            qparams.x0 = pvect[0];
            qparams.vx = pvect[1];
            return qparams;
        }
        //tuple <data, sec>
        public static FitParams LinFit(List<Tuple<double, double>> data)
        {
            FitParams linparams = new FitParams();

            Matrix A = new DenseMatrix(2, 2);
            Vector b = new DenseVector(2);

            var sumTi = data.Sum(d => d.Item2);
            var sumTiSq = data.Sum(d => d.Item2 * d.Item2);

            A[0, 0] = data.Count;
            A[0, 1] = sumTi;
            A[1, 0] = sumTi;
            A[1, 1] = sumTiSq;

            b[0] = data.Sum(d => d.Item1);
            b[1] = data.Sum(d => d.Item1 * d.Item2);

            var pvect = A.Inverse() * b;
            linparams.x0 = pvect[0];
            linparams.vx = pvect[1];
            return linparams;
        }
    }
}

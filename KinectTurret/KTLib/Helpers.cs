using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using MathNet.Numerics.LinearAlgebra.Double;

namespace KTLib
{
    public static class Helpers
    {
        public static Emgu.CV.Image<Gray, Byte> ImageFromArray8(byte[] field8, int w, int h)
        {
            Emgu.CV.Image<Gray, Byte> img = new Emgu.CV.Image<Gray, Byte>(w, h);
            img.Bytes = field8;
            return img;
        }

        public static Texture2D ToTex(this Emgu.CV.Image<Gray, Byte> img, GraphicsDevice gd)
        {
            int w = img.Width;
            int h = img.Height;

            Texture2D tex = new Texture2D(gd, w, h);
            Color[] data = new Color[w * h];

             for (int y = 0; y < h; y++)
                 for (int x = 0; x < w; x++)
                 {
                     byte val = img.Data[y, x, 0];
                     data[x + y * w] = new Color(val, val, val);
                 }

             tex.SetData<Color>(data);
             return tex;
        }

        public static Texture2D ToTex(this Emgu.CV.Image<Bgr, Byte> img, GraphicsDevice gd)
        {
            img = img.Copy();

            int w = img.Width;
            int h = img.Height;

            Texture2D tex = new Texture2D(gd, w, h);

            //byte[] rgba = new byte[w * h * 4];

            //int i = 0;
            //for (int y = 0; y < h; y++)
            //    for (int x = 0; x < w; x++)
            //    {
            //        rgba[i++] = img.Data[y, x, 2];
            //        rgba[i++] = img.Data[y, x, 1];
            //        rgba[i++] = img.Data[y, x, 0];
            //        rgba[i++] = 255;
            //    }

            //tex.SetData<byte>(rgba);


            byte[] rgba = new byte[w * h * 4];

            int i = 0;
            var bytes = img.Bytes;
            for (int k = 0; k < bytes.Length; k += 3)
            {
                rgba[i++] = bytes[k + 2];
                rgba[i++] = bytes[k + 1];
                rgba[i++] = bytes[k + 0];
                rgba[i++] = 255;
            }

            tex.SetData<byte>(rgba);

            return tex;
        }

        public static DenseVector ToLinV(this Vector3 v)
        {
            return new DenseVector(new double[] { v.X, v.Y, v.Z });
        }
        public static Vector3 ToV3(this Vector v)
        {
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }
    }
}

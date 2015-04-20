using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Research.Kinect;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using System.Threading;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;


namespace KTLib
{
    public class KinectInterface
    {
        public Runtime nui;

        Color[] colorFrameData;
        Texture2D colorFrameTex;
        Texture2D depthFrameTex;
        public Texture2D ColorFrameTex
        {
            get { return colorFrameTex; }
        }
        public Texture2D DepthFrameTex
        {
            get { return depthFrameTex; }
        }

        public Image<Gray, Byte> FullDepth;

        public event Action OnDepthFrame;

        public ushort[] depthMM;

        public const int w = 640;
        public const int h = 480;

        GraphicsDevice gd;

        FPSCounter fps;
        public int FPS
        {
            get { return fps.FPS; }
        }

        double theta;

        void initKinect()
        {
            nui = Runtime.Kinects[0];

            try
            {
                nui.Initialize(RuntimeOptions.UseDepth | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }


            try
            {
                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution640x480, ImageType.Depth);
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
            nui.NuiCamera.ElevationAngle = 10;
            theta = nui.NuiCamera.ElevationAngle * Math.PI / 180;
        }

        float tanTmax, tanPmax;
        void initMaths()
        {
            float FOV_x = 57 * (float)(Math.PI / 180);
            float FOV_y = 43 * (float)(Math.PI / 180);
            float tmax = FOV_x / 2;
            float pmax = FOV_y / 2;

            tanTmax = (float)Math.Tan(tmax);
            tanPmax = (float)Math.Tan(pmax);
        }

        public KinectInterface(GraphicsDevice gd)
        {
            this.gd = gd;

            initMaths();

            colorFrameTex = new Texture2D(gd, w, h);
            fps = new FPSCounter();
        }
        public void Start()
        {
            initKinect();
            Ready = false;
        }

        public bool Ready;


        Texture2D KVideoToTex(PlanarImage img)
        {
            Texture2D tex = new Texture2D(gd, img.Width, img.Height);

            colorFrameData = new Color[img.Width * img.Height];

            for (int i = 0; i < colorFrameData.Length; i++)
            {
                colorFrameData[i].R = img.Bits[4 * i + 2];
                colorFrameData[i].G = img.Bits[4 * i + 1];
                colorFrameData[i].B = img.Bits[4 * i];
                colorFrameData[i].A = 255;
            }

            tex.SetData(colorFrameData);
            return tex;
        }
        void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            colorFrameTex = KVideoToTex(e.ImageFrame.Image);
        }


        void processDepthFrame(byte[] depthFrame16)
        {
            depthMM = new ushort[w * h];

            byte[] depth8 = new byte[w * h];
            //for (int i16 = 0, i = 0; i16 < depthFrame16.Length; i16 += 2, i++)
            //{
            //    ushort packet = (ushort)((depthFrame16[i16 + 1] << 8) | depthFrame16[i16]);
            //    ushort depth = (ushort)(0x0FFF & packet);

            //    depthMM[i] = depth;
            //}

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    int ref_i = y * w + w - 1 - x; //reflected x. the depth stream is reflected.

                    ushort packet = (ushort)((depthFrame16[2 * i + 1] << 8) | depthFrame16[2 * i]);
                    ushort depthVal = (ushort)(0x0FFF & packet);

                    depthMM[ref_i] = depthVal;

                    if (depthVal != 0)
                    {
                        depth8[ref_i] = (byte)(depthVal >> 4);
                    }
                    else
                    {
                        depth8[ref_i] = (byte)255;
                    }

                }

            FullDepth = Helpers.ImageFromArray8(depth8, w, h);
        }

        Texture2D generateDepthTex()
        {
            Texture2D tex = new Texture2D(gd, w, h);
            Color[] data = new Color[w * h];

            for (int i = 0; i < data.Length; i++)
            {
                ushort depth = depthMM[i];
                //float intensity = 1 - (float)(depth-800) / (float)0x0fff;

                byte val = (byte)~(depth >> 4);
                Color c = new Color();

                if (depth == 0)
                    c = Color.Gray;
                else
                {
                    // c = Color.Lerp(Color.Black, Color.White, intensity);
                    c.R = val;
                    c.G = val;
                    c.B = val;
                    c.A = 255;
                }

                data[i] = c;
            }

            tex.SetData<Color>(data);
            return tex;
        }

        public Vector3 RotateXCCW(Vector3 v, double theta)
        {
            var vrot = new Vector3();
            vrot.Z = v.Z * (float)Math.Cos(theta) - v.Y * (float)Math.Sin(theta);
            vrot.Y = v.Z * (float)Math.Sin(theta) + v.Y * (float)Math.Cos(theta);
            vrot.X = v.X;
            return vrot;
        }

        public Vector3 UnprojectDepth(float depth_mm, float px, float py)
        {
            float z = depth_mm / 1000f;

            float xnorm = 2f * ((float)px / w - 0.5f); //[-1, 1]
            float ynorm = 2f * -((float)py / h - 0.5f); //[-1, 1]

            //float FOV_x = 57 * (float)(Math.PI / 180);
            //float FOV_y = 43 * (float)(Math.PI / 180);
            //float tmax = FOV_x / 2;
            //float pmax = FOV_y / 2;


            //float xproj = z * xnorm * (float)Math.Tan(tmax);
            //float yproj = z * ynorm * (float)Math.Tan(pmax);

            float xproj = z * xnorm * tanTmax;
            float yproj = z * ynorm * tanPmax;

            var v = new Vector3(xproj, yproj, z);

            //correct for elevation angle
            v = RotateXCCW(v, theta);

            return v;
        }

        public Vector3 UnprojectDepth(int px, int py)
        {
            ushort depth = depthMM[px + py * w];
            if (depth == 0) //no data
                return Vector3.Zero;

            return UnprojectDepth(depth, px, py);
        }
        public bool ProjectToPx(Vector3 v, out Vector2 proj)
        {
            v = RotateXCCW(v, -theta);

            float FOV_x = 57 * (float)(Math.PI / 180);
            float FOV_y = 43 * (float)(Math.PI / 180);
            float tmax = FOV_x / 2;
            float pmax = FOV_y / 2;


            var xnorm = v.X / (v.Z * (float)Math.Tan(tmax));
            var ynorm = v.Y / (v.Z * (float)Math.Tan(pmax));


            float x = (float)(xnorm + 1) / 2 * w;
            float y = (float)(1 - ynorm) / 2 * h;

            proj = new Vector2(x, y);

            return v.Z > 0.0; //invalid if ball behind plane of projection
        }

        public bool GetColorFromDepth(int x, int y, out Color c)
        {
            c = Color.Black;
            int cX, cY;

            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, new ImageViewArea(), x/2, y/2, (short)(depthMM[x + y*w] << 3), out cX, out cY);

            if (cX != -1 && colorFrameData != null && cX < w && cY < h)
            {
                c = colorFrameData[cX + cY * w];
                return true;
            }

            return false;
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            processDepthFrame(e.ImageFrame.Image.Bits);
            depthFrameTex = generateDepthTex();

            if (OnDepthFrame != null)
                OnDepthFrame();

            fps.PushFrame();
            Ready = true;
        }
    }
}

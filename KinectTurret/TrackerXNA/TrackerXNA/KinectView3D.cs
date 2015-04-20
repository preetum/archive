using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KTLib;
using Microsoft.Xna.Framework.Input;


namespace TrackerXNA
{
    class KinectView3D
    {
        GraphicsDevice GraphicsDevice;
        KinectInterface kinect;
        TrackerManager trackMan;
        public Camera3D camera;


        Model sphere;

        VertexPositionNormalColor[] verts;
        int[] indices;

        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;
        BasicEffect effect;

        BasicEffect lineEffect;

        const int res = 8;

        QuadBuilder qb;

        public KinectView3D(GraphicsDevice gd, KinectInterface kinect, TrackerManager trackMan, Model sphere, Rectangle targetRect)
        {
            GraphicsDevice = gd;
            this.trackMan = trackMan;
            this.sphere = sphere;

            Viewport viewp = new Viewport(targetRect);
            camera = new Camera3D(viewp);
            camera.SetPosition(Vector3.UnitZ * 1);
            camera.LookAt(-Vector3.UnitZ);
            
            this.kinect = kinect;

            qb = new QuadBuilder();

            int numQuads = KinectInterface.w * KinectInterface.h / (res * res);
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalColor), numQuads * 4, BufferUsage.None);
            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), numQuads * 6, BufferUsage.None);

            effect = new BasicEffect(GraphicsDevice);
            lineEffect = new BasicEffect(GraphicsDevice);
        }


        Vector3 realToScreen3d(Vector3 realV)
        {
            realV.Z *= -1;
            return realV;
        }

        Vector2 lastPos = Vector2.Zero;
        KeyboardState prevState;
        bool autorot = false;
        bool color = false;

        DateTime baseTime;
        public void Update()
        {

            if (!kinect.Ready)
                return;

            qb.Reset();

            int w = KinectInterface.w;
            int h = KinectInterface.h;


            float rad = .02f; //0.005f;

            for (int y = 0; y < h; y += res)
            {
                for (int x = 0; x < w; x += res)
                {

                    Vector3 posReal = kinect.UnprojectDepth(x, y);

                    if (posReal != Vector3.Zero && posReal.Z < 2.7)
                    {
                        Vector3 pos3D = realToScreen3d(posReal);
                        Color c;

                        if (!color || !kinect.GetColorFromDepth(x, y, out c))
                            c = Color.White;

                        Vector3 normal = Vector3.UnitZ;

                        if (x >= res && y >= res && y < h - res && x < w - res)
                        {
                            Vector3 xp = realToScreen3d(kinect.UnprojectDepth(x + res, y));
                            Vector3 yp = realToScreen3d(kinect.UnprojectDepth(x, y + res));
                            Vector3 xm = realToScreen3d(kinect.UnprojectDepth(x - res, y));
                            Vector3 ym = realToScreen3d(kinect.UnprojectDepth(x, y - res));

                            if (xp != Vector3.Zero && yp != Vector3.Zero && xm != Vector3.Zero && ym != Vector3.Zero)
                            {
                                Vector3 dxp = xp - pos3D;
                                Vector3 dyp = yp - pos3D;
                                Vector3 dxm = xm - pos3D;
                                Vector3 dym = ym - pos3D;

                                Vector3 n1 = Vector3.Normalize(Vector3.Cross(dyp, dxp));
                                Vector3 n2 = Vector3.Normalize(Vector3.Cross(dxp, dym));
                                Vector3 n3 = Vector3.Normalize(Vector3.Cross(dym, dxm));
                                Vector3 n4 = Vector3.Normalize(Vector3.Cross(dxm, dyp));

                                normal = Vector3.Normalize(n1 + n2 + n3 + n4);
                                qb.AddQuad(pos3D, normal, c, rad);
                            }
                        }


                        //qb.AddQuad(pos3D, normal, c, rad);
                    }
                }
            }

            verts = qb.GetVertices();
            indices = qb.GetIndices();

            vertexBuffer.SetData(verts);
            indexBuffer.SetData(indices);


            //

            Vector3 focus = -1.5f * Vector3.UnitZ;

            var mState = Mouse.GetState();
            var mPos = new Vector2(mState.X, mState.Y);

            var del = (mPos - lastPos);

            if (mState.LeftButton == ButtonState.Pressed)
            {
                float yaw = -del.X / 1000;
                float pitch = del.Y / 1000;

                //camera.Yaw(yaw);
                //camera.Pitch(pitch);

                camera.YawAround(focus, yaw);
                camera.PitchAround(focus, pitch);
            }

            var kstate = Keyboard.GetState();

            if (!prevState.IsKeyDown(Keys.R) && kstate.IsKeyDown(Keys.R))
            {
                autorot ^= true;
                if (autorot)
                {
                    baseTime = DateTime.Now;
                }
            }


            color ^= !prevState.IsKeyDown(Keys.C) && kstate.IsKeyDown(Keys.C);

            if (autorot)
            {
                //camera.YawAround(focus, 0.005f);
                double t = (DateTime.Now - baseTime).TotalSeconds;

                var camPos = Vector3.UnitY + 2*Vector3.UnitX * (float)Math.Sin(t * 0.5f) + Vector3.UnitY * (float)Math.Cos(t * 0.5f);
                camera.Position = Vector3.UnitZ + camPos;
                camera.LookAt(focus);
            }

            prevState = kstate;
            lastPos = mPos;
        }


        public void Draw()
        {

            if (!kinect.Ready)
                return;

            var prevPort = GraphicsDevice.Viewport;
            GraphicsDevice.Viewport = camera.viewport;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            effect.World = Matrix.Identity;
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            effect.VertexColorEnabled = true;
            effect.EnableDefaultLighting();
            //effect.LightingEnabled = true;
            //effect.AmbientLightColor = Color.White.ToVector3();
            //effect.DiffuseColor = Color.White.ToVector3();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verts.Length, 0, indices.Length / 3);
            }


            if (trackMan.ActiveBall != null)
            {
                var ball = trackMan.ActiveBall.Frames.Last().Ball;

                var pos = trackMan.ActiveBall.ProjFit.PredictPos(DateTime.Now).ToV3();

                //drawBall(ball.Position.ToV3(), ball.Radius);
                drawBall(pos, ball.Radius);

                drawTracksFancy(trackMan.ActiveBall);
            }

            GraphicsDevice.Viewport = prevPort;

        }

        void drawTracks(BallTrackData activeBallData)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            var baseTime = activeBallData.Frames.First().Time;
            var end = DateTime.Now.AddSeconds(1);
            for (DateTime t = baseTime; t < end; t = t.AddSeconds(0.01f))
            {
                var pos = realToScreen3d(activeBallData.ProjFit.PredictPos(t).ToV3());
                vertices.Add(new VertexPositionColor(pos, Color.Red));
            }

            var varr = vertices.ToArray();
            var indices = Enumerable.Range(0, varr.Length).ToArray();

            lineEffect.World = Matrix.Identity;
            lineEffect.View = camera.View;
            lineEffect.Projection = camera.Projection;
            lineEffect.VertexColorEnabled = true;
            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, varr, 0, varr.Length, indices, 0, varr.Length - 1);
            }
        }
        void drawTracksFancy(BallTrackData activeBallData)
        {
            List<VertexPositionColor>[] vertices = new List<VertexPositionColor>[5];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new List<VertexPositionColor>();

            int numTrails = vertices.Length - 1;

            var rad = activeBallData.Frames.Last().Ball.Radius;

            var baseTime = activeBallData.Frames.First().Time;
            var end = DateTime.Now.AddSeconds(0.5);

            for (DateTime t = baseTime; t < end; t = t.AddSeconds(0.01f))
            {
                var pos = realToScreen3d(activeBallData.ProjFit.PredictPos(t).ToV3());

                var posNext = realToScreen3d(activeBallData.ProjFit.PredictPos(t.AddSeconds(0.01f)).ToV3());
                
                Color c;

                if (t < DateTime.Now)
                    c = Color.LimeGreen;
                else
                    c = Color.Red;

                for (int i = 0; i < numTrails; i++)
                {
                    var dir = Vector3.Normalize(posNext - pos);
                    var u = Vector3.Normalize(Vector3.Cross(dir, Vector3.UnitY));
                    var v = Vector3.Normalize(Vector3.Cross(u, dir));

                    var theta = (2 * Math.PI / numTrails * i);
                    var disp = (float)Math.Sin(theta) * u + (float)Math.Cos(theta) * v;

                    var posMod = pos + disp * rad;
                    vertices[i].Add(new VertexPositionColor(posMod, c));
                }

                vertices[vertices.Length-1].Add(new VertexPositionColor(pos, Color.Red));
            }

            var indices = Enumerable.Range(0, vertices[0].Count()).ToArray();

            lineEffect.World = Matrix.Identity;
            lineEffect.View = camera.View;
            lineEffect.Projection = camera.Projection;
            lineEffect.VertexColorEnabled = true;
            foreach (EffectPass pass in lineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var verts in vertices)
                {
                    var varr = verts.ToArray();
                    GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, varr, 0, varr.Length, indices, 0, varr.Length - 1);
                }
            }
        }

        void drawBall(Vector3 position, float radius)
        {
            position = realToScreen3d(position);
            float scale = 1.2f;

            foreach (ModelMesh mesh in sphere.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = Color.YellowGreen.ToVector3();
                    effect.DiffuseColor = Color.YellowGreen.ToVector3();

                    effect.World = Matrix.CreateScale(scale * radius / mesh.BoundingSphere.Radius) * Matrix.CreateTranslation(position);
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }
    }
}

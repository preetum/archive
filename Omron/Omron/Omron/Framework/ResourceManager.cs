using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Omron.Actors;

namespace Omron.Framework
{
    public interface ISpriteDrawable
    {
        void Draw(SpriteBatch spriteBatch);
    }

    public struct OffsetData
    {
        public float WidthScale;
        public float HeightScale;
        public Vector2 CenterOffset;

        public OffsetData(float wscale, float hscale, Vector2 centerOffset)
        {
            WidthScale = wscale;
            HeightScale = hscale;
            CenterOffset = centerOffset;
        }
    }

    public struct FrameData
    {
        public Texture2D Image;
        public float SclWidth, SclHeight;
        public OffsetData Offset;

        public void SetScale(float scl)
        {
            SclWidth = scl;
            SclHeight = scl;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos, float rot, Color col, float depth)
        {
            var tex = Image;
            var sclX = 1f / SclWidth;
            sclX *= Offset.WidthScale;
            var sclY = 1f / SclHeight;
            sclY *= Offset.HeightScale;
            spriteBatch.Draw(tex, pos + Offset.CenterOffset, null, col, rot,
                new Vector2(tex.Width / 2f, tex.Height / 2f), new Vector2(sclX, -sclY), SpriteEffects.None, depth); 
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 pos, float rot, Color col)
        {
            Draw(spriteBatch, pos, rot, col, 0.2f);
        }

        public FrameData(Texture2D img, float sclWidth, float sclHeight, OffsetData ofstScl)
        {
            Image = img;
            SclWidth = sclWidth;
            SclHeight = sclHeight;
            Offset = ofstScl;
        }

        public FrameData(Texture2D img, float sclWidth, float sclHeight)
            :this(img, sclWidth, sclHeight,new OffsetData(1, 1, Vector2.Zero))
        {
        }
        public FrameData(Texture2D img, float imgScl)
            : this(img, imgScl, imgScl )
        {
        }

    }
    public class SpriteData
    {
        public FrameData SelectedFrame;
        public FrameData UnselectedFrame;
        public SpriteData(FrameData selected, FrameData unselected)
        {
            SelectedFrame = selected;
            UnselectedFrame = unselected;
        }
    }

    public static class ResourceManager
    {
        public static Game BaseGame;

        public static GraphicsDevice GraphicsDevice;
        public static ContentManager ContentManager;
        public static GraphicsDeviceManager GraphicsDeviceManager;

        public static Dictionary<string, dynamic> Resources;

        public static void Init(GraphicsDevice gdevice, ContentManager cman, GraphicsDeviceManager gman, Game baseGame)
        {
            GraphicsDevice = gdevice;
            ContentManager = cman;
            GraphicsDeviceManager = gman;
            Resources = new Dictionary<string, dynamic>();
            BaseGame = baseGame;
        }

        public static void AddRange<TK, TV>(this Dictionary<TK, dynamic> baseDict, Dictionary<TK, TV> concatDict)
        {
            foreach (var kvp in concatDict)
                baseDict.Add(kvp.Key, kvp.Value);
        }
        public static void Load()
        {
            Resources["pixel"] = GeneratePixelTex(Color.White);
            Resources["hexStroke"] = ContentManager.Load<Texture2D>("hexStroke");
            Resources["hex"] = ContentManager.Load<Texture2D>("hex");
            Resources["disk"] = ContentManager.Load<Texture2D>("disk");
            Resources.Add("Settings", UnitsAndBuilduings.Loader.LoadSettings(ContentManager, "Settings"));

            //load resources
            loadFolder<SpriteFont>("Fonts");
            loadFolder<Texture2D>("UI");
            loadVictories("Victories");

            Resources.AddRange(UnitsAndBuilduings.Loader.LoadUnits(ContentManager));
            Resources.AddRange(UnitsAndBuilduings.Loader.LoadBuildings(ContentManager));

            loadFoldersArray<Texture2D>("Buildings\\Animations");
            loadFoldersArray<Texture2D>("Units\\Animations");
            loadFoldersArray<Texture2D>("GraphicsEffects");
        }

        static void loadFoldersArray<T1>(string folder)
        {
            foreach (string dir in System.IO.Directory.GetDirectories(System.IO.Path.Combine(
                ContentManager.RootDirectory, folder)))
            {
                List<T1> items = new List<T1>();
                foreach (string file in System.IO.Directory.GetFiles(dir))
                {
                    string fName = System.IO.Path.GetFileNameWithoutExtension(file);
                    items.Add(ContentManager.Load<T1>(
                        dir.Substring(dir.IndexOf('\\') + 1) + "\\" + fName));
                }
                Resources.Add(dir.Substring(dir.LastIndexOf('\\') + 1), items.ToArray());
            }
        }

        static void loadFolder<T1>(string folder)
        {
            foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.Combine(
                ContentManager.RootDirectory, folder)))
            {
                string fName = System.IO.Path.GetFileNameWithoutExtension(file);
                Resources.Add(fName, ContentManager.Load<T1>(
                    folder + "\\" + fName));
            }
        }

        static void loadVictories(string folder)
        {
            foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.Combine(
                ContentManager.RootDirectory, folder)))
            {
                string fName = System.IO.Path.GetFileNameWithoutExtension(file);
                UnitsAndBuilduings.VictoryTypeInfo info =  ContentManager.Load<UnitsAndBuilduings.VictoryTypeInfo>(
                    folder + "\\" + fName);
                Resources.Add(fName, MakeVicChecker(info));
            }
        }

        public static  VictoryChecker MakeVicChecker(UnitsAndBuilduings.VictoryTypeInfo info)
        {
            VictoryChecker vicChecker = new VictoryChecker();
            vicChecker.CheckTime = (CheckTime)info.CheckType;
            vicChecker.TimerTime = info.CheckTime;
            string winCode = "using System; using Omron; using Omron.Framework; using Omron.Actors; using System.Linq;" +
                "namespace Omron { public class CHKR { public static bool Win(World world, Faction faction) {" + info.WinCondition + "} } }";
            string loseCode = "using System; using Omron; using Omron.Framework; using Omron.Actors; using System.Linq;" +
                "namespace Omron { public class CHKR { public static bool Lose(World world, Faction faction) {" + info.LoseCondition + "} } }";
            vicChecker.FactionWon = getDelegate(winCode, "Win");
            vicChecker.FactionLost = getDelegate(loseCode, "Lose");
            return vicChecker;
        }

        #region code
        static System.Reflection.Assembly buildAssembly(string code)
        {
            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            List<string> assNames = new List<string>();
            assNames.Add("System.dll");
            assNames.Add("System.Core.dll");
            assNames.Add(System.Reflection.Assembly.GetCallingAssembly().Location);
            System.CodeDom.Compiler.CompilerParameters pars = new System.CodeDom.Compiler.CompilerParameters(assNames.ToArray());
            pars.GenerateExecutable = false;
            pars.GenerateInMemory = true;
            System.CodeDom.Compiler.CompilerResults res = provider.CompileAssemblyFromSource(pars, code);
            if (res.Errors.HasErrors)
            {
                foreach (System.CodeDom.Compiler.CompilerError err in res.Errors)
                {
                    Console.WriteLine(err.ErrorText);
                }
                throw new Exception("You better chech your victory checker code in the xml!");
            }
            System.Reflection.Assembly ass = res.CompiledAssembly;
            return ass;
        }

        static FactionWonDelegate getDelegate(string code, string meth)
        {
            System.Reflection.Assembly ass = buildAssembly(code);
            Type t = ass.GetType("Omron.CHKR");
            System.Reflection.MethodInfo method = t.GetMethod(meth);
            return new FactionWonDelegate(delegate(World world, Faction faction) { return (bool)method.Invoke(null, new object[2]{ world, faction}); });
        }
        #endregion

        /*public static SpriteData MakeSPDataFromPoly(IPolygon poly, Color sel, Color unsel)
        {
            FrameData selFrame = new FrameData();
            FrameData unselFrame;

            if (poly is RectPoly)
            {
                var rpoly = ((RectPoly)poly);
                selFrame.Image = Resources["pixel"];
                selFrame.SclWidth = 1f / rpoly.Width;
                selFrame.SclHeight = 1f / rpoly.Height;
            }
            else if (poly is HexPoly)
            {
                selFrame.Image = Resources["hexStroke"];
                selFrame.SetScale(selFrame.Image.Width / (poly.MaxRadius * 2));
            }

            selFrame.Color = sel;
            unselFrame = selFrame;
            unselFrame.Color = unsel;

            return new SpriteData(selFrame, unselFrame);
        }*/

        public static Texture2D GeneratePixelTex(Color color)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, 1, 1);
            tex.SetData<Color>(new Color[] { color });
            return tex;
        }
        public static Texture2D GenerateRectTex(Color color, int w, int h)
        {
            Texture2D tex = new Texture2D(GraphicsDevice, w, h);
            Color[] cData = new Color[w * h];
            for (int i = 0; i < w * h; i++)
                cData[i] = color;
            tex.SetData<Color>(cData);
            return tex;
        }
    }
}

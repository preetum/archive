using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace MainGame
{
    static class ResourceManager
    {
        public static Dictionary<String, dynamic> Resources { get; private set; }
        public static void LoadContent(ContentManager content)
        {
            Resources = new Dictionary<string, dynamic>();
            foreach (string rawfile in System.IO.Directory.GetFiles(content.RootDirectory))
            {
                var file = rawfile.Split(new char[] { '.', '\\' });
                var filename = file[1].Split(new char[] { '_' });
                switch (filename[1])
                {
                    case "tex": Resources.Add(filename[0], content.Load<Texture2D>(file[1])); break;
                    case "fx": Resources.Add(filename[0], content.Load<Effect>(file[1])); break;
                }
            }
        }
    }
}

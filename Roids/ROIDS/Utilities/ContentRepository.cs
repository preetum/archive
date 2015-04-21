using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Utilities
{
    public static class ContentRepository
    {
        public static Dictionary<string, dynamic> Repository { get; private set; }
        
        public static void LoadContent(ContentManager contentMgr)
        {
            Repository = new Dictionary<string, dynamic>();
            foreach (string file in Directory.GetFiles(contentMgr.RootDirectory))
            {
                string[] contentInfo = file.Split('\\','_','.');
                switch (contentInfo[2])
                {
                    case "img":
                        Repository.Add(contentInfo[1],
                            contentMgr.Load<Texture2D>(contentInfo[1] + "_" + contentInfo[2]));
                        break;
                    case "fnt":
                        Repository.Add(contentInfo[1],
                            contentMgr.Load<SpriteFont>(contentInfo[1] + "_" + contentInfo[2]));
                        break;

                    // Add New Extensions Here //

                    default:
                        break;
                }
            }            
        }
    }
}

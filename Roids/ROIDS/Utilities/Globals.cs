using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Utilities
{
    public static class Globals
    {
        public static Dictionary<string, dynamic> Data;
        public static void LoadGlobals(Game currentGame, SpriteBatch spriteBatch)
        {
            Data = new Dictionary<string, dynamic>()
            {
                {"Game", currentGame},                
                {"SpriteBatch", spriteBatch}
            };
        }

    }
}

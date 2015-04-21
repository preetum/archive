using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Utilities
{
    public static class DefaultSettings
    {
        public static Dictionary<String, dynamic> Settings { get; private set; }

        static DefaultSettings()
        {
            Settings = new Dictionary<String, Object>();
        }
        public static void LoadDefaultSettings()
        {
            // Later, values will be loaded from file! //
            Settings.Add("Font", "BasicFont");
            Settings.Add("Trivia Questions", false);
            Settings.Add("WindowSize", new Size(640, 480));
            Settings.Add("BackgroundColor", Color.Honeydew);
            Settings.Add("TextColor", Color.White);
            Settings.Add("PixelsPerMeter", 30f);
        }

    }
}

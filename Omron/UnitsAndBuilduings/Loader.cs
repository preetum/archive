using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace UnitsAndBuilduings
{
    public class Loader
    {//loads types of units, call in LoadContent()
        public static Dictionary<string, UnitTypeInfo> LoadUnits(ContentManager cm)
        {
            Dictionary<string, UnitTypeInfo> units = new Dictionary<string, UnitTypeInfo>();
            foreach (string file in Directory.GetFiles(Path.Combine(cm.RootDirectory, "Units\\Types\\")))
            {
                string path = file.Substring("Content\\".Length);
                path = path.Substring(0, path.Length - ".xnb".Length);
                string name = path.Substring(path.LastIndexOf('\\') + 1);
                units.Add(name, cm.Load<UnitTypeInfo>(path));
            }
            return units;
        }

        public static Dictionary<string, BuilduingTypeInfo> LoadBuildings(ContentManager cm)
        {
            Dictionary<string, BuilduingTypeInfo> buildings = new Dictionary<string, BuilduingTypeInfo>();
            foreach (string file in Directory.GetFiles(Path.Combine(cm.RootDirectory, "Buildings\\Types\\")))
            {
                string path = file.Substring("Content\\".Length);
                path = path.Substring(0, path.Length - ".xnb".Length);
                string name = path.Substring(path.LastIndexOf('\\') + 1);
                buildings.Add(name, cm.Load<BuilduingTypeInfo>(path));
            }
            return buildings;
        }

        public static Dictionary<string, dynamic> LoadSettings(ContentManager cm, string set)
        {
            Dictionary<string, dynamic> dic = new Dictionary<string, dynamic>();
            SettingsTypeInfo sets = cm.Load<SettingsTypeInfo>(set);
            foreach (Setting s in sets.Settings)
            {
                dic.Add(s.Key, s.Value);
            }
            return dic;
        }
    }
}

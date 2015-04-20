using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitsAndBuilduings
{
    public class Setting
    {
        public Setting() { }
        public string Key;
        public dynamic Value;
    }

    public class SettingsTypeInfo
    {
        public SettingsTypeInfo() { }

        public Setting[] Settings;
    }
}

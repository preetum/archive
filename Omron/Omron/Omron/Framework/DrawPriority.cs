using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omron.Framework
{
    public static class DrawPriority
    {
        public static float WallDepth = 0.4f;
        public static float UnitDepth = 0.3f;
        public static float BuildingDepth = 0.2f; //buildings are higher than units -- this could be a problem if a unit is in front of a building
    }
}

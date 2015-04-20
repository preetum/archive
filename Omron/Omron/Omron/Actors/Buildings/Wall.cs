using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using UnitsAndBuilduings;

namespace Omron.Actors
{
    public class Wall : FatherBuilding
    {
        public Wall(BuilduingTypeInfo info, string type, Vector2 loc)
            : base(info, type, loc)
        {
            DrawDepth = Omron.Framework.DrawPriority.WallDepth;
        }

        public override ActorMenu Menu
        {
            get
            {
                return base.Menu;
            }
        }
    }
}

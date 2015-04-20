using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using UnitsAndBuilduings;
using Omron.Framework;
using Omron.Framework.Networking;
using Lidgren.Network;

namespace Omron.Actors
{
    class Resource : FatherBuilding, IChangeUpdate
    {
        public Resource(BuilduingTypeInfo info, string type, Vector2 loc)
            : base(info, type, loc)
        {
            this.Attacked += new AttackedEventHandler(Resource_Attacked);
            if (info.SubTypeCosts.Length < 1)
                throw new Exception("Resources must have one SubTypeCosts broham!");
            resources = UnitConverter.CreateResourceData(info.SubTypeCosts[0]);

            menu = new ActorMenu(this);
            menu.Info = "     " + UnitConverter.CreateResourceDescription(resources) + "\n\nResource left: " + this.Health;

            IsInvalid = false;
            DrawDepth = DrawPriority.WallDepth;
        }

        ResourceData resources;
        ActorMenu menu;

        void Resource_Attacked(Framework.ArealAttack damage, Actor atacker)
        {
            atacker.Faction.Resources.AddResource(this.resources, damage.Damage.Mining * this.Defense.Mining, this.Health);

            IsInvalid = true;
            menu.Info = "     " + UnitConverter.CreateResourceDescription(resources) + "\n\nResource left: " + this.Health;
        }

        public override ActorMenu Menu
        {
            get
            {
                if (IsComplete)
                {
                    menu.Info = "     " + UnitConverter.CreateResourceDescription(resources) + "\n\nResource left: " + this.Health;
                    return menu;
                }
                return base.Menu;
            }
        }
        public override void UpdateFast(GameTime gameTime)
        {
            base.UpdateFast(gameTime);
        }

        public bool IsInvalid { get; set; }
        public void WriteOutChangeUpdateData(NetOutgoingMessage om)
        {
            base.WriteOutUpdateData(om);
        }
        public void ReadInChangeUpdateData(NetIncomingMessage im)
        {
            base.ReadInUpdateData(im);

            menu.Info = "     " + UnitConverter.CreateResourceDescription(resources) + "\n\nResource left: " + this.Health;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Omron;
using Omron.Actors;
using Omron.Framework;

namespace Omron.AI
{
    public enum BuildingAIState
    {
        Idleing,
        EngagingMelee,
        EngagingRange
    }
    public interface IBuildingAI
    {
        void UpdateSlow(GameTime gameTime);
        void UpdateFast(GameTime gameTime);
        void Engage(Actor enemy);
        BuildingAIState State { get; }
    }
}

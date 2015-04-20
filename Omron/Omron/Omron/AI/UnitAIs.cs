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
    public enum UnitAIState
    {
        Tracking,
        Holding,
        Pursuing,
        Building
    }

    public interface IUnitAI
    {
        void UpdateMotionFast(GameTime gameTime);
        void UpdateSlow(GameTime gameTime);
        void SetTarget(Vector2 pos);
        void SetTargetAggressive(Vector2 pos);
        void Build(FatherBuilding building);
        void Engage(Actor enemy);
        UnitAIState State { get; }
    }

    
}

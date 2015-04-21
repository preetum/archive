using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;

namespace ROIDS
{
    public static class KeyMappings
    {
        public enum SimpleGameControls
        {
            MoveUp = 1,
            MoveDown = 2,
            MoveRight = 3,
            MoveLeft = 4
        }

        public enum PolarMouseGameControls
        {
            MoveIn,
            MoveOut,
            MoveClockwise,
            MoveCounterClockwise,
            Shoot,
            ThrowSensor,
            BlastCharges,
        }

        public static Dictionary<Keys, int> PolarMouseMapping = new Dictionary<Microsoft.Xna.Framework.Input.Keys, int>()
            {
            {Keys.W, (int)PolarMouseGameControls.MoveIn},
            {Keys.S, (int)PolarMouseGameControls.MoveOut},
            {Keys.A, (int)PolarMouseGameControls.MoveClockwise},
            {Keys.D, (int)PolarMouseGameControls.MoveCounterClockwise},
            {Keys.E, (int)PolarMouseGameControls.Shoot},
            {Keys.F, (int)PolarMouseGameControls.ThrowSensor},
            {Keys.Space, (int)PolarMouseGameControls.BlastCharges}
            };

        public static Dictionary<Keys, int> SimpleMapping = new Dictionary<Microsoft.Xna.Framework.Input.Keys, int>()
            {
            {Keys.W, (int)SimpleGameControls.MoveUp},
            {Keys.S, (int)SimpleGameControls.MoveDown},
            {Keys.A, (int)SimpleGameControls.MoveLeft},
            {Keys.D, (int)SimpleGameControls.MoveRight},
            };
    }
}

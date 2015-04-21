using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;
namespace ROIDS.Demos
{    

        public static class DemoKeyMappings
        {
            // Sample Key Mappings
            public static Dictionary<Keys, int> ArrowMapping = new Dictionary<Microsoft.Xna.Framework.Input.Keys, int>()
             {
            {Keys.Up, (int)SimpleGameControls.MoveUp},
            {Keys.Down, (int)SimpleGameControls.MoveDown},
            {Keys.Right, (int)SimpleGameControls.MoveRight},
            {Keys.Left, (int)SimpleGameControls.MoveLeft},
             };

            // Sample Key Mappings
            public static Dictionary<Keys, int> WasdMapping = new Dictionary<Microsoft.Xna.Framework.Input.Keys, int>()
            {
            {Keys.W, (int)SimpleGameControls.MoveUp},
            {Keys.S, (int)SimpleGameControls.MoveDown},
            {Keys.D, (int)SimpleGameControls.MoveRight},
            {Keys.A, (int)SimpleGameControls.MoveLeft},
            };

            // Controls in Game - mapped to keys (see Load() for setup)
            public enum SimpleGameControls
            {
                MoveUp = 1,
                MoveDown = 2,
                MoveRight = 3,
                MoveLeft = 4
            }
        }
}

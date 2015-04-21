using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using GameCore;
using WorldCore;
using UICore;
using PhysicsCore;
using Utilities;

namespace ROIDS.Demos.Swarm_PC
{
    [Demo]
    class SwarmGame_PC : GameState, GameStates.IPlayable
    {

        public Map ActiveMap { get; set; }
        public PhysicsEngine PhysicsManager { get; private set; }
        public UIEngine UIManager { get; private set; }

        public void Reload() { /* Loading Maps for game */ }
        public void EndGame() { /* Saving and cleanup before switching maps */ }
        
        public override void Load() 
        { 
            /* initialize game state */
            UIManager = new UIEngine();

            var frame = new UIFrame();
            frame.KeyUp += new KeyEventHandler(frame_KeyUp);
            UIManager.AddAndLoad(frame);
        }

        void frame_KeyUp(GUIElement sender, KeyEventArgs e)
        {
            if (e.InterestingKeys.Contains<Keys>(Keys.Escape))
                ((UIFrame)sender).Close();
                // or: UIManager.ActiveFrame.Close()
        }
        public override void Update(GameTime time) 
        {
            if (!UIManager.Update(time))
                this.Exit();
        }
        public override void Draw(GameTime t, SpriteBatch spriteBatch) {/* Graphics */}
        public override void Destroy() { /* Optional Cleanup */ }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ROIDS;
using PhysicsCore;
using WorldCore;

using Microsoft.Xna.Framework;

namespace ROIDS.Demos.Swarm
{
    class Treasure : GameObjects.Actor, ICircleBody
    {
         public Treasure(Vector2 position, float orientation)
            : base(position, orientation)
        {
            Radius = 5;
            HasInfiniteMass = true;
            _treasureColor = Color.Gold;
        }

        /*
         * Physics Reqs
         */
        public override PhysicsCore.Region BoundingBox
        {
            get { return Region.FromCircle(Position, Radius); }
        }
        public float Radius { get; set; }
        /*
         * Graphics Reqs
         */
        public override void Draw(GameTime time, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Utilities.GraphicsUtils.DrawBall(Position, Radius, _treasureColor, 255);
        }

        Color _treasureColor;

        /*
         * World Reqs
         */
        public override void Destroy()
        {            
            ((SwarmGame)GameCore.GameEngine.Singleton.ActiveState).PlayerManger.Points += 10;
            base.Destroy();
        }

        /*
         * Droid Logic
         */

    }
}

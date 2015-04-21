using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PhysicsCore;
using Microsoft.Xna.Framework;

namespace ROIDS.Demos.Swarm
{
    class Player : GameObjects.Actor, ICircleBody, WorldCore.IPlayer
    {

        public Player(Vector2 position, float orientation)
            : base(position, orientation)
        {
            Radius = 10;
            _playerColor = Color.Blue;
            Speed = 2;
            //CollisionEngine.Collided += new LegacyCollisionEventHandler(CollisionEngine_Collided);
            this.Collided += new CollisionEventHandler(Player_Collided);
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
            Utilities.GraphicsUtils.DrawBall(Position, Radius, _playerColor, 255);
        }
        Color _playerColor;

        /*
         * World Reqs
         */
        public void Update(Microsoft.Xna.Framework.GameTime time)
        {
            // Do nothing
        }

        /*
         * Player Logic
         */

        void Player_Collided(IRigidBody impactB)
        {
            //    if (body1 is Treasure && body2 == this)
            //        ((GameObjects.Actor)body1).Destroy();
            if (impactB is Treasure)
                ((GameObjects.Actor)impactB).Destroy();
        }

        public int Speed;
    }
}

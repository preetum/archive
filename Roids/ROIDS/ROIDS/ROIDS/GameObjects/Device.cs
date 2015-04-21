using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WorldCore;
using PhysicsCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ROIDS.GameObjects
{
    public abstract class Device : WorldObject
    {
        public Actor Parent { get; set; }
        public virtual bool Suppressed { get; set; }

        public override Vector2 Position
        {
            get
            {
                if (Parent != null)
                    return Parent.Position;
                return Vector2.Zero;
            }
        }

        public override float Rotation
        {
            get
            {
                if (Parent != null)
                    return Parent.Rotation;
                return 0.0f;
            }
        }

        public Device(Actor parent)
            : base(parent.Position, 0f)
        {
            Parent = parent;
        }

        public Device()
            : base(Vector2.Zero, 0f)
        {
        }

        public override void Destroy()
        {
            var game = (GameStates.PlayableState)GameCore.GameEngine.Singleton
                .FindGameState(x => x is GameStates.PlayableState);
            game.KillObject(this);

            base.Destroy();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PhysicsCore;
using WorldCore;
using UICore;

namespace ROIDS.GameStates
{
    abstract class PlayableState : GameCore.GameState
    {
        public Map ActiveMap { get; protected set; }
        public PhysicsEngine PhysicsManager { get; protected set; }
        public UIEngine UIManager { get; protected set; }
        public PlayerProfile PlayerProfile { get; protected set; }

        public abstract void Reload();
        public abstract void EndGame();
        public virtual void KillObject<T>(T obj)
        {
            ActiveMap.SafelyRemoveObject(obj as WorldObject);
            if (obj is IRigidBody)
                PhysicsManager.SafeDelete(obj as IRigidBody);
        }

        public virtual void SpawnObject<T>(T obj)
        {
            ActiveMap.AddObject(obj as WorldObject);
            if (obj is IRigidBody)
                PhysicsManager.ActiveBodies.Add(obj as IRigidBody);
        }
    }
}
